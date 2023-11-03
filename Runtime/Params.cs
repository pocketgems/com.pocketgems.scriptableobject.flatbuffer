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
        public static void SetInstance(IMutableParameterManager parameterManager)
        {
            s_parameterManager = parameterManager;
            UpdateParameterManagerSafeFlag();
        }

        public static IParameterManager ParameterManager => s_parameterManager;
        public static IMutableParameterManager MutableParameterManager => s_parameterManager;

        public static T Get<T>(string identifier) where T : class, IBaseInfo => s_parameterManager.Get<T>(identifier);

        public static T GetWithGUID<T>(string guid) where T : class, IBaseInfo => s_parameterManager.GetWithGUID<T>(guid);

        public static IEnumerable<T> Get<T>() where T : class, IBaseInfo => s_parameterManager.Get<T>();

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
