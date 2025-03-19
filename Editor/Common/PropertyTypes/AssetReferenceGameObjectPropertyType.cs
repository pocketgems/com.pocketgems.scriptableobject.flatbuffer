#if ADDRESSABLE_PARAMS
using System.Reflection;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class AssetReferenceGameObjectPropertyType : AssetReferencePropertyType
    {
        public AssetReferenceGameObjectPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected override string ClassName => nameof(AssetReferenceGameObject);
    }
}
#endif
