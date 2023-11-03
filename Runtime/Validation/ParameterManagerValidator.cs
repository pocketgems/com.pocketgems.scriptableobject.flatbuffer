using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation.Attributes;
using UnityEngine;

namespace PocketGems.Parameters.Validation
{
    /// <summary>
    /// Class that validates data for a data loaded parameterManager.  The manager finds all IParameterValidator objects
    /// in the, project and constructs them.  These IParameterValidator are used to validate the parameterManager.
    /// </summary>
    public class ParameterManagerValidator
    {
        /// <summary>
        /// Current errors identified through Validate<T>() calls.
        /// </summary>
        public IReadOnlyList<ValidationError> ValidationErrors
        {
            get
            {
                List<ValidationError> errors = new List<ValidationError>(_errors);
                foreach (var dataValidator in _dataValidatorInstances.Values)
                    if (dataValidator.Errors != null && dataValidator.Errors.Count > 0)
                        errors.AddRange(dataValidator.Errors);
                foreach (var dataValidator in _dataValidatorStructInstances.Values)
                    if (dataValidator.Errors != null && dataValidator.Errors.Count > 0)
                        errors.AddRange(dataValidator.Errors);
                errors.AddRange(ParameterManagerValidatorCache.Errors);
                return errors;
            }
        }

