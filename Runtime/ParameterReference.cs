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
        [NonSerialized] protected string _identifier;

        /// <summary>
        /// The assigned guid that is set typically from in the editor or when loaded from the parameter manager.
        /// </summary>
        public string AssignedGUID => guid;

        /// <summary>
        ///  Identifier is only set at runtime (abtesting) and not serialized in the editor.
        /// </summary>
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
    public class ParameterReference<T> : ParameterReference, IComparable<ParameterReference<T>> where T : class, IBaseInfo
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

        public int CompareTo(ParameterReference<T> other)
        {
            if (!string.IsNullOrWhiteSpace(AssignedGUID) &&
                AssignedGUID == other.AssignedGUID)
            {
                return 0;
            }

            var info = Info;
            var otherInfo = other.Info;
            if (info == otherInfo)
                return 0;
            if (info == null)
                return -1;
            if (otherInfo == null)
                return 1;
            return string.Compare(info.Identifier, otherInfo.Identifier);
        }

        /// <summary>
        /// A getter return the parameter info.  Null if it doesn't exist.
        /// </summary>
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

        /// <summary>
        /// Returns true if an info exists for the reference.
        ///
        /// A ParameterReference could have HasAssignedValue be true but InfoExists return false.
        /// This could happen if the assigned guid/identifier doesn't exist in the ParameterManager
        /// due to an incorrect assignment (validation wasn't ran or the identifier was assigned at
        /// runtime and incorrect).
        /// </summary>
        public bool InfoExists => Info != null;

        /// <summary>
        /// True if this reference has been assigned a guid/identifier that might exist.
        ///
        /// A ParameterReference could have HasAssignedValue be true but InfoExists return false.
        /// This could happen if the assigned guid/identifier doesn't exist in the ParameterManager
        /// due to an incorrect assignment (validation wasn't ran or the identifier was assigned at
        /// runtime and incorrect).
        /// </summary>
        public bool HasAssignedValue => !string.IsNullOrWhiteSpace(guid) || !string.IsNullOrWhiteSpace(_identifier);

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
