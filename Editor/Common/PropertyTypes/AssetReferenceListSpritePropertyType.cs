#if ADDRESSABLE_PARAMS
using System.Reflection;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    internal class AssetReferenceListSpritePropertyType : AssetReferenceListPropertyType
    {
        public AssetReferenceListSpritePropertyType(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        protected override string ClassName => nameof(AssetReferenceSprite);
    }
}
#endif
