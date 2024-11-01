#if ADDRESSABLE_PARAMS
using System.Reflection;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class AssetReferenceAtlasedSpritePropertyType : AssetReferencePropertyType
    {
        public AssetReferenceAtlasedSpritePropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected override string ClassName => nameof(AssetReferenceAtlasedSprite);
    }
}
#endif
