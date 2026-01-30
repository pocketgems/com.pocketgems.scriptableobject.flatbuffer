using System;
using System.Collections.Generic;
using PocketGems.Parameters.Interface;
using UnityEngine;

namespace PocketGems.Parameters
{
    /// <summary>
    ///  Convenience wrappers for APIs that are frequently called by games.
    /// </summary>
    public static class Params
    {
        /// <summary>
        /// Sets the global static parameter manager instance.
        /// </summary>
        /// <param name="parameterManager">the parameter manager to set</param>
        public static void SetInstance(IMutableParameterManager parameterManager)
        {
            s_parameterManager = parameterManager;
            UpdateParameterManagerSafeFlag();
        }

        public static IParameterManager ParameterManager => s_parameterManager;
        public static IMutableParameterManager MutableParameterManager => s_parameterManager;

        /// <summary>
        /// Get a particular parameter object by identifier from the globally set parameter manager.
        /// </summary>
        /// <param name="identifier">identifier to query</param>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>the parameter if it exists</returns>
        public static T Get<T>(string identifier) where T : class, IBaseInfo => s_parameterManager.Get<T>(identifier);

        /// <summary>
        /// Get a particular parameter object by identifier from the globally set parameter manager.
        /// </summary>
        /// <param name="identifier">identifier to query</param>
        /// <param name="parameter">the parameter if it exists, otherwise null</param>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>true if the parameter exists</returns>
        public static bool TryGet<T>(string identifier, out T parameter) where T : class, IBaseInfo => s_parameterManager.TryGet(identifier, out parameter);

        /// <summary>
        /// Get a particular parameter object by identifier from the globally set parameter manager.
        /// </summary>
        /// <param name="type">type of object to return. This is expected to be a subinterface of IBaseInfo</param>
        /// <param name="identifier">identifier to query</param>
        /// <returns>object of type IBaseInfo, and implicitly of type Type</returns>
        public static IBaseInfo Get(string identifier, Type type) => s_parameterManager.Get(identifier, type);

        /// <summary>
        /// Get a particular parameter object by guid from the globally set parameter manager.
        /// </summary>
        /// <param name="guid">asset guid to query</param>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>the parameter if it exists</returns>
        public static T GetWithGUID<T>(string guid) where T : class, IBaseInfo => s_parameterManager.GetWithGUID<T>(guid);

        /// <summary>
        /// Get an enumerable of parameter objects of the same type from the globally set parameter manager.
        ///
        /// There is no guaranteed order.
        /// </summary>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>enumerator for all parameters the type</returns>
        public static IEnumerable<T> Get<T>() where T : class, IBaseInfo => s_parameterManager.Get<T>();

        /// <summary>
        /// Get an enumerable of parameter objects of the same type in
        /// identifier ascending order from the globally set parameter manager.
        ///
        /// This call is more expensive than Get() due to extra sorting overhead.
        /// </summary>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>enumerator for all parameters the type</returns>
        public static IEnumerable<T> GetSorted<T>() where T : class, IBaseInfo => s_parameterManager.GetSorted<T>();

        /// <summary>
        /// Gets or sets whether any ParameterManager's Get methods are safe to be called.  Get can be unsafe
        /// if they're currently waiting to be modified via AB testing.
        /// </summary>
        private static bool s_isGettingSafe = true;
        public static bool IsGettingSafe
        {
            get => s_isGettingSafe;
            set
            {
                s_isGettingSafe = value;
                UpdateParameterManagerSafeFlag();
            }
        }

        private static void UpdateParameterManagerSafeFlag()
        {
            if (s_parameterManager == null)
                return;

            s_parameterManager.IsGettingSafe = s_isGettingSafe;
            if (!s_isGettingSafe && s_parameterManager.HasGetBeenCalled)
            {
                Debug.LogError($"Parameters were already read while {nameof(IsGettingSafe)} is false. " +
                               $"Check the ordering of calls to ensure that {nameof(IsGettingSafe)} " +
                               $"is set to true before {nameof(IParameterManager)}'s GetX methods are used.");
            }
        }

        private static IMutableParameterManager s_parameterManager;
    }
}
