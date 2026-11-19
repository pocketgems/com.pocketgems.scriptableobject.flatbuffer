using System;
using System.Collections.Generic;
using System.Linq;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Util;
using UnityEngine;

namespace PocketGems.Parameters
{
    /// <summary>
    /// Implementation fo IMutableParameterManager to add, remove, and query Flatbuffer objects.
    /// </summary>
    public partial class ParameterManager : IMutableParameterManager
    {
        #region Constructors

        /// <summary>
        /// General public constructor.
        /// </summary>
        public ParameterManager() : this(new(), new()) { }

        /// <summary>
        /// internal constructor.
        /// </summary>
        protected ParameterManager(
            Dictionary<string, Dictionary<string, IMutableParameter>> identifierMappings,
            Dictionary<string, Dictionary<string, IMutableParameter>> guidMappings)
        {
            IsGettingSafe = true;
            _identifierMappings = identifierMappings;
            _guidMappings = guidMappings;
            _guidConverter = new GuidConverter();
        }

        #endregion

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
            // the edited rows are gone too, so forget them rather than keep the old data in memory
            ClearAllOverrides();
        }

        #endregion

        #region IParameterManager

        /// <inheritdoc cref="IParameterManager.Get{T}(string)"/>
        public T Get<T>(string identifier) where T : class, IBaseInfo
        {
            CheckGet();
            return (T)Get(typeof(T), identifier);
        }

        /// <inheritdoc cref="IParameterManager.TryGet{T}(string, out T)"/>
        public bool TryGet<T>(string identifier, out T parameter) where T : class, IBaseInfo
        {
            parameter = Get<T>(identifier);
            return parameter != null;
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
            return (T)GetStructWithGUID(typeof(T), guid);
        }

        /// <inheritdoc cref="IParameterManager.Get{T}()"/>
        public virtual IEnumerable<T> Get<T>() where T : class, IBaseInfo
        {
            CheckGet();
            var type = typeof(T);
            if (type == typeof(IBaseInfo))
            {
                Debug.LogError($"Cannot use {nameof(IBaseInfo)} as type.");
                yield break;
            }

            foreach (var kvp in GetCurrent(type.Name))
                yield return (T)kvp.Value;
        }

        /// <inheritdoc cref="IParameterManager.GetSorted{T}()"/>
        public virtual IEnumerable<T> GetSorted<T>() where T : class, IBaseInfo
        {
            CheckGet();
            var type = typeof(T);
            if (type == typeof(IBaseInfo))
            {
                Debug.LogError($"Cannot use {nameof(IBaseInfo)} as type.");
                yield break;
            }

            foreach (var kvp in GetCurrent(type.Name).OrderBy(x => x.Key, StringComparer.Ordinal))
                yield return (T)kvp.Value;
        }

        /// <inheritdoc cref="IParameterManager.IsGettingSafe()"/>
        public bool IsGettingSafe { get; set; }

        /// <inheritdoc cref="IParameterManager.HasGetBeenCalled()"/>
        public bool HasGetBeenCalled { get; internal set; }

        #endregion

        protected internal readonly Dictionary<string, Dictionary<string, IMutableParameter>> _identifierMappings;
        protected internal readonly Dictionary<string, Dictionary<string, IMutableParameter>> _guidMappings;
        protected readonly GuidConverter _guidConverter;

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
                Debug.LogError("There is a mismatch in guids (possibly due to renaming of assets).  Regenerate all parameters and try again.");
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

        /// <inheritdoc cref="IParameterManager.Get"/>
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

        protected virtual IMutableParameter Get(string type, string identifier)
        {
            return GetCurrent(type, identifier);
        }

        /*
         * The GetLoaded methods return rows exactly as loaded, deleted ones included.  They know nothing about
         * overrides: the GetCurrent methods in ParameterManager.Overrides.cs add the rows that overrides add and
         * hide the ones they delete.
         */

        private IMutableParameter GetLoaded(string type, string identifier)
        {
            if (!_identifierMappings.TryGetValue(type, out Dictionary<string, IMutableParameter> parameters))
                return null;
            if (!parameters.TryGetValue(identifier, out IMutableParameter parameter))
                return null;
            return parameter;
        }

        private IMutableParameter GetLoadedWithGUID(string typeName, string guid)
        {
            if (!_guidMappings.TryGetValue(typeName, out Dictionary<string, IMutableParameter> parameters))
                return null;
            if (!parameters.TryGetValue(guid, out IMutableParameter parameter))
                return null;
            return parameter;
        }

        // a struct by guid, or by key path, since structs are stored under the hash of their key path
        private IMutableParameter GetLoadedStructWithGUID(string typeName, string guid)
        {
            if (!_guidMappings.TryGetValue(typeName, out Dictionary<string, IMutableParameter> parameters))
                return null;
            if (parameters.TryGetValue(guid, out IMutableParameter parameter))
                return parameter;
            if (!parameters.TryGetValue(_guidConverter.ToGuid(guid), out parameter))
                return null;
            return parameter;
        }

        // the loaded row an override names: by identifier, then guid, then struct key path
        private IMutableParameter GetLoadedForOverride(string interfaceType, string identifierOrKeyPathOrGuid)
        {
            return GetLoaded(interfaceType, identifierOrKeyPathOrGuid) ??
                   GetLoadedWithGUID(interfaceType, identifierOrKeyPathOrGuid) ??
                   GetLoadedStructWithGUID(interfaceType, identifierOrKeyPathOrGuid);
        }

        private IMutableParameter GetWithGUID(Type type, string guid)
        {
            if (type == typeof(IBaseInfo))
            {
                Debug.LogError($"Cannot use {nameof(IBaseInfo)} as type.");
                return null;
            }

            return GetWithGUID(type.Name, guid);
        }

        protected virtual IMutableParameter GetWithGUID(string typeName, string guid)
        {
            return GetWithGUID(typeName, guid, true);
        }

        private IMutableParameter GetStructWithGUID(Type type, string guid)
        {
            if (type == typeof(IBaseStruct))
            {
                Debug.LogError($"Cannot use {nameof(IBaseStruct)} as type.");
                return null;
            }

            return GetStructWithGUID(type.Name, guid, true);
        }

        protected virtual IMutableParameter GetStructWithGUID(string typeName, string guid, bool errorForMissing)
        {
            return GetCurrentStructWithGUID(typeName, guid, errorForMissing);
        }

        protected virtual IMutableParameter GetWithGUID(string typeName, string guid, bool errorForMissing)
        {
            return GetCurrentWithGUID(typeName, guid, errorForMissing);
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
