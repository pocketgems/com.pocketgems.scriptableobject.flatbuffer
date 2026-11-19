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
        private string _referencedTypeName;
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
            _referencedTypeName = _validationType.GetGenericArguments()[0].Name;
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
            // IsNullOrEmpty, not HasAssignedValue, so a whitespace guid or identifier is reported
            string guid = parameterReference.AssignedGUID;
            if ((string.IsNullOrEmpty(guid) && string.IsNullOrEmpty(parameterReference.AssignedIdentifier)) ||
                parameterReference.AssignedValueExists(_parameterManager))
                return null;
            return !string.IsNullOrEmpty(guid)
                ? $"references {_referencedTypeName} with GUID {guid}, which is missing (removed, or deleted by an override)"
                : $"references {_referencedTypeName} \"{parameterReference.AssignedIdentifier}\", which is missing (removed, or deleted by an override)";
        }
    }
}
