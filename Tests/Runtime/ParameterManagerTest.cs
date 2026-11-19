using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Util;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class ParameterManagerTest
    {
        private MockSubclassAInfo _mockSubclassAInfo;
        private MockSubclassBInfo _mockSubclassBInfo;
        private MockKeyValueStruct _mockKeyValueStruct;

        private const string BaseStructErrorMsg = "Cannot use IBaseStruct as type.";
        private const string BaseInfoErrorMsg = "Cannot use IBaseInfo as type.";

        private const string SubclassAId = "id1";
        private const string SubclassAGuid = "guid1";

        private const string SubclassBId = "id2";
        private const string SubclassBGuid = "guid2";

        private const string KeyValueStructKeyPath = "KeyValueStruct[SomeInfo]";
        private static readonly string KeyValueStructGuid = HashKeyPath(KeyValueStructKeyPath);

        private ParameterManager _parameterManager;

        private static string HashKeyPath(string keyPath)
        {
            using (var guidConverter = new GuidConverter())
            {
                return guidConverter.ToGuid(keyPath);
            }
        }

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
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(_mockKeyValueStruct, KeyValueStructGuid);

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
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid));
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

            Assert.AreEqual(_mockKeyValueStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid));

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>("non existing guid"));
        }

        [Test]
        public void InvalidUsesWithIBaseInfo()
        {
            LoadInfos();
            LogAssert.Expect(LogType.Error, BaseInfoErrorMsg);
            Assert.IsNull(_parameterManager.Get<IBaseInfo>("some_identifier"));

            LogAssert.Expect(LogType.Error, BaseInfoErrorMsg);
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

            LogAssert.Expect(LogType.Error, BaseStructErrorMsg);
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
        public void GetSortedIsOrdinal()
        {
            MockSubclassAInfo lowerB = new("b");
            MockSubclassAInfo underscore = new("_x");
            MockSubclassAInfo upperB = new("B");
            MockSubclassAInfo lowerA = new("a");

            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(lowerB, lowerB.Identifier, "guid1");
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(underscore, underscore.Identifier, "guid2");
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(upperB, upperB.Identifier, "guid3");
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(lowerA, lowerA.Identifier, "guid4");

            // ordinal: uppercase (0x42) < underscore (0x5F) < lowercase (0x61+), unlike a culture-aware sort
            CollectionAssert.AreEqual(new[] { upperB, underscore, lowerA, lowerB },
                _parameterManager.GetSorted<ISubInterfaceAInfo>().ToArray());
        }

        [Test]
        public void LoadUpdatedObject()
        {
            LoadInfos();

            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassBId));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.Get(SubclassBId, typeof(ISubInterfaceAInfo)));
            Assert.AreEqual(_mockSubclassBInfo, _parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassBGuid));
            Assert.AreEqual(_mockKeyValueStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid));

            // override an existing loaded object
            // load new object with the same identifier & guid
            var newInfo = new MockSubclassBInfo(SubclassBId);
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassBInfo>(newInfo, newInfo.Identifier, SubclassBGuid);
            _parameterManager.Load<ISubInterfaceBInfo, MockSubclassBInfo>(newInfo, newInfo.Identifier, SubclassBGuid);

            var newStruct = new MockKeyValueStruct(_parameterManager, "new struct", 100, "guid", new string[0]);
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(newStruct, KeyValueStructGuid);

            Assert.AreEqual(newInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassBId));
            Assert.AreEqual(newInfo, _parameterManager.Get(SubclassBId, typeof(ISubInterfaceAInfo)));
            Assert.AreEqual(newInfo, _parameterManager.GetWithGUID<ISubInterfaceAInfo>(SubclassBGuid));
            Assert.AreEqual(newStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid));

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

        #region override json

        // single quoted json, which Json.NET reads, from an object's properties
        private static string Json(params string[] properties) => "{" + string.Join(", ", properties) + "}";

        // one row of an add or edit, in one csv
        private static string Row(string action, string csvName, string key, string values) =>
            $"'{action}': {{'{csvName}': {{'{key}': {values}}}}}";

        private static string Edit(string csvName, string key, string column, string value) =>
            Row("edit", csvName, key, $"{{'{column}': '{value}'}}");

        private static string Delete(string csvName, params string[] keys) =>
            $"'delete': {{'{csvName}': ['{string.Join("', '", keys)}']}}";

        private static string InfoRow(string identifier) => $"'{identifier}': {{'Name': 'name of {identifier}'}}";

        // info rows added to one csv, each with a name
        private static string AddInfo(string csvName, params string[] identifiers) =>
            $"'add': {{'{csvName}': {Json(identifiers.Select(InfoRow).ToArray())}}}";

        private static string RowFormatError(string action, string csvName, string key) =>
            $"Override {action} for csv [{csvName}] row [{key}] expects an object of column values that aren't arrays or objects.";

        private bool Apply(string json, out IReadOnlyList<string> errors) =>
            _parameterManager.ApplyOverrides(json, out errors);

        private void AssertApplied(string json)
        {
            Assert.IsTrue(Apply(json, out var errors), errors == null ? null : string.Join("\n", errors));
            Assert.IsNull(errors);
        }

        private void AssertFailed(string json, string expectedError)
        {
            Assert.IsFalse(Apply(json, out var errors));
            Assert.Contains(expectedError, errors.ToList());
        }

        #endregion

        [Test]
        public void ApplyOverrides_BaseCases()
        {
            LoadInfos();
            IReadOnlyList<string> errors;
            var success = _parameterManager.ApplyOverrides(null, out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            success = _parameterManager.ApplyOverrides("", out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            success = _parameterManager.ApplyOverrides("{}", out errors);
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

            success = _parameterManager.ApplyOverrides("{\"delete\":{}}", out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            success = _parameterManager.ApplyOverrides("{\"add\":{}}", out errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);

            success = _parameterManager.ApplyOverrides("{\"jump\":{}}", out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Override action jump isn't valid.", errors[0]);

            success = _parameterManager.ApplyOverrides("{\"edit\":[[\"hello\"]]}", out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Override action edit expects an object keyed by csv name.", errors[0]);

            // json that doesn't parse, isn't an object, or repeats a key is one error
            foreach (var json in new[] { "{\"edit\":", "[]", "{\"edit\":{}, \"edit\":{}}" })
            {
                success = _parameterManager.ApplyOverrides(json, out errors);
                Assert.IsFalse(success);
                Assert.AreEqual(1, errors.Count);
                StringAssert.StartsWith("Overrides json isn't valid: ", errors[0]);
            }

            success = _parameterManager.ApplyOverrides(Json(Edit("MySpecialInfo.csv", SubclassAId, "SomeColumnName", "SomeValue")), out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual($"Cannot find parameter for csv [MySpecialInfo.csv] and identifier/guid [{SubclassAId}].", errors[0]);

            LoadInfos();
            Assert.AreEqual(0, _mockSubclassAInfo.RemoveAllEditCalls);
            Assert.AreEqual(0, _mockSubclassAInfo.EditPropertyCalls);

            // set an error to be returned by the info
            _mockSubclassAInfo.ReturnEditPropertyError = "some error";


            success = _parameterManager.ApplyOverrides(Json(Edit("SubInterfaceAInfo.csv", SubclassAId, "SomeColumnName", "SomeValue")), out errors);
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
        public void ApplyOverrides_StructKeyPathAddressing()
        {
            // structs bake hashed key path guids; override payloads address them by raw key path
            const string keyPath = "KeyValueStruct[SomeInfo].SomeProperty";
            var hashedStruct = new MockKeyValueStruct(_parameterManager, "desc", 20, "guid", new string[0]);
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(hashedStruct, HashKeyPath(keyPath));

            var success = _parameterManager.ApplyOverrides(Json(Edit("KeyValueStruct.csv", keyPath, "SomeColumnName", "SomeValue")),
                out IReadOnlyList<string> errors);
            Assert.IsTrue(success);
            Assert.IsNull(errors);
            Assert.AreEqual(1, hashedStruct.EditPropertyCalls);
            Assert.AreEqual("SomeColumnName", hashedStruct.EditPropertyPropertyName);
            Assert.AreEqual("SomeValue", hashedStruct.EditPropertyValue);

            // a failing second edit reverts the key path addressed edit through the same resolution
            success = _parameterManager.ApplyOverrides("{'edit': {" +
                                                       $"'KeyValueStruct.csv': {{'{keyPath}': {{'OtherColumnName': 'SomeValue'}}}}, " +
                                                       "'MySpecialInfo.csv': {'MissingId': {'SomeColumnName': 'SomeValue'}}" +
                                                       "}}", out errors);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(1, hashedStruct.RevertEditPropertyCalls);
        }

        [Test]
        public void ApplyOverrides_PartialFailure_RevertsSuccessful()
        {
            LoadInfos();

            // first edit will succeed, second will fail
            _mockSubclassBInfo.ReturnEditPropertyError = "some error";

            var success = _parameterManager.ApplyOverrides("{'edit': {" +
                                                           $"'SubInterfaceAInfo.csv': {{'{SubclassAId}': {{'SomeColumnName': 'SomeValue'}}}}, " +
                                                           $"'SubInterfaceBInfo.csv': {{'{SubclassBId}': {{'SomeColumnName2': 'SomeValue2'}}}}" +
                                                           "}}", out IReadOnlyList<string> errors);
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
        public void ApplyOverrides_FailedCallKeepsEarlierEdit()
        {
            LoadInfos();
            AssertApplied(Json(Edit("SubInterfaceAInfo.csv", SubclassAId, "SomeColumn", "5")));

            // a row refuses a second edit of the same property, and the failed call leaves the first one alone
            AssertFailed(Json(Edit("SubInterfaceAInfo.csv", SubclassAId, "SomeColumn", "7")),
                $"Error editing ({nameof(ISubInterfaceAInfo)})[{SubclassAId}] property [SomeColumn] with value [7]: Overrides is already set.");

            Assert.IsTrue(_mockSubclassAInfo.EditedValues.TryGetValue("SomeColumn", out var value));
            Assert.AreEqual("5", value);
            Assert.AreEqual(0, _mockSubclassAInfo.RevertEditPropertyCalls);
        }

        [Test]
        public void ApplyOverrides_FailedCallEditingPropertyTwiceRollsBackOnce()
        {
            LoadInfos();

            // the same property, by identifier and by guid
            Assert.IsFalse(Apply("{'edit': {'SubInterfaceAInfo.csv': {" +
                                 $"'{SubclassAId}': {{'SomeColumn': '1'}}, " +
                                 $"'{SubclassAGuid}': {{'SomeColumn': '2'}}, " +
                                 "'missing': {'SomeColumn': 'x'}}}}", out var errors));

            // rows apply in ordinal order, so the guid's edit applies first, the identifier's is refused, and the
            // guid's is undone once
            CollectionAssert.AreEqual(new[]
            {
                $"Error editing ({nameof(ISubInterfaceAInfo)})[{SubclassAId}] property [SomeColumn] with value [1]: Overrides is already set.",
                "Cannot find parameter for csv [SubInterfaceAInfo.csv] and identifier/guid [missing]."
            }, errors);
            Assert.IsFalse(_mockSubclassAInfo.EditedValues.ContainsKey("SomeColumn"));
            Assert.AreEqual(1, _mockSubclassAInfo.RevertEditPropertyCalls);
        }

        [Test]
        public void ApplyOverrides_MalformedEditReportsLaterErrors()
        {
            LoadInfos();

            Assert.IsFalse(Apply("{'edit': {'SubInterfaceAInfo.csv': {'hello': 'x', 'missing': {'SomeColumn': 'x'}}}}",
                out var errors));

            CollectionAssert.AreEqual(new[]
            {
                RowFormatError("edit", "SubInterfaceAInfo.csv", "hello"),
                "Cannot find parameter for csv [SubInterfaceAInfo.csv] and identifier/guid [missing]."
            }, errors);
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
            var success = _parameterManager.ApplyOverrides("{'edit': {" +
                                                           $"'SubInterfaceAInfo.csv': {{'{SubclassAId}': {{'SomeColumnName1': 'SomeValue1'}}}}, " +
                                                           $"'KeyValueStruct.csv': {{'{KeyValueStructKeyPath}': {{'SomeColumnName2': 'SomeValue2'}}}}" +
                                                           "}}", out IReadOnlyList<string> errors);
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
            success = _parameterManager.ApplyOverrides("{'edit': {" +
                                                       $"'SubInterfaceAInfo.csv': {{'{SubclassAId}': {{'SomeColumnName': 'SomeValue'}}}}, " +
                                                       $"'SubInterfaceBInfo.csv': {{'{SubclassBId}': {{'SomeColumnName2': 'SomeValue2'}}}}" +
                                                       "}}", out errors);
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
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid));

            LogAssert.Expect(LogType.Error, regex);
            Assert.IsEmpty(_parameterManager.GetSorted<ISubInterfaceAInfo>());

            // no more errors
            _parameterManager.IsGettingSafe = true;
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
        }

        #region add & delete overrides

        private const string NewId = "newId";
        private const string InnerKeyPath = "KeyValueStruct[New].InnerStruct";
        private const string OuterKeyPath = "KeyValueStruct[New]";

        [Test]
        public void ApplyOverrides_AddInfo()
        {
            LoadInfos();
            var factory = new FakeParameterFactory();
            _parameterManager.ParameterFactory = factory;

            AssertApplied(Json(AddInfo("SubInterfaceBInfo.csv", NewId)));

            // registered under its own and its base interface
            var added = (MockAddedSubclassBInfo)_parameterManager.Get<ISubInterfaceBInfo>(NewId);
            Assert.IsNotNull(added);
            Assert.AreSame(added, _parameterManager.Get<ISubInterfaceAInfo>(NewId));
            Assert.AreSame(added, _parameterManager.Get(NewId, typeof(ISubInterfaceAInfo)));
            Assert.AreEqual("name of newId", added.EditedValues["Name"]);

            // enumerations include it, sorted ordinally
            CollectionAssert.AreEquivalent(new IBaseInfo[] { _mockSubclassAInfo, _mockSubclassBInfo, added },
                _parameterManager.Get<ISubInterfaceAInfo>().ToArray());
            CollectionAssert.AreEqual(new[] { SubclassAId, SubclassBId, NewId },
                _parameterManager.GetSorted<ISubInterfaceAInfo>().Select(info => info.Identifier).ToArray());
        }

        [Test]
        public void ApplyOverrides_EditAddedRowFails()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();
            var edit = Edit("SubInterfaceAInfo.csv", NewId, "SomeColumn", "SomeValue");
            var error = $"Cannot edit (ISubInterfaceAInfo)[{NewId}] property [SomeColumn]: rows added by overrides are read-only. Set the value in the add.";

            // in the same call as the add, which is rolled back with it
            AssertFailed(Json(AddInfo("SubInterfaceAInfo.csv", NewId), edit), error);
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(NewId));

            // in a later call
            AssertApplied(Json(AddInfo("SubInterfaceAInfo.csv", NewId)));
            AssertFailed(Json(edit), error);
            // only the add's own edit
            Assert.AreEqual(1, ((MockAddedSubclassAInfo)_parameterManager.Get<ISubInterfaceAInfo>(NewId)).EditPropertyCalls);
        }

        [Test]
        public void ApplyOverrides_AddInfoWithNoBakedRows()
        {
            _parameterManager.ParameterFactory = new FakeParameterFactory();

            AssertApplied(Json(AddInfo("SubInterfaceAInfo.csv", NewId)));

            var added = _parameterManager.Get<ISubInterfaceAInfo>(NewId);
            Assert.IsNotNull(added);
            Assert.AreEqual(1, _parameterManager.Get<ISubInterfaceAInfo>().Count());
        }

        [Test]
        public void ApplyOverrides_AddStructs()
        {
            _parameterManager.ParameterFactory = new FakeParameterFactory();

            // the outer struct points to an inner struct added in the same call
            AssertApplied("{'add': {" +
                          $"'KeyValueStruct.csv': {{'{OuterKeyPath}': {{'Description': 'outer', 'Value': '1', 'InnerStruct': '{InnerKeyPath}'}}}}, " +
                          $"'InnerKeyValueStruct.csv': {{'{InnerKeyPath}': {{'Description': 'inner', 'Value': '2'}}}}" +
                          "}}");

            var outer = _parameterManager.GetStructWithGuid<IKeyValueStruct>(OuterKeyPath);
            Assert.AreSame(outer, _parameterManager.GetStructWithGuid<IKeyValueStruct>(HashKeyPath(OuterKeyPath)));
            Assert.AreEqual("outer", outer.Description);
            Assert.AreEqual(1, outer.Value);

            var inner = outer.InnerStruct.Struct;
            Assert.AreSame(inner, _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(InnerKeyPath));
            Assert.AreEqual("inner", inner.Description);
            Assert.AreEqual(2, inner.Value);
        }

        [Test]
        public void ApplyOverrides_AddErrors()
        {
            LoadInfos();
            var json = Json(AddInfo("SubInterfaceAInfo.csv", NewId));

            AssertFailed(json, "No parameter factory set. Regenerate parameter code.");

            // the missing factory is reported once, however many rows the call adds
            Assert.IsFalse(Apply(Json(AddInfo("SubInterfaceAInfo.csv", NewId, "other")), out var twoAddErrors));
            Assert.AreEqual(1, twoAddErrors.Count(message => message == "No parameter factory set. Regenerate parameter code."));

            // with no factory, an empty add succeeds and a bad row still reports its format error
            var badRow = Json(Row("add", "SubInterfaceAInfo.csv", NewId, "'x'"));
            AssertApplied("{'add': {}}");
            AssertFailed(badRow, RowFormatError("add", "SubInterfaceAInfo.csv", NewId));

            var factory = new FakeParameterFactory();
            _parameterManager.ParameterFactory = factory;
            AssertFailed(Json(AddInfo("UnknownInfo.csv", NewId)),
                "Cannot find parameter type for csv [UnknownInfo.csv].");
            AssertFailed(badRow, RowFormatError("add", "SubInterfaceAInfo.csv", NewId));
            AssertFailed("{'add': {'SubInterfaceAInfo.csv': ['x']}}",
                "Override add for csv [SubInterfaceAInfo.csv] expects an object of rows.");

            // loaded data can't have an empty identifier or key path, so an add can't either
            AssertFailed(Json(Row("add", "SubInterfaceAInfo.csv", "", "{'Name': 'x'}")),
                "Cannot add parameter for csv [SubInterfaceAInfo.csv]: the identifier or key path is empty.");
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(""));

            // every column is required, including when no values are given
            AssertFailed(Json(Row("add", "SubInterfaceAInfo.csv", NewId, "{}")),
                $"Error adding csv [SubInterfaceAInfo.csv] identifier/key path [{NewId}]: Expected 1 columns (Name), got 0.");
            AssertFailed(Json(Row("add", "SubInterfaceAInfo.csv", NewId, "{'Other': 'x'}")),
                $"Error adding csv [SubInterfaceAInfo.csv] identifier/key path [{NewId}]: Other: property doesn't exist");

            factory.EditError = "bad value";
            AssertFailed(json, $"Error adding csv [SubInterfaceAInfo.csv] identifier/key path [{NewId}]: Name: bad value");

            // nothing was left behind
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(NewId));
            Assert.AreEqual(2, _parameterManager.Get<ISubInterfaceAInfo>().Count());
        }

        [Test]
        public void ApplyOverrides_AddCollisions()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();

            string Collision(string csvName, string key) =>
                $"Cannot add parameter for csv [{csvName}] and identifier/key path [{key}]: it already exists.";

            // same type, and a base interface of the added type
            AssertFailed(Json(AddInfo("SubInterfaceAInfo.csv", SubclassAId)), Collision("SubInterfaceAInfo.csv", SubclassAId));
            AssertFailed(Json(AddInfo("SubInterfaceBInfo.csv", SubclassAId)), Collision("SubInterfaceBInfo.csv", SubclassAId));

            // twice in one call: a repeated key, then the same row through its base interface's csv.  Csvs apply in
            // ordinal order, so the base interface's csv adds the row and the other csv collides with it.
            Assert.IsFalse(Apply(Json(AddInfo("SubInterfaceAInfo.csv", NewId, NewId)), out var errors));
            StringAssert.StartsWith("Overrides json isn't valid: ", errors.Single());
            AssertFailed("{'add': {" +
                         $"'SubInterfaceBInfo.csv': {Json(InfoRow(NewId))}, " +
                         $"'SubInterfaceAInfo.csv': {Json(InfoRow(NewId))}" +
                         "}}",
                Collision("SubInterfaceBInfo.csv", NewId));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(NewId));

            // across calls
            AssertApplied(Json(AddInfo("SubInterfaceAInfo.csv", NewId)));
            AssertFailed(Json(AddInfo("SubInterfaceAInfo.csv", NewId)), Collision("SubInterfaceAInfo.csv", NewId));
        }

        [Test]
        public void ApplyOverrides_Delete()
        {
            LoadInfos();

            // by identifier, under a base interface csv
            AssertApplied(Json(Delete("SubInterfaceAInfo.csv", SubclassBId)));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassBId));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceBInfo>(SubclassBId));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetWithGUID<ISubInterfaceBInfo>(SubclassBGuid));
            Assert.IsEmpty(_parameterManager.Get<ISubInterfaceBInfo>());
            CollectionAssert.AreEqual(new[] { _mockSubclassAInfo }, _parameterManager.GetSorted<ISubInterfaceAInfo>().ToArray());

            // by guid
            AssertApplied(Json(Delete("SubInterfaceAInfo.csv", SubclassAGuid)));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));

            AssertFailed(Json(Delete("SubInterfaceAInfo.csv", "missing")),
                "Cannot find parameter for csv [SubInterfaceAInfo.csv] and identifier/guid [missing].");

            // a struct row, by key path, which a second delete can't find
            var deleteStruct = Json(Delete("KeyValueStruct.csv", KeyValueStructKeyPath));
            AssertApplied(deleteStruct);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructKeyPath));
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            Assert.IsNull(_parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid));
            AssertFailed(deleteStruct,
                $"Cannot delete csv [KeyValueStruct.csv] identifier/guid [{KeyValueStructKeyPath}]: it's already deleted.");

            AssertFailed("{'delete': {'SubInterfaceAInfo.csv': 'x'}}",
                "Override delete for csv [SubInterfaceAInfo.csv] expects an array of identifiers, guids or key paths.");
        }

        [Test]
        public void ApplyOverrides_DeleteThenEditFails()
        {
            LoadInfos();

            AssertFailed(Json(Delete("SubInterfaceAInfo.csv", SubclassAId), Edit("SubInterfaceAInfo.csv", SubclassAId, "SomeColumn", "SomeValue")),
                $"Cannot edit (ISubInterfaceAInfo)[{SubclassAId}] property [SomeColumn]: the row is deleted by overrides.");

            // the delete was rolled back
            Assert.AreSame(_mockSubclassAInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
        }

        [Test]
        public void ApplyOverrides_DeleteAfterEditFails()
        {
            LoadInfos();
            var edit = Edit("SubInterfaceAInfo.csv", SubclassAId, "SomeColumn", "SomeValue");
            var delete = Json(Delete("SubInterfaceAInfo.csv", SubclassAId));

            // an edit from an earlier call keeps the row
            AssertApplied(Json(edit));
            AssertFailed(delete,
                $"Cannot delete csv [SubInterfaceAInfo.csv] identifier/guid [{SubclassAId}]: it's edited by overrides.");
            Assert.AreSame(_mockSubclassAInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));

            // an edit that was rolled back doesn't
            _parameterManager.ClearAllOverrides();
            AssertFailed(Json(edit, Delete("SubInterfaceAInfo.csv", "missing")),
                "Cannot find parameter for csv [SubInterfaceAInfo.csv] and identifier/guid [missing].");
            AssertApplied(delete);
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
        }

        [Test]
        public void ApplyOverrides_DeleteTwiceFails()
        {
            LoadInfos();
            var delete = Json(Delete("SubInterfaceAInfo.csv", SubclassAId));
            var error = $"Cannot delete csv [SubInterfaceAInfo.csv] identifier/guid [{SubclassAId}]: it's already deleted.";

            // in the same call, which is rolled back
            AssertFailed(Json(Delete("SubInterfaceAInfo.csv", SubclassAId, SubclassAId)), error);
            Assert.AreSame(_mockSubclassAInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));

            // across calls, by identifier and by guid; the first delete stays
            AssertApplied(delete);
            AssertFailed(delete, error);
            AssertFailed(Json(Delete("SubInterfaceAInfo.csv", SubclassAGuid)),
                $"Cannot delete csv [SubInterfaceAInfo.csv] identifier/guid [{SubclassAGuid}]: it's already deleted.");
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
        }

        [Test]
        public void ApplyOverrides_DeleteAndAddSameRowFails()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();
            var delete = Delete("SubInterfaceAInfo.csv", SubclassAId);
            var deletedError = $"Cannot add parameter for csv [SubInterfaceAInfo.csv] and identifier/key path [{SubclassAId}]: " +
                               "it's deleted. Edit the row instead of deleting and adding it.";

            // in the same call, which is rolled back
            AssertFailed(Json(delete, AddInfo("SubInterfaceAInfo.csv", SubclassAId)), deletedError);
            Assert.AreSame(_mockSubclassAInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));

            // across calls
            AssertApplied(Json(delete));
            AssertFailed(Json(AddInfo("SubInterfaceAInfo.csv", SubclassAId)), deletedError);

            // a struct at a deleted info's key path collides with the loaded struct, which stays
            var structKeyPath = $"SubInterfaceAInfo[{SubclassAId}].Struct";
            var bakedStruct = new MockKeyValueStruct(_parameterManager, "baked", 1, "guid", new string[0]);
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(bakedStruct, HashKeyPath(structKeyPath));
            AssertFailed(Json(Row("add", "KeyValueStruct.csv", structKeyPath, $"{{'Description': 'added', 'Value': '2', 'InnerStruct': '{InnerKeyPath}'}}")),
                $"Cannot add parameter for csv [KeyValueStruct.csv] and identifier/key path [{structKeyPath}]: it already exists.");
            Assert.AreSame(bakedStruct, _parameterManager.GetStructWithGuid<IKeyValueStruct>(structKeyPath));

            // a row added by overrides can't be deleted
            AssertApplied(Json(AddInfo("SubInterfaceAInfo.csv", NewId)));
            AssertFailed(Json(Delete("SubInterfaceAInfo.csv", NewId)),
                $"Cannot delete csv [SubInterfaceAInfo.csv] identifier/guid [{NewId}]: it was added by overrides, so leave it out of the add instead.");
            Assert.IsNotNull(_parameterManager.Get<ISubInterfaceAInfo>(NewId));
        }

        [Test]
        public void ApplyOverrides_FailedCallRollsBackEverything()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();

            // an earlier call stays applied
            AssertApplied(Json(AddInfo("SubInterfaceAInfo.csv", "kept")));

            AssertFailed(Json(Delete("SubInterfaceBInfo.csv", SubclassBId),
                    AddInfo("SubInterfaceAInfo.csv", NewId),
                    "'edit': {'SubInterfaceAInfo.csv': {" +
                    $"'{SubclassAId}': {{'SomeColumn': 'SomeValue'}}, " +
                    "'missing': {'SomeColumn': 'SomeValue'}}}"),
                "Cannot find parameter for csv [SubInterfaceAInfo.csv] and identifier/guid [missing].");

            Assert.AreSame(_mockSubclassBInfo, _parameterManager.Get<ISubInterfaceBInfo>(SubclassBId));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(NewId));
            Assert.AreEqual(1, _mockSubclassAInfo.RevertEditPropertyCalls);
            Assert.IsNotNull(_parameterManager.Get<ISubInterfaceAInfo>("kept"));
        }

        [Test]
        public void ApplyOverrides_MalformedEntriesRollBack()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();
            var delete = Delete("SubInterfaceBInfo.csv", SubclassBId);
            var add = AddInfo("SubInterfaceAInfo.csv", NewId);
            var editError = RowFormatError("edit", "SubInterfaceAInfo.csv", SubclassAId);

            // a malformed entry is an error, not an exception, so the deletes and adds before it are rolled back
            AssertFailed(Json(delete, add, "'edit': {'SubInterfaceAInfo.csv': ['a']}"),
                "Override edit for csv [SubInterfaceAInfo.csv] expects an object of rows.");
            AssertFailed(Json(delete, add, Row("edit", "SubInterfaceAInfo.csv", SubclassAId, "'x'")), editError);
            AssertFailed(Json(delete, add, Row("edit", "SubInterfaceAInfo.csv", SubclassAId, "{'SomeColumn': ['a']}")), editError);
            AssertFailed(Json(delete, Row("add", "SubInterfaceAInfo.csv", NewId, "{'Name': ['a', 'b']}")),
                RowFormatError("add", "SubInterfaceAInfo.csv", NewId));
            AssertFailed(Json(add, "'delete': {'SubInterfaceBInfo.csv': [{'id': 'x'}]}"),
                "Override delete for csv [SubInterfaceBInfo.csv] expects an array of identifiers, guids or key paths.");
            AssertFailed(Json(add, "'delete': [['SubInterfaceBInfo.csv', 'x']]"), "Override action delete expects an object keyed by csv name.");

            Assert.AreSame(_mockSubclassBInfo, _parameterManager.Get<ISubInterfaceBInfo>(SubclassBId));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(NewId));

            // an edit value that isn't a string is still read as text
            AssertApplied(Json(Row("edit", "SubInterfaceAInfo.csv", SubclassAId, "{'SomeColumn': 5}")));
            Assert.AreEqual("5", _mockSubclassAInfo.EditPropertyValue);
        }

        [Test]
        public void ApplyOverrides_JsonKeyOrderDoesntChangeTheResult()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();

            // the same adds, with csvs and rows in two orders: added rows come after the loaded ones, by csv and then
            // by identifier, in ordinal order
            foreach (var json in new[]
                     {
                         "{'add': {" +
                         $"'SubInterfaceBInfo.csv': {Json(InfoRow("b"))}, " +
                         $"'SubInterfaceAInfo.csv': {Json(InfoRow("c"), InfoRow("a"))}" +
                         "}}",
                         "{'add': {" +
                         $"'SubInterfaceAInfo.csv': {Json(InfoRow("a"), InfoRow("c"))}, " +
                         $"'SubInterfaceBInfo.csv': {Json(InfoRow("b"))}" +
                         "}}",
                     })
            {
                AssertApplied(json);
                CollectionAssert.AreEqual(new[] { SubclassAId, SubclassBId, "a", "c", "b" },
                    _parameterManager.Get<ISubInterfaceAInfo>().Select(info => info.Identifier).ToArray());
                _parameterManager.ClearAllOverrides();
            }

            // the same failing overrides, with actions, csvs, rows, columns and deletes in two orders: the errors come
            // out deletes first, then edits, each by csv, row and column in ordinal order
            _mockSubclassAInfo.ReturnEditPropertyError = "some error";
            string editRow = $"'{SubclassAId}': {{'B': 'v', 'A': 'v'}}";
            string editRowSorted = $"'{SubclassAId}': {{'A': 'v', 'B': 'v'}}";
            string[] expectedErrors =
            {
                "Cannot find parameter for csv [SubInterfaceAInfo.csv] and identifier/guid [m1].",
                "Cannot find parameter for csv [SubInterfaceAInfo.csv] and identifier/guid [m2].",
                "Cannot find parameter for csv [MissingInfo.csv] and identifier/guid [y].",
                $"Error editing (ISubInterfaceAInfo)[{SubclassAId}] property [A] with value [v]: some error",
                $"Error editing (ISubInterfaceAInfo)[{SubclassAId}] property [B] with value [v]: some error",
            };
            foreach (var json in new[]
                     {
                         "{'edit': {" +
                         $"'SubInterfaceAInfo.csv': {{{editRow}}}, " +
                         "'MissingInfo.csv': {'y': {'C': 'v'}}" +
                         "}, " + Delete("SubInterfaceAInfo.csv", "m2", "m1") + "}",
                         "{" + Delete("SubInterfaceAInfo.csv", "m1", "m2") + ", 'edit': {" +
                         "'MissingInfo.csv': {'y': {'C': 'v'}}, " +
                         $"'SubInterfaceAInfo.csv': {{{editRowSorted}}}" +
                         "}}",
                     })
            {
                Assert.IsFalse(Apply(json, out var errors));
                CollectionAssert.AreEqual(expectedErrors, errors);
            }
        }

        [Test]
        public void ApplyOverrides_ClearRemovesAddsAndDeletes()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();
            var json = Json(Delete("SubInterfaceAInfo.csv", SubclassAId), AddInfo("SubInterfaceAInfo.csv", NewId));
            AssertApplied(json);

            _parameterManager.ClearAllOverrides();

            Assert.AreSame(_mockSubclassAInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
            Assert.IsNull(_parameterManager.Get<ISubInterfaceAInfo>(NewId));
            AssertApplied(json);
        }

        [Test]
        public void RemoveAll_ClearsAddsAndDeletes()
        {
            LoadInfos();
            _parameterManager.ParameterFactory = new FakeParameterFactory();
            AssertApplied(Json(Delete("SubInterfaceAInfo.csv", SubclassAId), AddInfo("SubInterfaceAInfo.csv", NewId)));

            // a reload that now has the added row, and still has the deleted one
            _parameterManager.RemoveAll();
            LoadInfos();
            var reloaded = new MockSubclassAInfo(NewId);
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(reloaded, NewId, "new guid");

            Assert.AreSame(_mockSubclassAInfo, _parameterManager.Get<ISubInterfaceAInfo>(SubclassAId));
            Assert.AreSame(reloaded, _parameterManager.Get<ISubInterfaceAInfo>(NewId));
            Assert.AreEqual(3, _parameterManager.Get<ISubInterfaceAInfo>().Count());

            // the reloaded row can be deleted again
            AssertApplied(Json(Delete("SubInterfaceAInfo.csv", SubclassAId)));
        }

        [Test]
        public void RemoveAll_ForgetsEditedRows()
        {
            LoadInfos();
            AssertApplied(Json(Edit("SubInterfaceAInfo.csv", SubclassAId, "SomeColumn", "SomeValue")));

            // RemoveAll clears the overrides once, then doesn't keep the removed row, so clearing them again
            // doesn't reach it
            _parameterManager.RemoveAll();
            Assert.AreEqual(1, _mockSubclassAInfo.RemoveAllEditCalls);
            _parameterManager.ClearAllOverrides();
            Assert.AreEqual(1, _mockSubclassAInfo.RemoveAllEditCalls);
        }

        #endregion
    }
}

