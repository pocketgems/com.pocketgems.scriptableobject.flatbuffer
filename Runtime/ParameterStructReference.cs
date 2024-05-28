using PocketGems.Parameters.Interface;
using UnityEngine;
#if UNITY_EDITOR
using System.Threading;
using UnityEditor;
#endif

namespace PocketGems.Parameters
{
    public abstract class ParameterStructReference
    {
        protected string _guid;

        protected ParameterStructReference(string guid)
        {
            _guid = guid;
        }
    }

    public abstract class ParameterStructReference<T> : ParameterStructReference where T : class, IBaseStruct
    {
        protected ParameterStructReference(string guid) : base(guid)
        {
        }

        public T Struct
        {
            get
            {
#if UNITY_EDITOR
                // We allow the Info to be used in the background at runtime.
                // Calling Application.isPlaying causes issues in the background.
                if (Thread.CurrentThread.ManagedThreadId == 1 && !Application.isPlaying)
                {
                    return GetStruct(EditorParams.ParameterManager);
                }
#endif
                if (Params.ParameterManager == null)
                {
                    Debug.LogError("Fetching Struct before ParamsSetup.Setup() has been called.");
                    return null;
                }

                return GetStruct(Params.ParameterManager);
            }
        }

        internal abstract T GetStruct(IParameterManager parameterManager);
    }

#if UNITY_EDITOR
    public class ParameterStructReferenceEditor<T> : ParameterStructReference<T> where T : class, IBaseStruct
    {
        private T _struct;

        public ParameterStructReferenceEditor(T @struct) : base(null)
        {
            _struct = @struct;
        }

        internal override T GetStruct(IParameterManager parameterManager) => _struct;
    }
#endif

    public class ParameterStructReferenceRuntime<T> : ParameterStructReference<T> where T : class, IBaseStruct
    {
        public ParameterStructReferenceRuntime(string guid) : base(guid)
        {
        }

        public override string ToString()
        {
            string description = _guid;
            if (string.IsNullOrWhiteSpace(_guid))
            {
                description = ": not assigned";
            }
            else if (Struct == null)
            {
                description = $": missing asset for GUID {_guid}";
            }

            return $"{GetType().Name}<{typeof(T).Name}>: {description}";
        }


        internal override T GetStruct(IParameterManager parameterManager) => parameterManager.GetStructWithGuid<T>(_guid);
    }
}
