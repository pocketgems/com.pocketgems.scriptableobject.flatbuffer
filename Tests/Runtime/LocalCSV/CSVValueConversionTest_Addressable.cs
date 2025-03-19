#if ADDRESSABLE_PARAMS
using System;
using NUnit.Framework;
using UnityEngine.AddressableAssets;

namespace PocketGems.Parameters.LocalCSV
{
    public partial class CSVValueConversionTest
    {
        [Test]
        [TestCase("blah", "objName", "blah-objName")]
        [TestCase("blah", "objName-", "blah-objName-")]
        [TestCase("blah", "", "blah-")]
        [TestCase("blah", null, "blah-")]
        [TestCase("", null, "-")]
        [TestCase(null, null, "-")]
        public void AssetReferenceToString(string guid, string subObjectName, string expected)
        {
            var reference = new AssetReference(guid);
            if (subObjectName != null)
                reference.SubObjectName = subObjectName;

            Assert.AreEqual(expected, CSVValueConverter.AssetReference.ToString(reference));

            if (guid == null)
                Assert.AreEqual(expected, CSVValueConverter.AssetReference.ToString(null));
        }

        [Test]
        [TestCase("blah-", "blah", null)]
        [TestCase("blah-boo", "blah", "boo")]
        [TestCase("blah-boo-", "blah", "boo-")]
        [TestCase("", "", null)]
        [TestCase(null, "", null)]
        public void AssetReferenceFromString(string value, string expectedGUID, string expectedSubObjectName)
        {
            var reference = CSVValueConverter.AssetReference.FromString(value);
            Assert.IsNotNull(reference);
            Assert.AreEqual(typeof(AssetReference), reference.GetType());
            Assert.AreEqual(expectedGUID, reference.AssetGUID);
            Assert.AreEqual(expectedSubObjectName, reference.SubObjectName);
        }

        [Test]
        [TestCase("blah", "objName", "blah-objName")]
        [TestCase("blah", "objName-", "blah-objName-")]
        [TestCase("blah", "", "blah-")]
        [TestCase("blah", null, "blah-")]
        [TestCase("", null, "-")]
        [TestCase(null, null, "-")]
        public void AssetReferenceGameObjectToString(string guid, string subObjectName, string expected)
        {
            var reference = new AssetReferenceGameObject(guid);
            if (subObjectName != null)
                reference.SubObjectName = subObjectName;

            Assert.AreEqual(expected, CSVValueConverter.AssetReferenceGameObject.ToString(reference));

            if (guid == null)
                Assert.AreEqual(expected, CSVValueConverter.AssetReferenceGameObject.ToString(null));
        }

        [Test]
        [TestCase("blah-", "blah", null)]
        [TestCase("blah-boo", "blah", "boo")]
        [TestCase("blah-boo-", "blah", "boo-")]
        [TestCase("", "", null)]
        [TestCase(null, "", null)]
        public void AssetReferenceGameObjectFromString(string value, string expectedGUID, string expectedSubObjectName)
        {
            var reference = CSVValueConverter.AssetReferenceGameObject.FromString(value);
            Assert.IsNotNull(reference);
            Assert.AreEqual(typeof(AssetReferenceGameObject), reference.GetType());
            Assert.AreEqual(expectedGUID, reference.AssetGUID);
            Assert.AreEqual(expectedSubObjectName, reference.SubObjectName);
        }

        [Test]
        public void AssetReferenceFromStringError()
        {
            Assert.Throws<Exception>(() => CSVValueConverter.AssetReference.FromString("asdf"));
        }

        [Test]
        [TestCase("blah", "objName", "blah-objName")]
        [TestCase("blah", "objName-", "blah-objName-")]
        [TestCase("blah", "", "blah-")]
        [TestCase("blah", null, "blah-")]
        [TestCase("", null, "-")]
        [TestCase(null, null, "-")]
        public void AssetReferenceSpriteToString(string guid, string subObjectName, string expected)
        {
            var reference = new AssetReferenceSprite(guid);
            if (subObjectName != null)
                reference.SubObjectName = subObjectName;

            Assert.AreEqual(expected, CSVValueConverter.AssetReferenceSprite.ToString(reference));

            if (guid == null)
                Assert.AreEqual(expected, CSVValueConverter.AssetReferenceSprite.ToString(null));
        }

        [Test]
        [TestCase("blah-", "blah", null)]
        [TestCase("blah-boo", "blah", "boo")]
        [TestCase("blah-boo-", "blah", "boo-")]
        [TestCase("", "", null)]
        [TestCase(null, "", null)]
        public void AssetReferenceSpriteFromString(string value, string expectedGUID, string expectedSubObjectName)
        {
            var reference = CSVValueConverter.AssetReferenceSprite.FromString(value);
            Assert.IsNotNull(reference);
            Assert.AreEqual(typeof(AssetReferenceSprite), reference.GetType());
            Assert.AreEqual(expectedGUID, reference.AssetGUID);
            Assert.AreEqual(expectedSubObjectName, reference.SubObjectName);
        }

