using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PocketGems.Parameters.Interface;
using UnityEngine;

namespace PocketGems.Parameters
{
    /// <summary>
    /// Implementation fo IMutableParameterManager to add, remove, and query Flatbuffer objects.
    /// </summary>
    public class ParameterManager : IMutableParameterManager
    {
        /// <summary>
        /// General constructor.
        /// </summary>
        public ParameterManager()
        {
            IsGettingSafe = true;
            _overriddenParameters = new HashSet<IMutableParameter>();
            _identifierMappings = new Dictionary<string, Dictionary<string, IMutableParameter>>();
            _guidMappings = new Dictionary<string, Dictionary<string, IMutableParameter>>();
        }

        #region IMutableParameterManager

        /// <inheritdoc cref="IMutableParameterManager.Load{T}"/>
        public void Load<T, G>(G parameter, string identifier, string guid) where T : class, IBaseInfo
                                                                            where G : T, IMutableParameter
        {
            LoadInfo(typeof(T), parameter, identifier, guid);
        }

        public void Load<T, G>(G parameter, string guid) where T : class, IBaseStruct
                                                         where G : T, IMutableParameter
        {
            LoadStruct(typeof(T), parameter, guid);
        }

        /// <inheritdoc cref="IMutableParameterManager.RemoveAll"/>
        public void RemoveAll()
        {
            _identifierMappings.Clear();
            _guidMappings.Clear();
        }

        /// <inheritdoc cref="IMutableParameterManager.ApplyOverrides"/>
        public bool ApplyOverrides(JObject json, out IReadOnlyList<string> errors)
        {
            if (json == null || !json.HasValues)
            {
                errors = null;
                return true;
            }

            List<string> localErrors = null;
            bool success = true;
            void Error(string error)
            {
                success = false;
                if (localErrors == null)
                    localErrors = new List<string>();
                localErrors.Add(error);
            }

            JArray edits = null;
            const string kAddKey = "add";
            const string kDeleteKey = "delete";
            const string kEditKey = "edit";
            foreach (var overridesToken in json.Children())
            {
                var action = (JProperty)overridesToken;
                if (action.Name == kEditKey)
                    edits = (JArray)action.Value;
                else if (action.Name == kAddKey || action.Name == kDeleteKey)
                    Error($"Override action {action.Name} isn't supported yet.");
                else
                    Error($"Override action {action.Name} isn't valid.");
            }

            if (edits != null)
            {
                foreach (var dataOverride in edits)
                {
                    var array = (JArray)dataOverride;
                    if (array.Count != 4)
                    {
                        Error("ParameterManager expects 4 elements in the array of data.");
                        break;
                    }

                    var csvName = (string)array[0];
                    var identifierOrGuid = (string)array[1];
                    var propertyName = (string)array[2];
                    var value = (string)array[3];

                    var interfaceType = LocalCSV.CSVUtil.CSVToInterfaceFileName(csvName);
                    IMutableParameter mutableParameter;
                    mutableParameter = Get(interfaceType, identifierOrGuid) ??
                                       GetWithGUID(interfaceType, identifierOrGuid);
                    if (mutableParameter == null)
                    {
                        Error($"Cannot find parameter for csv [{csvName}] and identifier/guid [{identifierOrGuid}].");
                        continue;
                    }

                    _overriddenParameters.Add(mutableParameter);
                    if (!mutableParameter.EditProperty(propertyName, value, out string error))
                        Error(
                            $"Error editing ({interfaceType})[{identifierOrGuid}] property [{propertyName}] with value [{value}]: {error}");
                }
            }

            errors = localErrors;
            return success;
        }

        /// <inheritdoc cref="IMutableParameterManager.ClearAllOverrides"/>
        public void ClearAllOverrides()
        {
            foreach (var info in _overriddenParameters)
                info.RemoveAllEdits();
            _overriddenParameters.Clear();
        }

        #endregion

        #region IParameterManager

        /// <inheritdoc cref="IParameterManager.Get{T}(string)"/>
        public T Get<T>(string identifier) where T : class, IBaseInfo
        {
            CheckGet();
            return (T)Get(typeof(T), identifier);
        }

        /// <inheritdoc cref="IParameterManager.GetWithGUID{T}(string)"/>
        public T GetWithGUID<T>(string guid) where T : class, IBaseInfo
        {
            CheckGet();
            return (T)GetWithGUID(typeof(T), guid);
        }

        /// <inheritdoc cref="IParameterManager.GetStructWithGuid{T}"/>
        public T GetStructWithGuid<T>(string guid) where T : class, IBaseStruct
        {
            CheckGet();
            return (T)GetWithGUID(typeof(T), guid);
        }

        /// <inheritdoc cref="IParameterManager.Get{T}()"/>
        public IEnumerable<T> Get<T>() where T : class, IBaseInfo
        {
            CheckGet();
            var type = typeof(T);
            if (type == typeof(IBaseInfo))
            {
                Debug.LogError($"Cannot use {nameof(IBaseInfo)} as type.");
                yield break;
            }

            if (_identifierMappings.TryGetValue(type.Name, out Dictionary<string, IMutableParameter> parameters))
            {
                foreach (var kvp in parameters)
                    yield return (T)kvp.Value;
            }
        }

