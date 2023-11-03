using PocketGems.Parameters.Validation;
using UnityEngine;

namespace PocketGems.Parameters.Interface
{
    public abstract class ParameterScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        public abstract ValidationError[] ValidationErrors();
#endif
    }
}
