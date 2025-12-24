using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParameterManagerTest
    {
        private MockMySpecialInfo _mockMySpecialInfo;
        private MockMyVerySpecialInfo _mockMyVerySpecialInfo;
        private MockKeyValueStruct _mockKeyValueStruct;

        private const string BaseInterfaceErrorMsg = "Cannot use IBaseInfo or IBaseStruct as type.";
        private const string BaseStructErrorMsg = "Cannot use IBaseStruct as type.";
        private const string BaseInfoErrorMsg = "Cannot use IBaseInfo as type.";

        private const string SpecialId = "id1";
        private const string SpecialGuid = "guid1";

        private const string VerySpecialId = "id2";
        private const string VerySpecialGuid = "guid2";

        private const string StructGuid = "guid3";

        private ParameterManager _parameterManager;

        [SetUp]
        public void SetUp()
        {
            _parameterManager = new ParameterManager();
            _mockMySpecialInfo = new MockMySpecialInfo();
            _mockMyVerySpecialInfo = new MockMyVerySpecialInfo();
            _mockKeyValueStruct = new MockKeyValueStruct(_parameterManager, "desc", 10, "guid", new string[0]);
        }

        private void LoadInfos()
        {
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(_mockKeyValueStruct, StructGuid);

            // load under one interface
            _parameterManager.Load<IMySpecialInfo, MockMySpecialInfo>(_mockMySpecialInfo, SpecialId, SpecialGuid);

            // load under two interfaces
            _parameterManager.Load<IMySpecialInfo, MockMyVerySpecialInfo>(_mockMyVerySpecialInfo, VerySpecialId, VerySpecialGuid);
            _parameterManager.Load<IMyVerySpecialInfo, MockMyVerySpecialInfo>(_mockMyVerySpecialInfo, VerySpecialId, VerySpecialGuid);
        }

        private void AssertEmptyManager()
        {
            Assert.IsNull(_parameterManager.Get<IMySpecialInfo>(SpecialId));
            Assert.IsNull(_parameterManager.Get<IMyVerySpecialInfo>(SpecialId));
            Assert.IsNull(_parameterManager.Get(SpecialId, typeof(IMySpecialInfo)));
            Assert.IsNull(_parameterManager.Get(SpecialId, typeof(IMyVerySpecialInfo)));

            Assert.IsNull(_parameterManager.Get<IMySpecialInfo>(VerySpecialId));
            Assert.IsNull(_parameterManager.Get<IMyVerySpecialInfo>(VerySpecialId));
            Assert.IsNull(_parameterManager.Get(VerySpecialId, typeof(IMySpecialInfo)));
            Assert.IsNull(_parameterManager.Get(VerySpecialId, typeof(IMyVerySpecialInfo)));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<IMySpecialInfo>(SpecialGuid));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<IMyVerySpecialInfo>(SpecialGuid));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<IMySpecialInfo>(VerySpecialGuid));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<IMyVerySpecialInfo>(VerySpecialGuid));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));
        }

        [Test]
        public void LoadAndGet()
        {
            AssertEmptyManager();
            LoadInfos();

            Assert.AreEqual(_mockMySpecialInfo, _parameterManager.Get<IMySpecialInfo>(SpecialId));
            Assert.AreEqual(_mockMySpecialInfo, _parameterManager.Get(SpecialId, typeof(IMySpecialInfo)));
            Assert.IsNull(_parameterManager.Get<IMyVerySpecialInfo>(SpecialId));
            Assert.IsNull(_parameterManager.Get(SpecialId, typeof(IMyVerySpecialInfo)));

            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.Get(VerySpecialId, typeof(IMySpecialInfo)));
            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.Get(VerySpecialId, typeof(IMyVerySpecialInfo)));
            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.Get<IMySpecialInfo>(VerySpecialId));
            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.Get<IMyVerySpecialInfo>(VerySpecialId));

            Assert.AreEqual(_mockMySpecialInfo, _parameterManager.GetWithGUID<IMySpecialInfo>(SpecialGuid));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<IMyVerySpecialInfo>(SpecialGuid));

            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.GetWithGUID<IMySpecialInfo>(VerySpecialGuid));
            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.GetWithGUID<IMyVerySpecialInfo>(VerySpecialGuid));
        }

        [Test]
        public void LoadAndGetWithStruct()
        {
            AssertEmptyManager();
            LoadInfos();

            Assert.AreEqual(_mockKeyValueStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>("non existing guid"));
        }

        [Test]
        public void InvalidUsesWithIBaseInfo()
        {
            LoadInfos();
            LogAssert.Expect(LogType.Error, BaseInfoErrorMsg);
            Assert.IsNull(_parameterManager.Get<IBaseInfo>("some_identifier"));

            LogAssert.Expect(LogType.Error, BaseInterfaceErrorMsg);
            Assert.IsNull(_parameterManager.GetWithGUID<IBaseInfo>("some_guid"));

            LogAssert.Expect(LogType.Error, BaseInfoErrorMsg);
            Assert.AreEqual(0, _parameterManager.Get<IBaseInfo>().ToArray().Length);

            LogAssert.Expect(LogType.Error, BaseInfoErrorMsg);
            Assert.AreEqual(0, _parameterManager.GetSorted<IBaseInfo>().ToArray().Length);

            LogAssert.Expect(LogType.Error, BaseInfoErrorMsg);
            _parameterManager.Load<IBaseInfo, MockMySpecialInfo>(null, "some_id", "some guid");
        }

        [Test]
        public void InvalidUsesWithIBaseStruct()
        {
            LoadInfos();

            LogAssert.Expect(LogType.Error, BaseInterfaceErrorMsg);
            Assert.IsNull(_parameterManager.GetStructWithGuid<IBaseStruct>("some_guid"));

            LogAssert.Expect(LogType.Error, BaseStructErrorMsg);
            _parameterManager.Load<IBaseStruct, MockKeyValueStruct>(null, "some guid");
        }

        [Test]
        public void GetIEnumerable()
        {
            AssertEmptyManager();
            LoadInfos();

            var specialInfos = _parameterManager.Get<IMySpecialInfo>().ToList();
            Assert.AreEqual(2, specialInfos.Count);
            Assert.IsTrue(specialInfos.Contains(_mockMySpecialInfo));
            Assert.IsTrue(specialInfos.Contains(_mockMyVerySpecialInfo));

            var verySpecialInfos = _parameterManager.Get<IMyVerySpecialInfo>().ToList();
            Assert.AreEqual(1, verySpecialInfos.Count);
            Assert.AreEqual(_mockMyVerySpecialInfo, verySpecialInfos[0]);
        }

        [Test]
        public void GetSortedIEnumerable()
        {
            var info1 = new MockMySpecialInfo { Identifier = "b" };
            var info2 = new MockMySpecialInfo { Identifier = "0" };
            var info3 = new MockMySpecialInfo { Identifier = "a" };
            var info4 = new MockMySpecialInfo { Identifier = "1" };

            _parameterManager.Load<IMySpecialInfo, MockMySpecialInfo>(info1, info1.Identifier, "guid1");
            _parameterManager.Load<IMySpecialInfo, MockMySpecialInfo>(info2, info2.Identifier, "guid2");
            _parameterManager.Load<IMySpecialInfo, MockMySpecialInfo>(info3, info3.Identifier, "guid3");
            _parameterManager.Load<IMySpecialInfo, MockMySpecialInfo>(info4, info4.Identifier, "guid4");

            var infos = _parameterManager.GetSorted<IMySpecialInfo>().ToArray();
            Assert.AreEqual(4, infos.Length);
            Assert.AreEqual(info2, infos[0]);
            Assert.AreEqual(info4, infos[1]);
            Assert.AreEqual(info3, infos[2]);
            Assert.AreEqual(info1, infos[3]);
        }

        [Test]
        public void LoadUpdatedObject()
        {
            LoadInfos();

            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.Get<IMySpecialInfo>(VerySpecialId));
            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.Get(VerySpecialId, typeof(IMySpecialInfo)));
            Assert.AreEqual(_mockMyVerySpecialInfo, _parameterManager.GetWithGUID<IMySpecialInfo>(VerySpecialGuid));
            Assert.AreEqual(_mockKeyValueStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));

            // override an existing loaded object
            // load new object with the same identifier & guid
            var newInfo = new MockMyVerySpecialInfo();
            _parameterManager.Load<IMySpecialInfo, MockMyVerySpecialInfo>(newInfo, VerySpecialId, VerySpecialGuid);
            _parameterManager.Load<IMyVerySpecialInfo, MockMyVerySpecialInfo>(newInfo, VerySpecialId, VerySpecialGuid);

            var newStruct = new MockKeyValueStruct(_parameterManager, "new struct", 100, "guid", new string[0]);
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(newStruct, StructGuid);

            Assert.AreEqual(newInfo, _parameterManager.Get<IMySpecialInfo>(VerySpecialId));
            Assert.AreEqual(newInfo, _parameterManager.Get(VerySpecialId, typeof(IMySpecialInfo)));
            Assert.AreEqual(newInfo, _parameterManager.GetWithGUID<IMySpecialInfo>(VerySpecialGuid));
            Assert.AreEqual(newStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));

            var specialInfos = _parameterManager.Get<IMySpecialInfo>().ToList();
            Assert.AreEqual(2, specialInfos.Count);
            Assert.IsTrue(specialInfos.Contains(_mockMySpecialInfo));
            Assert.IsTrue(specialInfos.Contains(newInfo));

            var verySpecialInfos = _parameterManager.Get<IMyVerySpecialInfo>().ToList();
            Assert.AreEqual(1, verySpecialInfos.Count);
            Assert.AreEqual(newInfo, verySpecialInfos[0]);
        }

        [Test]
        public void LoadUpdatedObject_GuidChange()
        {
            LoadInfos();

            // currently not supported
            // loading of object with same identifier but different guid
            var newInfo = new MockMyVerySpecialInfo();
            var newGuid = "myNewGuid";

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<IMySpecialInfo, MockMyVerySpecialInfo>(newInfo, VerySpecialId, newGuid);

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<IMyVerySpecialInfo, MockMyVerySpecialInfo>(newInfo, VerySpecialId, newGuid);
        }

        [Test]
        public void LoadUpdatedObject_IdentifierChange()
        {
            LoadInfos();

            // currently not supported
            // loading of object with different identifier but same guid
            var newInfo = new MockMyVerySpecialInfo();
            var newIdentifier = "myNewIdentifier";

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<IMySpecialInfo, MockMyVerySpecialInfo>(newInfo, newIdentifier, VerySpecialGuid);

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<IMyVerySpecialInfo, MockMyVerySpecialInfo>(newInfo, newIdentifier, VerySpecialGuid);
        }

        [Test]
        public void RemoveAll()
        {
            LoadInfos();

            _parameterManager.RemoveAll();

            AssertEmptyManager();
        }

        [Test]
        public void ApplyOverrides_BaseCases()
        {
            LoadInfos();
            IReadOnlyList<string> errors;
            var success = _parameterManager.ApplyOverrides(null, out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            success = _parameterManager.ApplyOverrides(JObject.Parse("{}"), out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            Assert.AreEqual(0, _mockMySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.RemoveAllEditCalls);
        }

        [Test]
        public void ApplyOverrides_ErrorChecking()
        {
            bool success;
            IReadOnlyList<string> errors;

            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"delete\":[]}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Override action delete isn't supported yet.", errors[0]);

            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"add\":[]}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Override action add isn't supported yet.", errors[0]);

            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"jump\":[]}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Override action jump isn't valid.", errors[0]);

            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":[[\"hello\"]]}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("ParameterManager expects 4 elements in the array of data.", errors[0]);


            LogAssert.Expect(LogType.Error, $"Missing: Cannot find parameter by GUID {SpecialId} for type IMySpecialInfo");
            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                       "[" +
                                                       "  [\"MySpecialInfo.csv\"," +
                                                       $"  \"{SpecialId}\"," +
                                                       "   \"SomeColumnName\"," +
                                                       "   \"SomeValue\"]" +
                                                       "]" +
                                                       "}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual($"Cannot find parameter for csv [MySpecialInfo.csv] and identifier/guid [{SpecialId}].", errors[0]);

            LoadInfos();
            Assert.AreEqual(0, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockMySpecialInfo.EditPropertyCalls);

            // set an error to be returned by the info
            _mockMySpecialInfo.ReturnEditPropertyError = "some error";


            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                       "[" +
                                                       "  [\"MySpecialInfo.csv\"," +
                                                       $"   \"{SpecialId}\"," +
                                                       "   \"SomeColumnName\"," +
                                                       "   \"SomeValue\"]" +
                                                       "]" +
                                                       "}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual($"Error editing (IMySpecialInfo)[{SpecialId}] property [SomeColumnName] with value [SomeValue]: {_mockMySpecialInfo.ReturnEditPropertyError}", errors[0]);

            // assert calls to the info
            Assert.AreEqual(0, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockMySpecialInfo.EditPropertyCalls);
            Assert.AreEqual("SomeColumnName", _mockMySpecialInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue", _mockMySpecialInfo.EditPropertyValue);
        }

        [Test]
        public void ApplyAndClearOverrides()
        {
            LoadInfos();
            _parameterManager.ClearAllOverrides();
            Assert.AreEqual(0, _mockMySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.RemoveAllEditCalls);

            // apply to one row
            var success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                               "[" +
                                                               "  [\"MySpecialInfo.csv\"," +
                                                               $"  \"{SpecialId}\"," +
                                                               "   \"SomeColumnName1\"," +
                                                               "   \"SomeValue1\"]," +
                                                               "  [\"KeyValueStruct.csv\"," +
                                                               $"  \"{StructGuid}\"," +
                                                               "   \"SomeColumnName2\"," +
                                                               "   \"SomeValue2\"]" +
                                                               "]" +
                                                               "}"), out IReadOnlyList<string> errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            // assert calls to the info
            Assert.AreEqual(1, _mockMySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual("SomeColumnName1", _mockMySpecialInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue1", _mockMySpecialInfo.EditPropertyValue);

            // assert calls to the struct
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(0, _mockKeyValueStruct.RemoveAllEditCalls);
            Assert.AreEqual("SomeColumnName2", _mockKeyValueStruct.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue2", _mockKeyValueStruct.EditPropertyValue);

            _parameterManager.ClearAllOverrides();

            Assert.AreEqual(1, _mockMySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(1, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.RemoveAllEditCalls);

            // apply to both rows
            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                       "[" +
                                                       "  [\"MySpecialInfo.csv\"," +
                                                       $"  \"{SpecialId}\"," +
                                                       "   \"SomeColumnName\"," +
                                                       "   \"SomeValue\"]," +
                                                       "  [\"MyVerySpecialInfo.csv\"," +
                                                       $"  \"{VerySpecialId}\"," +
                                                       "   \"SomeColumnName2\"," +
                                                       "   \"SomeValue2\"]" +
                                                       "]" +
                                                       "}"), out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            // assert calls to the info
            Assert.AreEqual(2, _mockMySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(1, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockMyVerySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockMyVerySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual("SomeColumnName", _mockMySpecialInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue", _mockMySpecialInfo.EditPropertyValue);
            Assert.AreEqual("SomeColumnName2", _mockMyVerySpecialInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue2", _mockMyVerySpecialInfo.EditPropertyValue);

            // assert no new calls to the struct
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.RemoveAllEditCalls);

            _parameterManager.ClearAllOverrides();

            Assert.AreEqual(2, _mockMySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(2, _mockMySpecialInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockMyVerySpecialInfo.EditPropertyCalls);
            Assert.AreEqual(1, _mockMyVerySpecialInfo.RemoveAllEditCalls);
        }

        [Test]
        public void UnsafeGetError()
        {
            _parameterManager.IsGettingSafe = false;

            var regex = new Regex($".*{nameof(IParameterManager.IsGettingSafe)}.*");

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsNull(_parameterManager.Get<IMySpecialInfo>(SpecialId));
            Assert.IsTrue(_parameterManager.HasGetBeenCalled);

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsNull(_parameterManager.Get(SpecialId, typeof(IMySpecialInfo)));

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsEmpty(_parameterManager.Get<IMySpecialInfo>());

            LogAssert.Expect(LogType.Error, regex);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<IMySpecialInfo>(SpecialGuid));

            LogAssert.Expect(LogType.Error, regex);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsEmpty(_parameterManager.GetSorted<IMySpecialInfo>());

            // no more errors
            _parameterManager.IsGettingSafe = true;
            Assert.IsNull(_parameterManager.Get<IMySpecialInfo>(SpecialId));
        }
    }
}

