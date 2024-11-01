using System;
using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.DataGeneration.Util.Editor
{
    public class StructKeyPathBuilderTest
    {
        private StructKeyPathBuilder _builder;
        private const string kRootIdentifier = "ID";
        private const string kRootType = "RootType";
        private const string kKey1 = "key1";
        private const string kKey2 = "key2";
        private string ExpectedRootKey => $"{kRootType}[{kRootIdentifier}]";

        [SetUp]
        public void SetUp()
        {
            _builder = new StructKeyPathBuilder();
        }

        [Test]
        // success
        [TestCase("BuildingInfo[LumberYard]", true, "BuildingInfo", "LumberYard")]
        [TestCase("BuildingInfo[LumberYard].Blah.Blah[0].Blah", true, "BuildingInfo", "LumberYard")]
        // bad inputs
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("[]", false)]
        [TestCase("BuildingInfo", false)]
        [TestCase("BuildingInfo[]", false)]
        [TestCase("BuildingInfo]LumberYard[", false)]
        [TestCase("[LumberYard]", false)]
        [TestCase("test[test", false)]
        [TestCase("test]test", false)]
        public void FetchRootKey(string input, bool assertSuccess, string assertType = null, string assertIdentifier = null)
        {
            var success = StructKeyPathBuilder.FetchRootKey(input,
                out string rootKeyType, out string rootKeyIdentifier, out string error);

            Assert.AreEqual(assertSuccess, success);
            if (assertSuccess)
            {
                Assert.AreEqual(assertType, rootKeyType);
                Assert.AreEqual(assertIdentifier, rootKeyIdentifier);
                Assert.IsNull(error);
            }
            else
            {
                Assert.IsNull(rootKeyType);
                Assert.IsNull(rootKeyIdentifier);
                Assert.IsNotEmpty(error);
            }
        }

        [Test]
        public void PushAndPop()
        {
            Assert.AreEqual("", _builder.KeyPath());
            Assert.AreEqual(0, _builder.Length);

            _builder.PushRootKey(kRootType, kRootIdentifier);

            Assert.AreEqual($"{ExpectedRootKey}", _builder.KeyPath());
            Assert.AreEqual(1, _builder.Length);
            Assert.AreEqual(kRootIdentifier, _builder.RootKeyIdentifier);
            Assert.AreEqual(kRootType, _builder.RootKeyType);

            _builder.PushKey(kKey1);

            Assert.AreEqual($"{ExpectedRootKey}.{kKey1}", _builder.KeyPath());
            Assert.AreEqual(2, _builder.Length);
            Assert.AreEqual(kRootIdentifier, _builder.RootKeyIdentifier);
            Assert.AreEqual(kRootType, _builder.RootKeyType);

            _builder.PushKey(kKey2, 10);

            Assert.AreEqual($"{ExpectedRootKey}.{kKey1}.{kKey2}[10]", _builder.KeyPath());
            Assert.AreEqual(3, _builder.Length);
            Assert.AreEqual(kRootIdentifier, _builder.RootKeyIdentifier);
            Assert.AreEqual(kRootType, _builder.RootKeyType);

            _builder.PopKey();

            Assert.AreEqual($"{ExpectedRootKey}.{kKey1}", _builder.KeyPath());
            Assert.AreEqual(2, _builder.Length);

            _builder.PushKey(kKey2);

            Assert.AreEqual($"{ExpectedRootKey}.{kKey1}.{kKey2}", _builder.KeyPath());
            Assert.AreEqual(3, _builder.Length);
            Assert.AreEqual(kRootIdentifier, _builder.RootKeyIdentifier);
            Assert.AreEqual(kRootType, _builder.RootKeyType);

            _builder.PopKey();
            _builder.PopKey();

            Assert.AreEqual($"{ExpectedRootKey}", _builder.KeyPath());
            Assert.AreEqual(1, _builder.Length);
            Assert.AreEqual(kRootIdentifier, _builder.RootKeyIdentifier);
            Assert.AreEqual(kRootType, _builder.RootKeyType);

            _builder.PopKey();

            Assert.AreEqual("", _builder.KeyPath());
            Assert.AreEqual(0, _builder.Length);
            Assert.AreEqual(null, _builder.RootKeyIdentifier);
            Assert.AreEqual(null, _builder.RootKeyType);

            Assert.Throws<IndexOutOfRangeException>(() => { _builder.PopKey(); });
        }

        [Test]
        public void InvalidKey()
        {
            Assert.Throws<InvalidDataException>(() => { _builder.PushRootKey("", "blah"); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushRootKey(null, "blah"); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushRootKey("blah", ""); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushRootKey("blah", null); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushKey(""); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushKey(null); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushKey("blah.blah"); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushKey("blah[blah"); });
            Assert.Throws<InvalidDataException>(() => { _builder.PushKey("blah]blah"); });
        }

        [Test]
        public void Clear()
        {
            Assert.AreEqual("", _builder.KeyPath());
            Assert.AreEqual(0, _builder.Length);
            Assert.AreEqual(null, _builder.RootKeyIdentifier);
            Assert.AreEqual(null, _builder.RootKeyType);

            _builder.PushRootKey(kRootType, kRootIdentifier);
            _builder.PushKey(kKey1);
            _builder.PushKey(kKey2);

            Assert.AreEqual($"{ExpectedRootKey}.{kKey1}.{kKey2}", _builder.KeyPath());
            Assert.AreEqual(3, _builder.Length);
            Assert.AreEqual(kRootIdentifier, _builder.RootKeyIdentifier);
            Assert.AreEqual(kRootType, _builder.RootKeyType);

            _builder.Clear();
            Assert.AreEqual("", _builder.KeyPath());
            Assert.AreEqual(0, _builder.Length);
            Assert.AreEqual(null, _builder.RootKeyIdentifier);
            Assert.AreEqual(null, _builder.RootKeyType);
        }

        [Test]
        public void ForgettingPushRootKey()
        {
            Assert.Throws<InvalidDataException>(() => { _builder.PushKey("key"); });
        }
    }
}