        [Test]
        [TestCase("blah", "objName", "blah-objName")]
        [TestCase("blah", "objName-", "blah-objName-")]
        [TestCase("blah", "", "blah-")]
        [TestCase("blah", null, "blah-")]
        [TestCase("", null, "-")]
        [TestCase(null, null, "-")]
        public void AssetReferenceAtlasedSpriteToString(string guid, string subObjectName, string expected)
        {
            var reference = new AssetReferenceAtlasedSprite(guid);
            if (subObjectName != null)
                reference.SubObjectName = subObjectName;

            Assert.AreEqual(expected, CSVValueConverter.AssetReferenceAtlasedSprite.ToString(reference));

            if (guid == null)
                Assert.AreEqual(expected, CSVValueConverter.AssetReferenceAtlasedSprite.ToString(null));
        }

        [Test]
        [TestCase("blah-", "blah", null)]
        [TestCase("blah-boo", "blah", "boo")]
        [TestCase("blah-boo-", "blah", "boo-")]
        [TestCase("", "", null)]
        [TestCase(null, "", null)]
        public void AssetReferenceAtlasedSpriteFromString(string value, string expectedGUID, string expectedSubObjectName)
        {
            var reference = CSVValueConverter.AssetReferenceAtlasedSprite.FromString(value);
            Assert.IsNotNull(reference);
            Assert.AreEqual(typeof(AssetReferenceAtlasedSprite), reference.GetType());
            Assert.AreEqual(expectedGUID, reference.AssetGUID);
            Assert.AreEqual(expectedSubObjectName, reference.SubObjectName);
        }

        [Test]
        public void AssetReferenceArrayToString()
        {
            void Test(string expected, AssetReference[] value)
            {
                Assert.AreEqual(expected, CSVValueConverter.AssetReferenceArray.ToString(value));
            }

            var refNormal1 = new AssetReference("guid1");
            var refNormal2 = new AssetReference("guid2");
            refNormal2.SubObjectName = "sub";
            var refNull = new AssetReference(null);
            var refEmpty = new AssetReference("");

            Test("", null);
            Test("guid1-", new[] { refNormal1 });
            Test("guid1-|-", new[] { refNormal1, refNull });
            Test("guid1-|-|-|guid2-sub", new[] { refNormal1, refNull, refEmpty, refNormal2 });
        }

        [Test]
        public void AssetReferenceArrayFromString()
        {
            Assert.AreEqual(Array.Empty<AssetReference>(), CSVValueConverter.AssetReferenceArray.FromString(null));
            Assert.AreEqual(Array.Empty<AssetReference>(), CSVValueConverter.AssetReferenceArray.FromString(""));
            Assert.AreEqual(Array.Empty<AssetReference>(), CSVValueConverter.AssetReferenceArray.FromString(" "));
            var result = CSVValueConverter.AssetReferenceArray.FromString("guid-|-|-|guid2-sub");
            Assert.AreEqual("guid", result[0].AssetGUID);
            Assert.AreEqual(null, result[0].SubObjectName);
            Assert.AreEqual("", result[1].AssetGUID);
            Assert.AreEqual(null, result[1].SubObjectName);
            Assert.AreEqual("", result[2].AssetGUID);
            Assert.AreEqual(null, result[2].SubObjectName);
            Assert.AreEqual("guid2", result[3].AssetGUID);
            Assert.AreEqual("sub", result[3].SubObjectName);
        }

        [Test]
        public void AssetReferenceGameObjectArrayToString()
        {
            void Test(string expected, AssetReferenceGameObject[] value)
            {
                Assert.AreEqual(expected, CSVValueConverter.AssetReferenceGameObjectArray.ToString(value));
            }

            var refNormal1 = new AssetReferenceGameObject("guid1");
            var refNormal2 = new AssetReferenceGameObject("guid2");
            refNormal2.SubObjectName = "sub";
            var refNull = new AssetReferenceGameObject(null);
            var refEmpty = new AssetReferenceGameObject("");

            Test("", null);
            Test("guid1-", new[] { refNormal1 });
            Test("guid1-|-", new[] { refNormal1, refNull });
            Test("guid1-|-|-|guid2-sub", new[] { refNormal1, refNull, refEmpty, refNormal2 });
        }

