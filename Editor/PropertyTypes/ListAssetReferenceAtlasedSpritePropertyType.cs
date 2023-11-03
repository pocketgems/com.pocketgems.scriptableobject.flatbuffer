#if ADDRESSABLE_PARAMS
using System.Reflection;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class ListAssetReferenceAtlasedSpritePropertyType : ListAssetReferencePropertyType
    {
        public ListAssetReferenceAtlasedSpritePropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected override string ClassName => nameof(AssetReferenceAtlasedSprite);
    }
}
#endif
