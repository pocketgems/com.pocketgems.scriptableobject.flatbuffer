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
        private MockSubclassAInfo _mockSubclassAInfo;
        private MockSubclassBInfo _mockSubclassBInfo;
        private MockKeyValueStruct _mockKeyValueStruct;

        private const string BaseInterfaceErrorMsg = "Cannot use IBaseInfo or IBaseStruct as type.";
        private const string BaseStructErrorMsg = "Cannot use IBaseStruct as type.";
        private const string BaseInfoErrorMsg = "Cannot use IBaseInfo as type.";

        private const string SubclassAId = "id1";
        private const string SubclassAGuid = "guid1";

        private const string SubclassBId = "id2";
        private const string SubclassBGuid = "guid2";

        private const string StructGuid = "guid3";

        private ParameterManager _parameterManager;

        [SetUp]
        public void SetUp()
        {
            _parameterManager = new ParameterManager();
            _mockSubclassAInfo = new MockSubclassAInfo(SubclassAId);
            _mockSubclassBInfo = new MockSubclassBInfo(SubclassBId);
            _mockKeyValueStruct = new MockKeyValueStruct(_parameterManager, "desc", 10, "guid", new string[0]);
        }

        private void LoadInfos()
        {
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(_mockKeyValueStruct, StructGuid);

            // load under one interface
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(_mockSubclassAInfo, _mockSubclassAInfo.Identifier, SubclassAGuid);

            // load under two interfaces
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassBInfo>(_mockSubclassBInfo, _mockSubclassBInfo.Identifier, SubclassBGuid);
            _parameterManager.Load<ISubInterfaceBInfo, MockSubclassBInfo>(_mockSubclassBInfo, _mockSubclassBInfo.Identifier, SubclassBGuid);
        }

        private void AssertEmptyManager()
        {
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceBInfo>(SubclassAId));
            Assert.IsNull(_parameterManager.Get(SubclassAId, typeof(ISubInterfaceAInfo)));
            Assert.IsNull(_parameterManager.Get(SubclassAId, typeof(ISubInterfaceBInfo)));

            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassBId));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceBInfo>(SubclassBId));
            Assert.IsNull(_parameterManager.Get(SubclassBId, typeof(ISubInterfaceAInfo)));
            Assert.IsNull(_parameterManager.Get(SubclassBId, typeof(ISubInterfaceBInfo)));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassAGuid));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<ISubInterfaceBInfo>(SubclassAGuid));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassBGuid));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<ISubInterfaceBInfo>(SubclassBGuid));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));
        }

        [Test]
        public void LoadAndGet()
        {
            AssertEmptyManager();
            LoadInfos();

            Assert.AreEqual(_mockSubclassAInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
            Assert.AreEqual(_mockSubclassAInfo, _parameterManager.Get(SubclassAId, typeof(ISubInterfaceAInfo)));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceBInfo>(SubclassAId));
            Assert.IsNull(_parameterManager.Get(SubclassAId, typeof(ISubInterfaceBInfo)));

            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get(SubclassBId, typeof(ISubInterfaceAInfo)));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get(SubclassBId, typeof(ISubInterfaceBInfo)));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassBId));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get<ISubInterfaceBInfo>(SubclassBId));

            Assert.AreEqual(_mockSubclassAInfo, _parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassAGuid));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<ISubInterfaceBInfo>(SubclassAGuid));

            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassBGuid));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.GetWithGUID<ISubInterfaceBInfo>(SubclassBGuid));
        }

        [Test]
        public void LoadAndTryGet()
        {
            AssertEmptyManager();
            LoadInfos();

            Assert.That(_parameterManager.TryGet<ISubInterfaceAInfo>(SubclassAId, out var info1), Is.True);
            Assert.That(info1, Is.EqualTo(_mockSubclassAInfo));

            Assert.That(_parameterManager.TryGet<ISubInterfaceBInfo>(SubclassAId, out var info2), Is.False);
            Assert.That(info2, Is.Null);

            Assert.That(_parameterManager.TryGet<ISubInterfaceAInfo>(SubclassBId, out var info3), Is.True);
            Assert.That(info3, Is.EqualTo(_mockSubclassBInfo));

            Assert.That(_parameterManager.TryGet<ISubInterfaceBInfo>(SubclassBId, out var info4), Is.True);
            Assert.That(info4, Is.EqualTo(_mockSubclassBInfo));
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
            _parameterManager.Load<IBaseInfo, MockSubclassAInfo>(null, "some_id", "some guid");
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

            var specialInfos = _parameterManager.Get<ISubInterfaceAInfo>().ToList();
            Assert.AreEqual(2, specialInfos.Count);
            Assert.IsTrue(specialInfos.Contains(_mockSubclassAInfo));
            Assert.IsTrue(specialInfos.Contains(_mockSubclassBInfo));

            var verySpecialInfos = _parameterManager.Get<ISubInterfaceBInfo>().ToList();
            Assert.AreEqual(1, verySpecialInfos.Count);
            Assert.AreEqual(_mockSubclassBInfo, verySpecialInfos[0]);
        }

        [Test]
        public void GetSortedIEnumerable()
        {
            var info1 = new MockSubclassAInfo("b");
            var info2 = new MockSubclassAInfo("0");
            var info3 = new MockSubclassAInfo("a");
            var info4 = new MockSubclassAInfo("1");

            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(info1, info1.Identifier, "guid1");
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(info2, info2.Identifier, "guid2");
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(info3, info3.Identifier, "guid3");
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(info4, info4.Identifier, "guid4");

            var infos = _parameterManager.GetSorted<ISubInterfaceAInfo>().ToArray();
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

            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassBId));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get(SubclassBId, typeof(ISubInterfaceAInfo)));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassBGuid));
            Assert.AreEqual(_mockKeyValueStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));

            // override an existing loaded object
            // load new object with the same identifier & guid
            var newInfo = new MockSubclassBInfo(SubclassBId);
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassBInfo>(newInfo, newInfo.Identifier, SubclassBGuid);
            _parameterManager.Load<ISubInterfaceBInfo, MockSubclassBInfo>(newInfo, newInfo.Identifier, SubclassBGuid);

            var newStruct = new MockKeyValueStruct(_parameterManager, "new struct", 100, "guid", new string[0]);
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(newStruct, StructGuid);

            Assert.AreEqual(newInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassBId));
            Assert.AreEqual(newInfo, _parameterManager.Get(SubclassBId, typeof(ISubInterfaceAInfo)));
            Assert.AreEqual(newInfo, _parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassBGuid));
            Assert.AreEqual(newStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));

            var specialInfos = _parameterManager.Get<ISubInterfaceAInfo>().ToList();
            Assert.AreEqual(2, specialInfos.Count);
            Assert.IsTrue(specialInfos.Contains(_mockSubclassAInfo));
            Assert.IsTrue(specialInfos.Contains(newInfo));

            var verySpecialInfos = _parameterManager.Get<ISubInterfaceBInfo>().ToList();
            Assert.AreEqual(1, verySpecialInfos.Count);
            Assert.AreEqual(newInfo, verySpecialInfos[0]);
        }

        [Test]
        public void LoadUpdatedObject_GuidChange()
        {
            LoadInfos();

            // currently not supported
            // loading of object with same identifier but different guid
            var newInfo = new MockSubclassBInfo(SubclassBId);

            const string newGuid = "myNewGuid";

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassBInfo>(newInfo, newInfo.Identifier, newGuid);

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<ISubInterfaceBInfo, MockSubclassBInfo>(newInfo, newInfo.Identifier, newGuid);
        }

        [Test]
        public void LoadUpdatedObject_IdentifierChange()
        {
            LoadInfos();

            // currently not supported
            // loading of object with different identifier but same guid
            const string newIdentifier = "myNewIdentifier";
            var newInfo = new MockSubclassBInfo(newIdentifier);

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassBInfo>(newInfo, newIdentifier, SubclassBGuid);

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            _parameterManager.Load<ISubInterfaceBInfo, MockSubclassBInfo>(newInfo, newIdentifier, SubclassBGuid);
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

            Assert.AreEqual(0, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.RemoveAllEditCalls);
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


            LogAssert.Expect(LogType.Error, $"Missing: Cannot find parameter by GUID {SubclassAId} for type IMySpecialInfo");
            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                       "[" +
                                                       "  [\"MySpecialInfo.csv\"," +
                                                       $"  \"{SubclassAId}\"," +
                                                       "   \"SomeColumnName\"," +
                                                       "   \"SomeValue\"]" +
                                                       "]" +
                                                       "}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual($"Cannot find parameter for csv [MySpecialInfo.csv] and identifier/guid [{SubclassAId}].", errors[0]);

            LoadInfos();
            Assert.AreEqual(0, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockSubclassAInfo.EditPropertyCalls);

            // set an error to be returned by the info
            _mockSubclassAInfo.ReturnEditPropertyError = "some error";


            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                       "[" +
                                                       "  [\"SubInterfaceAInfo.csv\"," +
                                                       $"   \"{SubclassAId}\"," +
                                                       "   \"SomeColumnName\"," +
                                                       "   \"SomeValue\"]" +
                                                       "]" +
                                                       "}"), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual($"Error editing (ISubInterfaceAInfo)[{SubclassAId}] property [SomeColumnName] with value [SomeValue]: {_mockSubclassAInfo.ReturnEditPropertyError}", errors[0]);

            // assert calls to the info
            Assert.AreEqual(0, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual("SomeColumnName", _mockSubclassAInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue", _mockSubclassAInfo.EditPropertyValue);
        }

        [Test]
        public void ApplyOverrides_PartialFailure_RevertsSuccessful()
        {
            LoadInfos();

            // first edit will succeed, second will fail
            _mockSubclassBInfo.ReturnEditPropertyError = "some error";

            var success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                           "[" +
                                                           "  [\"SubInterfaceAInfo.csv\"," +
                                                           $"  \"{SubclassAId}\"," +
                                                           "   \"SomeColumnName\"," +
                                                           "   \"SomeValue\"]," +
                                                           "  [\"SubInterfaceBInfo.csv\"," +
                                                           $"  \"{SubclassBId}\"," +
                                                           "   \"SomeColumnName2\"," +
                                                           "   \"SomeValue2\"]" +
                                                           "]" +
                                                           "}"), out IReadOnlyList<string> errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);

            // first edit succeeded then was rolled back
            Assert.AreEqual(1, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual(1, _mockSubclassAInfo.RevertEditPropertyCalls);
            Assert.AreEqual("SomeColumnName", _mockSubclassAInfo.RevertEditPropertyPropertyName);

            // second edit failed — no revert needed
            Assert.AreEqual(1, _mockSubclassBInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.RevertEditPropertyCalls);
        }

        [Test]
        public void ApplyAndClearOverrides()
        {
            LoadInfos();
            _parameterManager.ClearAllOverrides();
            Assert.AreEqual(0, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.RemoveAllEditCalls);

            // apply to one row
            var success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                               "[" +
                                                               $"  [\"SubInterfaceAInfo.csv\"," +
                                                               $"  \"{SubclassAId}\"," +
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
            Assert.AreEqual(1, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.RemoveAllEditCalls);
            Assert.AreEqual("SomeColumnName1", _mockSubclassAInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue1", _mockSubclassAInfo.EditPropertyValue);

            // assert calls to the struct
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(0, _mockKeyValueStruct.RemoveAllEditCalls);
            Assert.AreEqual("SomeColumnName2", _mockKeyValueStruct.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue2", _mockKeyValueStruct.EditPropertyValue);

            // no reverts on success
            Assert.AreEqual(0, _mockSubclassAInfo.RevertEditPropertyCalls);
            Assert.AreEqual(0, _mockKeyValueStruct.RevertEditPropertyCalls);

            _parameterManager.ClearAllOverrides();

            Assert.AreEqual(1, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual(1, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.RemoveAllEditCalls);

            // apply to both rows
            success = _parameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                       "[" +
                                                       "  [\"SubInterfaceAInfo.csv\"," +
                                                       $"  \"{SubclassAId}\"," +
                                                       "   \"SomeColumnName\"," +
                                                       "   \"SomeValue\"]," +
                                                       "  [\"SubInterfaceBInfo.csv\"," +
                                                       $"  \"{SubclassBId}\"," +
                                                       "   \"SomeColumnName2\"," +
                                                       "   \"SomeValue2\"]" +
                                                       "]" +
                                                       "}"), out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            // assert calls to the info
            Assert.AreEqual(2, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual(1, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockSubclassBInfo.EditPropertyCalls);
            Assert.AreEqual(0, _mockSubclassBInfo.RemoveAllEditCalls);
            Assert.AreEqual("SomeColumnName", _mockSubclassAInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue", _mockSubclassAInfo.EditPropertyValue);
            Assert.AreEqual("SomeColumnName2", _mockSubclassBInfo.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue2", _mockSubclassBInfo.EditPropertyValue);

            // assert no new calls to the struct
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.RemoveAllEditCalls);

            _parameterManager.ClearAllOverrides();

            Assert.AreEqual(2, _mockSubclassAInfo.EditPropertyCalls);
            Assert.AreEqual(2, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.EditPropertyCalls);
            Assert.AreEqual(1, _mockKeyValueStruct.RemoveAllEditCalls);
            Assert.AreEqual(1, _mockSubclassBInfo.EditPropertyCalls);
            Assert.AreEqual(1, _mockSubclassBInfo.RemoveAllEditCalls);
        }

        [Test]
        public void UnsafeGetError()
        {
            _parameterManager.IsGettingSafe = false;

            var regex = new Regex($".*{nameof(IParameterManager.IsGettingSafe)}.*");

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
            Assert.IsTrue(_parameterManager.HasGetBeenCalled);

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsNull(_parameterManager.Get(SubclassAId, typeof(ISubInterfaceAInfo)));

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsEmpty(_parameterManager.Get<ISubInterfaceAInfo>());

            LogAssert.Expect(LogType.Error, regex);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassAGuid));

            LogAssert.Expect(LogType.Error, regex);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid));

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsEmpty(_parameterManager.GetSorted<ISubInterfaceAInfo>());

            // no more errors
            _parameterManager.IsGettingSafe = true;
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
        }
    }
}

