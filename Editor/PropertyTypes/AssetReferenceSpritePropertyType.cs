#if ADDRESSABLE_PARAMS
using System.Reflection;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class AssetReferenceSpritePropertyType : AssetReferencePropertyType
    {
        public AssetReferenceSpritePropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected override string ClassName => nameof(AssetReferenceSprite);
    }
}
#endif