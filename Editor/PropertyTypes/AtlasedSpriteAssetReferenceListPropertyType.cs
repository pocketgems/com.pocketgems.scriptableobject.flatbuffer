#if ADDRESSABLE_PARAMS
using System.Reflection;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.PropertyTypes
{
    internal class AtlasedSpriteAssetReferenceListPropertyType : AssetReferenceListPropertyType
    {
        public AtlasedSpriteAssetReferenceListPropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected override string ClassName => nameof(AssetReferenceAtlasedSprite);
    }
}
#endif