        /// <inheritdoc cref="IParameterManager.GetSorted{T}()"/>
        public IEnumerable<T> GetSorted<T>() where T : class, IBaseInfo
        {
            CheckGet();
            var type = typeof(T);
            if (type == typeof(IBaseInfo))
            {
                Debug.LogError($"Cannot use {nameof(IBaseInfo)} as type.");
                yield break;
            }

            if (_identifierMappings.TryGetValue(type.Name, out Dictionary<string, IMutableParameter> parameters))
            {
                var sortedKeys = parameters.Keys.OrderBy(x => x);
                foreach (var key in sortedKeys)
                    yield return (T)parameters[key];
            }
        }

        /// <inheritdoc cref="IParameterManager.IsGettingSafe()"/>
        public bool IsGettingSafe { get; set; }

        /// <inheritdoc cref="IParameterManager.HasGetBeenCalled()"/>
        public bool HasGetBeenCalled { get; internal set; }

        #endregion

        private readonly HashSet<IMutableParameter> _overriddenParameters;
        private readonly Dictionary<string, Dictionary<string, IMutableParameter>> _identifierMappings;
        private readonly Dictionary<string, Dictionary<string, IMutableParameter>> _guidMappings;

        #region private methods
        /*
         * private methods to optimize the public generic methods
         * https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generics-in-the-run-time
         */

        private void LoadInfo(Type type, IMutableParameter parameter, string identifier, string guid)
        {
            if (type == typeof(IBaseInfo))
            {
                Debug.LogError($"Cannot use {nameof(IBaseInfo)} as type.");
                return;
            }

            if (!_identifierMappings.TryGetValue(type.Name, out Dictionary<string, IMutableParameter> idDict))
            {
                idDict = new Dictionary<string, IMutableParameter>();
                _identifierMappings[type.Name] = idDict;
            }
            bool updatingExistingIdentifier = idDict.ContainsKey(identifier);


            if (!_guidMappings.TryGetValue(type.Name, out Dictionary<string, IMutableParameter> guidDict))
            {
                guidDict = new Dictionary<string, IMutableParameter>();
                _guidMappings[type.Name] = guidDict;
            }
            bool updatingExistingGuid = guidDict.ContainsKey(guid);

            // This would only occur while trying to drastically author params in Editor while hot loading.
            // Currently not supporting renaming of parameters or creating new assets at Editor Runtime.
            if (updatingExistingIdentifier != updatingExistingGuid)
            {
                Debug.LogError("Loading parameter doesn't match existing mappings.");
                return;
            }

            idDict[identifier] = parameter;
            guidDict[guid] = parameter;
        }

        private void LoadStruct(Type type, IMutableParameter parameter, string guid)
        {
            if (type == typeof(IBaseStruct))
            {
                Debug.LogError($"Cannot use {nameof(IBaseStruct)} as type.");
                return;
            }

            if (!_guidMappings.TryGetValue(type.Name, out Dictionary<string, IMutableParameter> guidDict))
            {
                guidDict = new Dictionary<string, IMutableParameter>();
                _guidMappings[type.Name] = guidDict;
            }

            guidDict[guid] = parameter;
        }

        public IBaseInfo Get(string identifier, Type type)
        {
            CheckGet();
            return (IBaseInfo)Get(type, identifier);
        }

        private IMutableParameter Get(Type type, string identifier)
        {
            if (type == typeof(IBaseInfo))
            {
                Debug.LogError($"Cannot use {nameof(IBaseInfo)} as type.");
                return null;
            }

            return Get(type.Name, identifier);
        }

        private IMutableParameter Get(string type, string identifier)
        {
            if (!_identifierMappings.TryGetValue(type, out Dictionary<string, IMutableParameter> parameters))
                return null;
            if (!parameters.TryGetValue(identifier, out IMutableParameter parameter))
                return null;
            return parameter;
        }

        private IMutableParameter GetWithGUID(Type type, string guid)
        {
            if (type == typeof(IBaseInfo) || type == typeof(IBaseStruct))
            {
                Debug.LogError(
                    $"Cannot use {nameof(IBaseInfo)} or {nameof(IBaseStruct)} as type.");
                return null;
            }

            return GetWithGUID(type.Name, guid);
        }

        private IMutableParameter GetWithGUID(string typeName, string guid)
        {
            if (!_guidMappings.TryGetValue(typeName, out Dictionary<string, IMutableParameter> parameters))
            {
                Debug.LogError($"Missing: Cannot find parameter by GUID {guid} for type {typeName}");
                return null;
            }
            if (!parameters.TryGetValue(guid, out IMutableParameter parameter))
            {
                Debug.LogError($"Bug: Cannot find parameter by GUID {guid} for type {typeName}");
                return null;
            }

            return parameter;
        }

        // We need to call this every time a parameter is retrieved with one of the public Get methods above.
        private void CheckGet()
        {
            if (!IsGettingSafe)
                Debug.LogError($"Parameters cannot be read while {nameof(IsGettingSafe)} is false. " +
                    $"Check the ordering of the startup steps to ensure that {nameof(IsGettingSafe)} " +
                    $"is set to true before {nameof(IParameterManager)}'s GetX methods are used.");
            HasGetBeenCalled = true;
        }

        #endregion
    }
}
