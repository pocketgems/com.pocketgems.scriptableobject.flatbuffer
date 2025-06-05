using System;
using System.Collections.Generic;
using System.Reflection;
using PocketGems.Parameters.Interface;
#if ADDRESSABLE_PARAMS
using UnityEngine.AddressableAssets;
#endif

namespace PocketGems.Parameters.Validation.Attributes
{
    internal class AssertAssignedReferenceExistsAttribute : BaseValidationAttribute
    {
#if ADDRESSABLE_PARAMS
        private bool _isAssetReference;
#endif
        private MethodInfo _paramRefGetInfoMethod;
        private HashSet<string> _addressablesGuidHashSet;

        protected override bool CompatibleWithReadOnlyLists => true;

        internal AssertAssignedReferenceExistsAttribute(HashSet<string> addressablesGuidHashSet)
        {
            _addressablesGuidHashSet = addressablesGuidHashSet;
        }

        protected override bool CheckType(Type type)
        {
#if ADDRESSABLE_PARAMS
            if (typeof(AssetReference).IsAssignableFrom(type))
                return true;
#endif
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ParameterReference<>);
        }

        protected override string ValidateEmptyList() => null;

        public override void WillValidateProperty(IParameterManager parameterManager, PropertyInfo propertyInfo)
        {
            base.WillValidateProperty(parameterManager, propertyInfo);

#if ADDRESSABLE_PARAMS
            _isAssetReference = typeof(AssetReference).IsAssignableFrom(_validationType);
            if (_isAssetReference)
                return;
#endif
            var methodName = nameof(ParameterReference<IBaseInfo>.GetInfo);
            _paramRefGetInfoMethod = _validationType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected override string ValidateElement(object element)
        {
            if (element == null)
                return null;

#if ADDRESSABLE_PARAMS
            // asset reference
            if (_isAssetReference)
                return ValidateAssetReference((AssetReference)element);
#endif
            // parameter reference
            return ValidateParameterReference((ParameterReference)element);
        }

#if ADDRESSABLE_PARAMS
        private string ValidateAssetReference(AssetReference assetReference)
        {
#if UNITY_EDITOR
            // can only do this check in editor - too costly and complicated verify this at device runtime.
            if (string.IsNullOrEmpty(assetReference.AssetGUID))
                return null;

            bool foundEntry = _addressablesGuidHashSet.Contains(assetReference.AssetGUID);
            return foundEntry ? null : $"cannot find addressable with guid {assetReference.AssetGUID}";
#else
            return null;
#endif
        }
#endif

        private string ValidateParameterReference(ParameterReference parameterReference)
        {
            if (string.IsNullOrEmpty(parameterReference.AssignedGUID) &&
                string.IsNullOrEmpty(parameterReference.AssignedIdentifier))
                return null;

            var args = new object[] { _parameterManager };
            object info = _paramRefGetInfoMethod.Invoke(parameterReference, args);
            if (info == null)
            {
                var interfaceType = parameterReference.GetType().GetGenericArguments()[0];
                if (!string.IsNullOrEmpty(parameterReference.AssignedGUID))
                    return $"cannot find parameter scriptable object {interfaceType.Name} with guid {parameterReference.AssignedGUID}";
                return
                    $"cannot find parameter scriptable object {interfaceType.Name} with identifier {parameterReference.AssignedIdentifier}";
            }

            return null;
        }
    }
}
