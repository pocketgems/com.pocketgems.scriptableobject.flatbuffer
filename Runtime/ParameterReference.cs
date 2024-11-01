using System;
using PocketGems.Parameters.Interface;
using UnityEngine;
#if UNITY_EDITOR
using System.Threading;
using UnityEditor;
#endif

namespace PocketGems.Parameters
{
    [Serializable]
    public abstract class ParameterReference
    {
        [SerializeField] protected string guid;

        /// <summary>
        ///  Identifier is only set at runtime (abtesting) and not serialized in the editor.
        /// </summary>
        [NonSerialized] protected string _identifier;

        public string AssignedGUID => guid;
        public string AssignedIdentifier => _identifier;

        /// <summary>
        /// Constructor for ParameterReferences.
        /// </summary>
        /// <param name="value">guid or identifier for the asset</param>
        /// <param name="isIdentifier">true if the string value is an identifier</param>
        protected ParameterReference(string value, bool isIdentifier)
        {
            if (isIdentifier)
                _identifier = value;
            else
                guid = value;
        }
    }

    [Serializable]
    public class ParameterReference<T> : ParameterReference where T : class, IBaseInfo
    {
        public ParameterReference(string value = null, bool isIdentifier = false) : base(value, isIdentifier)
        {
        }

        public override string ToString()
        {
            string description;
            if (string.IsNullOrWhiteSpace(guid) && string.IsNullOrWhiteSpace(_identifier))
            {
                description = "not assigned";
            }
            else if (Info == null)
            {
                if (!string.IsNullOrWhiteSpace(_identifier))
                    description = $"missing asset for identifier {_identifier}";
                else
                    description = $"missing asset for GUID {guid}";
            }
            else
            {
                description = $"{Info.Identifier}({guid})";
            }
            return $"{GetType().Name}<{typeof(T).Name}>: {description}";
        }

        public T Info
        {
            get
            {
#if UNITY_EDITOR
                // We allow the Info to be used in the background at runtime.
                // Calling Application.isPlaying causes issues in the background.
                if (Thread.CurrentThread.ManagedThreadId == 1 && !Application.isPlaying)
                {
                    return GetInfo(EditorParams.ParameterManager);
                }
#endif
                if (Params.ParameterManager == null)
                {
                    Debug.LogError("Fetching Info before ParamsSetup.Setup() has been called.");
                    return null;
                }

                return GetInfo(Params.ParameterManager);
            }
        }

        internal T GetInfo(IParameterManager parameterManager)
        {
            // parameterManager.GetWithGUID will throw a LogError if it doesn't exist since
            // all serialized guids should have data.
            if (!string.IsNullOrWhiteSpace(guid))
                return parameterManager.GetWithGUID<T>(guid);

            if (!string.IsNullOrWhiteSpace(_identifier))
            {
                // parameterManager.Get<T> will not throw an error if it doesn't exist because
                // there maybe situations where external code will want to query if it exists or not.
                var info = parameterManager.Get<T>(_identifier);
                // from a Parameter Reference perspective, it should always exist.
                if (info == null)
                    Debug.LogError($"Cannot find info of type {typeof(T)} for identifier: {_identifier}");
                return info;
            }

            return null;
        }
    }
}