        [Test]
        public void AssetReferenceGameObjectArrayFromString()
        {
            Assert.AreEqual(Array.Empty<AssetReferenceGameObject>(),
                CSVValueConverter.AssetReferenceGameObjectArray.FromString(null));
            Assert.AreEqual(Array.Empty<AssetReferenceGameObject>(),
                CSVValueConverter.AssetReferenceGameObjectArray.FromString(""));
            Assert.AreEqual(Array.Empty<AssetReferenceGameObject>(),
                CSVValueConverter.AssetReferenceGameObjectArray.FromString(" "));
            var result = CSVValueConverter.AssetReferenceGameObjectArray.FromString("guid-|-|-|guid2-sub");
            Assert.AreEqual("guid", result[0].AssetGUID);
            Assert.AreEqual(null, result[0].SubObjectName);
            Assert.AreEqual("", result[1].AssetGUID);
            Assert.AreEqual(null, result[1].SubObjectName);
            Assert.AreEqual("", result[2].AssetGUID);
            Assert.AreEqual(null, result[2].SubObjectName);
            Assert.AreEqual("guid2", result[3].AssetGUID);
            Assert.AreEqual("sub", result[3].SubObjectName);
        }

        [Test]
        public void AssetReferenceSpriteArrayToString()
        {
            void Test(string expected, AssetReferenceSprite[] value)
            {
                Assert.AreEqual(expected, CSVValueConverter.AssetReferenceSpriteArray.ToString(value));
            }

            var refNormal1 = new AssetReferenceSprite("guid1");
            var refNormal2 = new AssetReferenceSprite("guid2");
            refNormal2.SubObjectName = "sub";
            var refNull = new AssetReferenceSprite(null);
            var refEmpty = new AssetReferenceSprite("");

            Test("", null);
            Test("guid1-", new[] { refNormal1 });
            Test("guid1-|-", new[] { refNormal1, refNull });
            Test("guid1-|-|-|guid2-sub", new[] { refNormal1, refNull, refEmpty, refNormal2 });
        }

        [Test]
        public void AssetReferenceSpriteArrayFromString()
        {
            Assert.AreEqual(Array.Empty<AssetReferenceSprite>(), CSVValueConverter.AssetReferenceSpriteArray.FromString(null));
            Assert.AreEqual(Array.Empty<AssetReferenceSprite>(), CSVValueConverter.AssetReferenceSpriteArray.FromString(""));
            Assert.AreEqual(Array.Empty<AssetReferenceSprite>(), CSVValueConverter.AssetReferenceSpriteArray.FromString(" "));
            var result = CSVValueConverter.AssetReferenceSpriteArray.FromString("guid-|-|-|guid2-sub");
            Assert.AreEqual("guid", result[0].AssetGUID);
            Assert.AreEqual(null, result[0].SubObjectName);
            Assert.AreEqual("", result[1].AssetGUID);
            Assert.AreEqual(null, result[1].SubObjectName);
            Assert.AreEqual("", result[2].AssetGUID);
            Assert.AreEqual(null, result[2].SubObjectName);
            Assert.AreEqual("guid2", result[3].AssetGUID);
            Assert.AreEqual("sub", result[3].SubObjectName);
        }

        [Test]
        public void AssetReferenceAtlasedSpriteArrayToString()
        {
            void Test(string expected, AssetReferenceAtlasedSprite[] value)
            {
                Assert.AreEqual(expected, CSVValueConverter.AssetReferenceAtlasedSpriteArray.ToString(value));
            }

            var refNormal1 = new AssetReferenceAtlasedSprite("guid1");
            var refNormal2 = new AssetReferenceAtlasedSprite("guid2");
            refNormal2.SubObjectName = "sub";
            var refNull = new AssetReferenceAtlasedSprite(null);
            var refEmpty = new AssetReferenceAtlasedSprite("");

            Test("", null);
            Test("guid1-", new[] { refNormal1 });
            Test("guid1-|-", new[] { refNormal1, refNull });
            Test("guid1-|-|-|guid2-sub", new[] { refNormal1, refNull, refEmpty, refNormal2 });
        }

        [Test]
        public void AssetReferenceAtlasedSpriteArrayFromString()
        {
            Assert.AreEqual(Array.Empty<AssetReferenceAtlasedSprite>(),
                CSVValueConverter.AssetReferenceAtlasedSpriteArray.FromString(null));
            Assert.AreEqual(Array.Empty<AssetReferenceAtlasedSprite>(),
                CSVValueConverter.AssetReferenceAtlasedSpriteArray.FromString(""));
            Assert.AreEqual(Array.Empty<AssetReferenceAtlasedSprite>(),
                CSVValueConverter.AssetReferenceAtlasedSpriteArray.FromString(" "));
            var result = CSVValueConverter.AssetReferenceAtlasedSpriteArray.FromString("guid-|-|-|guid2-sub");
            Assert.AreEqual("guid", result[0].AssetGUID);
            Assert.AreEqual(null, result[0].SubObjectName);
            Assert.AreEqual("", result[1].AssetGUID);
            Assert.AreEqual(null, result[1].SubObjectName);
            Assert.AreEqual("", result[2].AssetGUID);
            Assert.AreEqual(null, result[2].SubObjectName);
            Assert.AreEqual("guid2", result[3].AssetGUID);
            Assert.AreEqual("sub", result[3].SubObjectName);
        }
    }
}
#endif