        /// <summary>
        /// Constructor of the ParameterManagerValidator
        /// </summary>
        /// <param name="parameterManager">parameterManager to validate</param>
        public ParameterManagerValidator(IParameterManager parameterManager)
        {
            ParameterManagerValidatorCache.Initialize();

            _parameterManager = parameterManager;
            _errors = new List<ValidationError>();

            // auto added attributes - add any others here
            _builtInAttributesCreators =
                new Func<IValidationAttribute>[] { () => new AssertAssignedReferenceExistsAttribute() };
            _builtInAttributesInstancePool = new IValidationAttribute[_builtInAttributesCreators.Length];
            for (int i = 0; i < _builtInAttributesCreators.Length; i++)
                _builtInAttributesInstancePool[i] = _builtInAttributesCreators[i].Invoke();

            // validators
            _dataValidatorInstances = new Dictionary<Type, IDataValidator>();
            _dataValidatorStructInstances = new Dictionary<Type, IDataValidatorStruct>();
            _dataValidatorVerifiedParameterManagerSet = new HashSet<IDataValidator>();
            _verifiedInfosMap = new Dictionary<IDataValidator, HashSet<IBaseInfo>>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validate one individual object.  This is only used from the custom inspector.
        /// </summary>
        /// <param name="obj">object to validate</param>
        /// <typeparam name="T">the interface type of the object</typeparam>
        public void ValidateObject<T>(T obj) where T : class, IBaseInfo
        {
            var objDataList = new List<ValidationObjectData> { new ValidationObjectData(typeof(T), obj) };
            ValidateAttributes(typeof(T), objDataList, true);
            var objList = new List<T> { obj };
            ValidateStructs(objList);
            ValidateInfoDataValidators(objList, true, false);
        }
#endif

        /// <summary>
        /// Validate for the specific T
        /// </summary>
        /// <typeparam name="T">the interface type to validate</typeparam>
        public void Validate<T>() where T : class, IBaseInfo
        {
            var infos = _parameterManager.Get<T>().ToList();
            List<ValidationObjectData> objectDatas = new List<ValidationObjectData>();
            for (int i = 0; i < infos.Count; i++)
                objectDatas.Add(new ValidationObjectData(typeof(T), infos[i]));
            ValidateAttributes(typeof(T), objectDatas, false);
            ValidateStructs(infos);
            ValidateInfoDataValidators(infos, false, true);
        }

        private readonly IParameterManager _parameterManager;
        private readonly List<ValidationError> _errors;

        // built in attributes
        private readonly Func<IValidationAttribute>[] _builtInAttributesCreators;
        private readonly IValidationAttribute[] _builtInAttributesInstancePool;

        // book keeping of data validators
        private readonly Dictionary<Type, IDataValidator> _dataValidatorInstances;
        private readonly Dictionary<Type, IDataValidatorStruct> _dataValidatorStructInstances;
        private readonly HashSet<IDataValidator> _dataValidatorVerifiedParameterManagerSet;
        private readonly Dictionary<IDataValidator, HashSet<IBaseInfo>> _verifiedInfosMap;

        private void ValidateInfoDataValidators<T>(List<T> infos, bool recurseInterfaces, bool callValidateParameters) where T : class, IBaseInfo
        {
            IReadOnlyList<Type> interfacesType;
            if (recurseInterfaces)
                interfacesType = ParameterManagerValidatorCache.GetAllInterfaceTypes(typeof(T));
            else
                interfacesType = new List<Type> { typeof(T) };

            for (int i = 0; i < interfacesType.Count; i++)
            {
                // get or create DataValidator
                var interfaceType = interfacesType[i];
                if (!_dataValidatorInstances.TryGetValue(interfaceType, out IDataValidator dataValidator))
                {
                    var dataValidators = ParameterManagerValidatorCache.GetDataValidatorMap();
                    if (!dataValidators.ContainsKey(interfaceType))
                    {
                        var error = new ValidationError(typeof(T), null, null, $"Cannot find validator for {interfaceType}");
                        _errors.Add(error);
                        continue;
                    }

                    dataValidator = (IDataValidator)Activator.CreateInstance(dataValidators[interfaceType]);
                    _dataValidatorInstances[interfaceType] = dataValidator;
                }

                // call ValidateParameters if needed
                if (callValidateParameters && !_dataValidatorVerifiedParameterManagerSet.Contains(dataValidator))
                {
                    _dataValidatorVerifiedParameterManagerSet.Add(dataValidator);
                    try
                    {
                        dataValidator.ValidateParameters(_parameterManager);
                    }
                    catch (Exception e)
                    {
                        var error = new ValidationError(typeof(T), null, null, $"{dataValidator.GetType().Name} threw an exception when calling {nameof(IDataValidator.ValidateParameters)}. see console.");
                        _errors.Add(error);
                        Debug.LogError(e);
                    }
                }

                // get verified set
                if (!_verifiedInfosMap.TryGetValue(dataValidator, out HashSet<IBaseInfo> verifiedInfos))
                {
                    verifiedInfos = new HashSet<IBaseInfo>();
                    _verifiedInfosMap[dataValidator] = verifiedInfos;
                }

                for (int j = 0; j < infos.Count; j++)
                {
                    var info = infos[j];
                    // already verified this object if it has multiple parameter interface implementations
                    if (verifiedInfos.Contains(info))
                        continue;
                    verifiedInfos.Add(info);
                    try
                    {
                        dataValidator.ValidateInfo(_parameterManager, info);
                    }
                    catch (Exception e)
                    {
                        var error = new ValidationError(typeof(T), info.Identifier, null,
                            $"{dataValidator.GetType().Name} threw an exception when calling {nameof(IDataValidator.ValidateInfo)}. see console.");
                        _errors.Add(error);
                        Debug.LogError(e);
                    }
                }
            }
        }

        private void ValidateStructDataValidators(Type structType, List<ValidationObjectData> structObjectDatas)
        {
            IReadOnlyList<Type> interfacesType = ParameterManagerValidatorCache.GetAllInterfaceTypes(structType);

            for (int i = 0; i < interfacesType.Count; i++)
            {
                // get or create DataValidator
                var interfaceType = interfacesType[i];
                if (!_dataValidatorStructInstances.TryGetValue(interfaceType, out IDataValidatorStruct dataValidator))
                {
                    var dataValidators = ParameterManagerValidatorCache.GetDataValidatorMap();
                    if (!dataValidators.ContainsKey(interfaceType))
                    {
                        var error = new ValidationError(structType, null, null, $"Cannot find validator for {interfaceType}");
                        _errors.Add(error);
                        continue;
                    }

                    dataValidator = (IDataValidatorStruct)Activator.CreateInstance(dataValidators[interfaceType]);
                    _dataValidatorStructInstances[interfaceType] = dataValidator;
                }

                for (int j = 0; j < structObjectDatas.Count; j++)
                {
                    var structObjectData = structObjectDatas[j];
                    try
                    {
                        dataValidator.ValidateStruct(_parameterManager, structObjectData);
                    }
                    catch (Exception e)
                    {
                        var error = new ValidationError(
                            structObjectData.InfoType,
                            structObjectData.Info.Identifier,
                            structObjectData.StructParentInfoReferenceProperty,
                            $"{dataValidator.GetType().Name} threw an exception when calling {nameof(IDataValidatorStruct.ValidateStruct)}. see console.",
                            structObjectData.StructKeyPath);
                        _errors.Add(error);
                        Debug.LogError(e);
                    }
                }
            }
        }

        private void ValidateStructs<T>(List<T> infos) where T : class, IBaseInfo
        {
            // holds all the structs reachable from the infos
            var typeToVisitedStructs = new Dictionary<Type, List<ValidationObjectData>>();

            // objects to the collect struct objects from
            Dictionary<Type, List<ValidationObjectData>> objectsToAnalyze = new Dictionary<Type, List<ValidationObjectData>>();

            /*
             * populate objectsToAnalyze with infos first
             */
            var infoValidationObjectDatas = new List<ValidationObjectData>();
            objectsToAnalyze[typeof(T)] = infoValidationObjectDatas;
            foreach (var info in infos)
                infoValidationObjectDatas.Add(new ValidationObjectData(typeof(T), info));

            /*
             * BFS through all objects to grab every struct utilized by the infos & structs referenced by infos
             */
            while (objectsToAnalyze.Count > 0)
            {
                var currentStructsAnalyzed = objectsToAnalyze;
                objectsToAnalyze = new Dictionary<Type, List<ValidationObjectData>>();

                foreach (var kvp in currentStructsAnalyzed)
                {
                    var type = kvp.Key;
                    var validationDataObjects = kvp.Value;

                    if (!typeToVisitedStructs.TryGetValue(type, out var visitedStructs))
                    {
                        visitedStructs = new List<ValidationObjectData>();
                        typeToVisitedStructs[type] = visitedStructs;
                    }

                    var getAllInterfaces = ParameterManagerValidatorCache.GetAllInterfaceTypes(type);
                    for (int j = 0; j < validationDataObjects.Count; j++)
                    {
                        var validationDataObject = validationDataObjects[j];
                        visitedStructs.Add(validationDataObject);

                        for (int i = 0; i < getAllInterfaces.Count; i++)
                        {
                            var referenceInfos = ParameterManagerValidatorCache.GetStructReferenceInfos(getAllInterfaces[i]);
                            for (int k = 0; k < referenceInfos.Count; k++)
                            {
                                var referenceInfo = referenceInfos[k];
                                var propertyInfo = referenceInfo.ReferencePropertyInfo;
                                var methodInfo = referenceInfo.ReferenceStructGetter;

                                // default values for iterating over a struct
                                object obj = validationDataObject.Struct;
                                string parentInfoReferenceProperty = validationDataObject.StructParentInfoReferenceProperty;
                                if (obj == null)
                                {
                                    // default values for iterating over an info
                                    obj = validationDataObject.Info;
                                    parentInfoReferenceProperty = propertyInfo.Name;
                                }

                                if (!objectsToAnalyze.TryGetValue(referenceInfo.ReferenceGenericType, out var structObjectDatas))
                                {
                                    structObjectDatas = new List<ValidationObjectData>();
                                    objectsToAnalyze[referenceInfo.ReferenceGenericType] = structObjectDatas;
                                }

                                var propertyValue = propertyInfo.GetValue(obj);

                                var args = new object[] { _parameterManager };
                                void ProcessStructReference(object structReference, string structKeyPath)
                                {
                                    if (structReference == null)
                                    {
                                        var error = new ValidationError(
                                            validationDataObject.InfoType,
                                            validationDataObject.Info.Identifier,
                                            parentInfoReferenceProperty,
                                            $"null {nameof(ParameterStructReference)}",
                                            structKeyPath);
                                        _errors.Add(error);
                                        return;
                                    }

                                    // ParameterStructReference<>
                                    object structObj = methodInfo.Invoke(structReference, args);
                                    if (structObj == null)
                                    {
                                        var error = new ValidationError(
                                            validationDataObject.InfoType,
                                            validationDataObject.Info.Identifier,
                                            parentInfoReferenceProperty,
                                            $"null struct in {nameof(ParameterStructReference)}",
                                            structKeyPath);
                                        _errors.Add(error);
                                        return;
                                    }

                                    var childStructObjectData = new ValidationObjectData(
                                        validationDataObject.InfoType,
                                        validationDataObject.Info,
                                        parentInfoReferenceProperty,
                                        structKeyPath,
                                        (IBaseStruct)structObj);
                                    structObjectDatas.Add(childStructObjectData);
                                }

                                if (!referenceInfo.IsList)
                                {
                                    var structKeyPath = propertyInfo.Name;
                                    if (validationDataObject.StructKeyPath != null)
                                        structKeyPath = $"{validationDataObject.StructKeyPath}.{structKeyPath}";
                                    ProcessStructReference(propertyValue, structKeyPath);
                                }
                                else
                                {
                                    int count = 0;
                                    // IReadOnlyList<ParameterStructReference<>>
                                    var enumerable = (IEnumerable)propertyValue;
                                    foreach (var element in enumerable)
                                    {
                                        var structKeyPath = propertyInfo.Name;
                                        if (validationDataObject.StructKeyPath != null)
                                            structKeyPath = $"{validationDataObject.StructKeyPath}.{structKeyPath}[{count}]";
                                        else
                                            structKeyPath = $"{structKeyPath}[{count}]";
                                        ProcessStructReference(element, structKeyPath);
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // validate structs in groups by type
            foreach (var kvp in typeToVisitedStructs)
            {
                if (!typeof(IBaseStruct).IsAssignableFrom(kvp.Key))
                    continue;

                List<object> objectList = new List<object>();
                for (int i = 0; i < kvp.Value.Count; i++)
                    objectList.Add(kvp.Value[i].Struct);
                ValidateAttributes(kvp.Key, kvp.Value, true);
                ValidateStructDataValidators(kvp.Key, kvp.Value);
            }
        }

        // iterate through and collect all structs recursively?
        // parse all the structs by type in the end?
        // - attribute
        // - validators
        private void ValidateAttributes(Type type, List<ValidationObjectData> validationObjectDatas, bool recurseInterfaces)
        {
            List<(PropertyInfo, IValidationAttribute)> checks = new List<(PropertyInfo, IValidationAttribute)>();
            HashSet<Type> visited = new HashSet<Type>();

            void GetAttributes(Type type)
            {
                if (visited.Contains(type))
                    return;

                visited.Add(type);
                PropertyInfo[] props = type.GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];

                    // find developer assigned validation attributes
                    object[] attrs = prop.GetCustomAttributes(true);
                    for (int j = 0; j < attrs.Length; j++)
                    {
                        if (attrs[j] is IValidationAttribute vAttr)
                        {
                            bool canValidate = false;
                            try
                            {
                                canValidate = vAttr.CanValidate(prop);
                            }
                            catch (Exception e)
                            {
                                var error = new ValidationError(type, null, prop.Name,
                                    $"{vAttr.GetType().Name} threw an exception when calling {nameof(IValidationAttribute.CanValidate)}. see console.");
                                _errors.Add(error);
                                Debug.LogError(e);
                                continue;
                            }

                            if (canValidate)
                            {
                                try
                                {
                                    vAttr.WillValidateProperty(_parameterManager, prop);
                                }
                                catch (Exception e)
                                {
                                    var error = new ValidationError(type, null, prop.Name,
                                        $"{vAttr.GetType().Name} threw an exception when calling {nameof(IValidationAttribute.WillValidateProperty)}. see console.");
                                    _errors.Add(error);
                                    Debug.LogError(e);
                                    continue;
                                }
                                checks.Add((prop, vAttr));
                            }
                            else
                            {
                                var error = new ValidationError(type, null, prop.Name, $"Attribute [{vAttr.GetType().Name}] cannot be assigned to {prop.Name}.  It is not compatible with type {prop.PropertyType}.");
                                _errors.Add(error);
                            }
                        }
                    }

                    // add auto assigned attributes
                    for (int j = 0; j < _builtInAttributesInstancePool.Length; j++)
                    {
                        var autoAddedAttribute = _builtInAttributesInstancePool[j];
                        if (autoAddedAttribute.CanValidate(prop))
                        {
                            autoAddedAttribute.WillValidateProperty(_parameterManager, prop);
                            checks.Add((prop, autoAddedAttribute));

                            // repopulate the pool with a new instance
                            _builtInAttributesInstancePool[j] = _builtInAttributesCreators[j].Invoke();
                        }
                    }
                }

                if (!recurseInterfaces)
                    return;

                var interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                    GetAttributes(interfaces[i]);
            }

            // get properties and attribute pairings
            GetAttributes(type);

            // apply validation to property values
            foreach (var validationObjectData in validationObjectDatas)
            {
                PropertyInfo propertyInfo = null;
                object propertyValue = null;
                object objectToValidate;

                bool isValidatingStruct = validationObjectData.Struct != null;
                if (isValidatingStruct)
                    objectToValidate = validationObjectData.Struct;
                else
                    objectToValidate = validationObjectData.Info;


                void AddError(string message)
                {
                    string infoPropertyName = propertyInfo.Name;
                    string structPropertyName = null;
                    if (isValidatingStruct)
                    {
                        infoPropertyName = validationObjectData.StructParentInfoReferenceProperty;
                        structPropertyName = propertyInfo.Name;
                    }
                    var error = new ValidationError(
                        validationObjectData.InfoType,
                        validationObjectData.Info.Identifier,
                        infoPropertyName,
                        message,
                        validationObjectData.StructKeyPath,
                        structPropertyName);
                    _errors.Add(error);
                }

                for (int i = 0; i < checks.Count; i++)
                {
                    (var newPropertyInfo, var attribute) = checks[i];
                    if (newPropertyInfo != propertyInfo)
                    {
                        propertyInfo = newPropertyInfo;
                        propertyValue = propertyInfo.GetValue(objectToValidate);
                    }

                    string errorMessage = null;
                    try
                    {
                        errorMessage = attribute.Validate(_parameterManager, propertyInfo, propertyValue);
                    }
                    catch (Exception e)
                    {
                        AddError(
                            $"{attribute.GetType().Name} threw an exception when calling {nameof(IValidationAttribute.Validate)}. see console.");
                        Debug.LogError(e);
                        continue;
                    }

                    if (string.IsNullOrEmpty(errorMessage))
                        continue;
                    AddError(errorMessage);
                }
            }
        }
    }
}
