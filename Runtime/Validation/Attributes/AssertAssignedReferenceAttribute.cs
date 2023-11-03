using System;
using System.Reflection;
#if ADDRESSABLE_PARAMS
using UnityEngine.AddressableAssets;
#endif

namespace PocketGems.Parameters.Validation.Attributes
{
    public class AssertAssignedReferenceAttribute : BaseValidationAttribute
    {
        private const string ErrorString = "requires assigned value";

#if ADDRESSABLE_PARAMS
        private bool _isAssetReference;
#endif
        protected override bool CompatibleWithReadOnlyLists => true;

        protected override bool CheckType(Type type)
        {
#if ADDRESSABLE_PARAMS
            if (typeof(AssetReference).IsAssignableFrom(type))
                return true;
#endif
            return typeof(ParameterReference).IsAssignableFrom(type);
        }

        public override void WillValidateProperty(IParameterManager parameterManager, PropertyInfo propertyInfo)
        {
            base.WillValidateProperty(parameterManager, propertyInfo);

#if ADDRESSABLE_PARAMS
            _isAssetReference = typeof(AssetReference).IsAssignableFrom(_validationType);
#endif
        }

        protected override string ValidateEmptyList() => null;

        protected override string ValidateElement(object element)
        {
            if (element == null)
                return ErrorString;

#if ADDRESSABLE_PARAMS
            // asset reference
            if (_isAssetReference)
            {
                if (string.IsNullOrEmpty(((AssetReference)element).AssetGUID))
                    return ErrorString;
                return null;
            }
#endif
            // parameter reference
            string assignedGuid = ((ParameterReference)element).AssignedGUID;
            if (string.IsNullOrEmpty(assignedGuid))
                return ErrorString;
            return null;
        }
    }
}
