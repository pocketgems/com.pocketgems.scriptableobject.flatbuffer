using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Util;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public enum LinkedParameterManagerSetUpType
    {
        // construct the LinkedParameterManager from a regular ParameterManager
        ConstructFromStandard,
        // construct the LinkedParameterManager from a another LinkedParameterManager
        ConstructFromLinked
    }

    [TestFixture(LinkedParameterManagerSetUpType.ConstructFromStandard)]
    [TestFixture(LinkedParameterManagerSetUpType.ConstructFromLinked)]
    public class LinkedParameterManagerTest
    {
        private readonly LinkedParameterManagerSetUpType _setUpType;

        private ParameterManager _parameterManager;
        private LinkedParameterManager _linkedParameterManager;

        private const string Item1SubclassAId = "id1";
        private const string Item1SubclassAGuid = "guid1";

        private const string Item2SubclassBId = "id2";
        private const string Item2SubclassBGuid = "guid2";

        private const string Item3SubclassBId = "id3";
        private const string Item3SubclassBGuid = "guid3";

        private const string KeyValueStructKeyPath = "KeyValueStruct[SomeInfo]";
        private static readonly string KeyValueStructGuid = HashKeyPath(KeyValueStructKeyPath);
        private const string StructDesc = "desc";
        private const int StructValue = 10;

        private static string HashKeyPath(string keyPath)
        {
            using (var guidConverter = new GuidConverter())
            {
                return guidConverter.ToGuid(keyPath);
            }
        }

        public LinkedParameterManagerTest(LinkedParameterManagerSetUpType setUpType)
        {
            _setUpType = setUpType;
        }

        [SetUp]
        public void SetUp()
        {
            // the linked managers copy the factory when they're built
            _parameterManager = new ParameterManager { ParameterFactory = new FakeParameterFactory() };

            switch (_setUpType)
            {
                case LinkedParameterManagerSetUpType.ConstructFromStandard:
                    _linkedParameterManager = new LinkedParameterManager(_parameterManager);
                    break;
                case LinkedParameterManagerSetUpType.ConstructFromLinked:
                    _linkedParameterManager = new LinkedParameterManager(new LinkedParameterManager(_parameterManager));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // single item info for type
            var item1 = new MockSubclassAInfo(Item1SubclassAId);
            // two items info for type
            var item2 = new MockSubclassBInfo(Item2SubclassBId);
            var item3 = new MockSubclassBInfo(Item3SubclassBId);
            _parameterManager.Load<ISubInterfaceAInfo, MockSubclassAInfo>(item1, item1.Identifier, Item1SubclassAGuid);
            _parameterManager.Load<ISubInterfaceBInfo, MockSubclassBInfo>(item2, item2.Identifier, Item2SubclassBGuid);
            _parameterManager.Load<ISubInterfaceBInfo, MockSubclassBInfo>(item3, item3.Identifier, Item3SubclassBGuid);

            // struct
            var mockKeyValueStruct = new MockKeyValueStruct(_parameterManager, StructDesc, StructValue, "guid", new string[0]);
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(mockKeyValueStruct, KeyValueStructGuid);
        }

        [Test]
        public void Get()
        {
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>("NonExistentId"), Is.Null);

            void Get<T>(string identifier) where T : class, IBaseInfo
            {
                var originalInfo = _parameterManager.Get<T>(identifier);
                var linkedInfo = _linkedParameterManager.Get<T>(identifier);

                // info data match
                Assert.That(originalInfo.Identifier, Is.EqualTo(identifier));
                Assert.That(linkedInfo.Identifier, Is.EqualTo(identifier));

                // infos are not the exact same info
                Assert.That(linkedInfo != originalInfo, Is.True);

                // querying the info more than once returns the same info
                Assert.That(linkedInfo == _linkedParameterManager.Get<T>(identifier), Is.True);
            }

            Get<ISubInterfaceAInfo>(Item1SubclassAId);
            Get<ISubInterfaceBInfo>(Item2SubclassBId);
            Get<ISubInterfaceBInfo>(Item3SubclassBId);
        }

        [Test]
        public void TryGet()
        {
            void TryGet<T>(string identifier, bool expectExist = true) where T : class, IBaseInfo
            {
                var originalResult = _parameterManager.TryGet<T>(identifier, out var originalInfo);
                var linkedResult = _linkedParameterManager.TryGet<T>(identifier, out var linkedInfo);

                Assert.That(linkedResult, Is.EqualTo(originalResult));

                if (expectExist)
                {
                    Assert.That(originalInfo, Is.Not.Null);
                    Assert.That(linkedInfo, Is.Not.Null);

                    // info data match
                    Assert.That(originalInfo.Identifier, Is.EqualTo(identifier));
                    Assert.That(linkedInfo.Identifier, Is.EqualTo(identifier));

                    // infos are not the exact same info
                    Assert.That(linkedInfo != originalInfo, Is.True);

                    // querying the info more than once returns the same info
                    Assert.That(_linkedParameterManager.TryGet<T>(identifier, out var anotherLinkedInfo), Is.True);
                    Assert.That(anotherLinkedInfo, Is.EqualTo(linkedInfo));
                }
                else
                {
                    Assert.That(linkedResult, Is.False);
                    Assert.That(linkedInfo, Is.Null);
                }
            }

            TryGet<ISubInterfaceAInfo>("NonExistentId", false);
            TryGet<ISubInterfaceAInfo>(Item1SubclassAId);
            TryGet<ISubInterfaceBInfo>(Item2SubclassBId);
            TryGet<ISubInterfaceBInfo>(Item3SubclassBId);
        }

        [Test]
        public void GetIEnumerable()
        {
            void Get<T>() where T : class, IBaseInfo
            {
                var originalInfos = _parameterManager.Get<T>().ToList();
                var linkedInfos = _linkedParameterManager.Get<T>().ToList();

                // count matches
                Assert.That(originalInfos.Count, Is.EqualTo(linkedInfos.Count));

                // info data match
                for (int i = 0; i < originalInfos.Count; i++)
                {
                    Assert.That(originalInfos[i].Identifier, Is.EqualTo(linkedInfos[i].Identifier));
                }

                // infos in linked manager are not of the same instance as the original source
                foreach (var linkedInfo in linkedInfos)
                    Assert.That(originalInfos, Is.Not.Contains(linkedInfo));

                // querying the info more than once returns the same info
                var otherLInkedInfos = _linkedParameterManager.Get<T>().ToList();
                Assert.That(otherLInkedInfos.Count, Is.EqualTo(linkedInfos.Count));
                for (int i = 0; i < linkedInfos.Count; i++)
                    Assert.That(linkedInfos[i], Is.EqualTo(otherLInkedInfos[i]));
            }

            Get<ISubInterfaceAInfo>();
            Get<ISubInterfaceBInfo>();
        }

        [Test]
        public void GetSortedIEnumerable()
        {
            void GetSorted<T>() where T : class, IBaseInfo
            {
                var originalInfos = _parameterManager.GetSorted<T>().ToList();
                var linkedInfos = _linkedParameterManager.GetSorted<T>().ToList();

                // count matches
                Assert.That(originalInfos.Count, Is.EqualTo(linkedInfos.Count));

                // info data match
                for (int i = 0; i < originalInfos.Count; i++)
                {
                    Assert.That(originalInfos[i].Identifier, Is.EqualTo(linkedInfos[i].Identifier));
                }

                // infos in linked manager are not of the same instance as the original source
                foreach (var linkedInfo in linkedInfos)
                    Assert.That(originalInfos, Is.Not.Contains(linkedInfo));

                // querying the info more than once returns the same info
                var otherLInkedInfos = _linkedParameterManager.GetSorted<T>().ToList();
                Assert.That(otherLInkedInfos.Count, Is.EqualTo(linkedInfos.Count));
                for (int i = 0; i < linkedInfos.Count; i++)
                    Assert.That(linkedInfos[i], Is.EqualTo(otherLInkedInfos[i]));
            }

            GetSorted<ISubInterfaceAInfo>();
            GetSorted<ISubInterfaceBInfo>();
        }

        [Test]
        public void GetWithGUID()
        {
            LogAssert.Expect(LogType.Error, $"Cannot find {nameof(ISubInterfaceAInfo)} with GUID NonExistentGuid: the row was removed, or its data isn't loaded");
            Assert.That(_linkedParameterManager.GetWithGUID<ISubInterfaceAInfo>("NonExistentGuid"), Is.Null);

            void GetWithGUID<T>(string identifier, string guid) where T : class, IBaseInfo
            {
                var originalInfo = _parameterManager.GetWithGUID<T>(guid);
                var linkedInfo = _linkedParameterManager.GetWithGUID<T>(guid);

                // info data match
                Assert.That(originalInfo.Identifier, Is.EqualTo(identifier));
                Assert.That(linkedInfo.Identifier, Is.EqualTo(identifier));

                // infos are not the exact same info
                Assert.That(linkedInfo != originalInfo, Is.True);

                // querying the info more than once returns the same info
                Assert.That(linkedInfo == _linkedParameterManager.GetWithGUID<T>(guid), Is.True);
            }

            GetWithGUID<ISubInterfaceAInfo>(Item1SubclassAId, Item1SubclassAGuid);
            GetWithGUID<ISubInterfaceBInfo>(Item2SubclassBId, Item2SubclassBGuid);
            GetWithGUID<ISubInterfaceBInfo>(Item3SubclassBId, Item3SubclassBGuid);
        }

        [Test]
        public void GetStructWithGuid()
        {
            var originalStruct = _parameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid);
            var linkedStruct = _linkedParameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid);

            // struct data matches
            Assert.That(originalStruct.Description, Is.EqualTo(StructDesc));
            Assert.That(originalStruct.Value, Is.EqualTo(StructValue));
            Assert.That(linkedStruct.Description, Is.EqualTo(StructDesc));
            Assert.That(linkedStruct.Value, Is.EqualTo(StructValue));

            // infos are not the exact same info
            Assert.That(linkedStruct != originalStruct, Is.True);

            // querying the info more than once returns the same info
            Assert.That(linkedStruct == _linkedParameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid), Is.True);
        }

        [UnityTest]
        public IEnumerator GarbageCollection_CleanCache()
        {
            const int checkGarbageCollectedFrequencySeconds = 1;

            switch (_setUpType)
            {
                case LinkedParameterManagerSetUpType.ConstructFromStandard:
                    _linkedParameterManager = new LinkedParameterManager(_parameterManager, checkGarbageCollectedFrequencySeconds);
                    break;
                case LinkedParameterManagerSetUpType.ConstructFromLinked:
                    _linkedParameterManager = new LinkedParameterManager(new LinkedParameterManager(_parameterManager), checkGarbageCollectedFrequencySeconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var firstInfo = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(1));
            WeakReference weakReference;
            {
                // get a different info within this scope so it's garbage collected afterwards
                var secondInfo = _linkedParameterManager.Get<ISubInterfaceBInfo>(Item2SubclassBId);
                Assert.That(secondInfo, Is.Not.Null);
                Assert.That(secondInfo.Identifier, Is.EqualTo(Item2SubclassBId));
                weakReference = new WeakReference(secondInfo);
                Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(2));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            yield return new WaitUntil(() =>
            {
                // garbage collect
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // wait for garbage collection frequency
                if (stopwatch.ElapsedMilliseconds < checkGarbageCollectedFrequencySeconds * 1000)
                    return false;

                // something bad happened - timeout
                if (stopwatch.ElapsedMilliseconds >= (checkGarbageCollectedFrequencySeconds + 3) * 1000)
                    return true;

                // wait until the 2nd info is garbage collected
                return !weakReference.IsAlive;
            });

            // info should be garbage collected by now
            Assert.That(weakReference.IsAlive, Is.False);

            // get the first info again to trigger a cache clearing of the garbage collected second info
            var firstInfoAgain = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            Assert.That(firstInfoAgain, Is.EqualTo(firstInfo));

            // second info should be removed from the cache
            Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(1));

            // re-cache the 2nd info again
            var secondInfoAgain = _linkedParameterManager.Get<ISubInterfaceBInfo>(Item2SubclassBId);
            Assert.That(secondInfoAgain, Is.Not.Null);
            Assert.That(secondInfoAgain.Identifier, Is.EqualTo(Item2SubclassBId));
            Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator GarbageCollection_Recache()
        {
            WeakReference weakReference;
            {
                // get a different info within this scope so it's garbage collected afterwards
                var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
                Assert.That(item1Info, Is.Not.Null);
                Assert.That(item1Info.Identifier, Is.EqualTo(Item1SubclassAId));
                weakReference = new WeakReference(item1Info);
                Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(1));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            yield return new WaitUntil(() =>
            {
                // garbage collect
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // something bad happened - timeout
                if (stopwatch.ElapsedMilliseconds >= 1000)
                    return true;

                // wait until the 2nd info is garbage collected
                return !weakReference.IsAlive;
            });

            // info should be garbage collected by now
            Assert.That(weakReference.IsAlive, Is.False);

            // the garbage collected info is still in the cache because we have yet hit the frequency to check it for clearing
            Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(1));

            // get the first info again to update the cache
            var firstInfoAgain = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            Assert.That(firstInfoAgain, Is.Not.Null);
            Assert.That(firstInfoAgain.Identifier, Is.EqualTo(Item1SubclassAId));

            // the entry in the cache should be updated with the newly constructed info
            Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(1));
        }

        [Test]
        public void ApplyAndClearOverrides()
        {
            // apply to one row & one struct
            var success = _linkedParameterManager.ApplyOverrides("{'edit': {" +
                                                                 $"'SubInterfaceAInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName1': 'SomeValue1'}}}}, " +
                                                                 $"'KeyValueStruct.csv': {{'{KeyValueStructKeyPath}': {{'SomeColumnName2': 'SomeValue2'}}}}" +
                                                                 "}}", out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // assert calls to the info on get
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            var mockedItem1Info = (MockSubclassAInfo)item1Info;
            Assert.That(mockedItem1Info.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedItem1Info.RemoveAllEditCalls, Is.EqualTo(0));
            Assert.That(mockedItem1Info.EditPropertyPropertyName, Is.EqualTo("SomeColumnName1"));
            Assert.That(mockedItem1Info.EditPropertyValue, Is.EqualTo("SomeValue1"));

            // assert calls to the struct on get
            var structInfo = _linkedParameterManager.GetStructWithGuid<IKeyValueStruct>(KeyValueStructGuid);
            var mockedStructInfo = (MockKeyValueStruct)structInfo;
            Assert.That(mockedStructInfo.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedStructInfo.RemoveAllEditCalls, Is.EqualTo(0));
            Assert.That(mockedStructInfo.EditPropertyPropertyName, Is.EqualTo("SomeColumnName2"));
            Assert.That(mockedStructInfo.EditPropertyValue, Is.EqualTo("SomeValue2"));

            var item2Info = _linkedParameterManager.Get<ISubInterfaceBInfo>(Item2SubclassBId);
            var mockedItem2Info = (MockSubclassBInfo)item2Info;
            var item3Info = _linkedParameterManager.Get<ISubInterfaceBInfo>(Item3SubclassBId);
            var mockedItem3Info = (MockSubclassBInfo)item3Info;

            // clear overrides
            _linkedParameterManager.ClearAllOverrides();

            Assert.That(mockedItem1Info.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedItem1Info.RemoveAllEditCalls, Is.EqualTo(1));

            Assert.That(mockedItem2Info.EditPropertyCalls, Is.EqualTo(0));
            // called removed even though there are no overrides on this because it's been cached
            Assert.That(mockedItem2Info.RemoveAllEditCalls, Is.EqualTo(1));

            Assert.That(mockedItem3Info.EditPropertyCalls, Is.EqualTo(0));
            // called removed even though there are no overrides on this because it's been cached
            Assert.That(mockedItem3Info.RemoveAllEditCalls, Is.EqualTo(1));

            Assert.That(mockedStructInfo.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedStructInfo.RemoveAllEditCalls, Is.EqualTo(1));

            // apply to both types
            success = _linkedParameterManager.ApplyOverrides("{'edit': {" +
                                                             $"'SubInterfaceAInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName': 'SomeValue'}}}}, " +
                                                             $"'SubInterfaceBInfo.csv': {{'{Item2SubclassBId}': {{'SomeColumnName2': 'SomeValue2'}}}}" +
                                                             "}}", out errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // assert calls to the cached info
            Assert.That(mockedItem1Info.EditPropertyCalls, Is.EqualTo(2));
            Assert.That(mockedItem1Info.RemoveAllEditCalls, Is.EqualTo(1));
            Assert.That(mockedItem2Info.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedItem2Info.RemoveAllEditCalls, Is.EqualTo(1));
            Assert.That(mockedItem1Info.EditPropertyPropertyName, Is.EqualTo("SomeColumnName"));
            Assert.That(mockedItem1Info.EditPropertyValue, Is.EqualTo("SomeValue"));
            Assert.That(mockedItem2Info.EditPropertyPropertyName, Is.EqualTo("SomeColumnName2"));
            Assert.That(mockedItem2Info.EditPropertyValue, Is.EqualTo("SomeValue2"));

            // assert no new calls to the struct
            Assert.That(mockedStructInfo.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedStructInfo.RemoveAllEditCalls, Is.EqualTo(1));

            _linkedParameterManager.ClearAllOverrides();

            Assert.That(mockedItem1Info.EditPropertyCalls, Is.EqualTo(2));
            Assert.That(mockedItem1Info.RemoveAllEditCalls, Is.EqualTo(2));

            Assert.That(mockedItem2Info.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedItem2Info.RemoveAllEditCalls, Is.EqualTo(2));

            Assert.That(mockedItem3Info.EditPropertyCalls, Is.EqualTo(0));
            // called removed even though there are no overrides on this because it's been cached
            Assert.That(mockedItem3Info.RemoveAllEditCalls, Is.EqualTo(2));

            Assert.That(mockedStructInfo.EditPropertyCalls, Is.EqualTo(1));
            // called removed even though there are no overrides on this because it's been cached
            Assert.That(mockedStructInfo.RemoveAllEditCalls, Is.EqualTo(2));
        }

        [Test]
        public void ApplyAndClearOverridesNoOp()
        {
            // apply to one row
            var success = _linkedParameterManager.ApplyOverrides(
                $"{{'edit': {{'SubInterfaceAInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName1': 'SomeValue1'}}}}}}}}",
                out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // clear the override
            _linkedParameterManager.ClearAllOverrides();

            // assert no calls to the info on get since the parameter is lazily created on Get.
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            var mockedItem1Info = (MockSubclassAInfo)item1Info;
            Assert.That(mockedItem1Info.EditPropertyCalls, Is.EqualTo(0));
            Assert.That(mockedItem1Info.RemoveAllEditCalls, Is.EqualTo(0));
        }

        [Test]
        public void ApplyOverrides_MissingError()
        {
            var success = _linkedParameterManager.ApplyOverrides(
                $"{{'edit': {{'MySpecialInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName': 'SomeValue'}}}}}}}}", out var errors);
            Assert.That(success, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo($"Cannot find parameter for csv [MySpecialInfo.csv] and identifier/guid [{Item1SubclassAId}]."));
        }

        [Test]
        public void ApplyOverrides_StructKeyPathAddressing()
        {
            // structs bake hashed key path guids; override payloads address them by raw key path
            const string keyPath = "KeyValueStruct[SomeInfo].SomeProperty";
            var hashedGuid = HashKeyPath(keyPath);
            var hashedStruct = new MockKeyValueStruct(_parameterManager, StructDesc, StructValue, "guid", new string[0]);
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(hashedStruct, hashedGuid);

            var success = _linkedParameterManager.ApplyOverrides(
                $"{{'edit': {{'KeyValueStruct.csv': {{'{keyPath}': {{'SomeColumnName': 'SomeValue'}}}}}}}}", out var errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // the override is lazily applied to the linked instance on get
            var structInfo = _linkedParameterManager.GetStructWithGuid<IKeyValueStruct>(hashedGuid);
            var mockedStructInfo = (MockKeyValueStruct)structInfo;
            Assert.That(mockedStructInfo.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedStructInfo.EditPropertyPropertyName, Is.EqualTo("SomeColumnName"));
            Assert.That(mockedStructInfo.EditPropertyValue, Is.EqualTo("SomeValue"));

            // the source struct is untouched
            Assert.That(hashedStruct.EditPropertyCalls, Is.EqualTo(0));
        }

        [Test]
        public void GetStructWithGuid_Missing()
        {
            LogAssert.Expect(LogType.Error, $"Cannot find {nameof(IKeyValueStruct)} with GUID NonExistentGuid: the row was removed, or its data isn't loaded");
            Assert.That(_linkedParameterManager.GetStructWithGuid<IKeyValueStruct>("NonExistentGuid"), Is.Null);
        }

        [Test]
        public void ApplyOverrides_DuplicatePropertyError()
        {
            // two values for the same property on the same parameter, by identifier and by guid.  Rows apply in
            // ordinal order, so the guid row applies first and the identifier row fails.
            var success = _linkedParameterManager.ApplyOverrides("{'edit': {'SubInterfaceAInfo.csv': {" +
                                                                 $"'{Item1SubclassAId}': {{'SomeColumnName': 'SomeValue1'}}, " +
                                                                 $"'{Item1SubclassAGuid}': {{'SomeColumnName': 'SomeValue2'}}" +
                                                                 "}}}", out var errors);
            Assert.That(success, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo($"({nameof(ISubInterfaceAInfo)})[{Item1SubclassAId}] has more than one value for [SomeColumnName] assigned "));

            // the successful first override was rolled back, so nothing is lazily applied on get
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            var mockedItem1Info = (MockSubclassAInfo)item1Info;
            Assert.That(mockedItem1Info.EditPropertyCalls, Is.EqualTo(0));
        }

        // the same edit in a csv that has the row and one that doesn't
        private static readonly string EditInTwoCsvs =
            "{'edit': {" +
            $"'SubInterfaceAInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName': 'SomeValue'}}}}, " +
            $"'MySpecialInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName': 'SomeValue'}}}}" +
            "}}";

        [Test]
        public void ApplyOverrides_RollbackRevertsCachedParameter()
        {
            // get info to pre-cache
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            var mockedItem1Info = (MockSubclassAInfo)item1Info;

            // the MySpecialInfo.csv edit fails and the SubInterfaceAInfo.csv edit succeeds, triggering a revert of it
            var success = _linkedParameterManager.ApplyOverrides(EditInTwoCsvs, out var errors);
            Assert.That(success, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo($"Cannot find parameter for csv [MySpecialInfo.csv] and identifier/guid [{Item1SubclassAId}]."));

            // the edit was applied to the cached parameter then reverted
            Assert.That(mockedItem1Info.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedItem1Info.RevertEditPropertyCalls, Is.EqualTo(1));
            Assert.That(mockedItem1Info.RevertEditPropertyPropertyName, Is.EqualTo("SomeColumnName"));
        }

        [Test]
        public void ApplyOverrides_RollbackRevertError()
        {
            // get info to pre-cache
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            var mockedItem1Info = (MockSubclassAInfo)item1Info;

            // set an error to be returned by the info when reverting
            mockedItem1Info.ReturnRevertEditPropertyError = "some revert error";

            // the MySpecialInfo.csv edit fails and the SubInterfaceAInfo.csv edit succeeds, and reverting it also errors
            var success = _linkedParameterManager.ApplyOverrides(EditInTwoCsvs, out var errors);
            Assert.That(success, Is.False);
            Assert.That(errors.Count, Is.EqualTo(2));
            Assert.That(errors[0], Is.EqualTo($"Cannot find parameter for csv [MySpecialInfo.csv] and identifier/guid [{Item1SubclassAId}]."));
            Assert.That(errors[1], Is.EqualTo($"Error reverting edit ({nameof(ISubInterfaceAInfo)})[{Item1SubclassAId}] property [SomeColumnName]: {mockedItem1Info.ReturnRevertEditPropertyError}"));
        }

        [Test]
        public void ApplyOverrides_FailedCachedEditIsNotKept()
        {
            // get info to pre-cache, and make its edit fail
            var mockedItem1Info = (MockSubclassAInfo)_linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            mockedItem1Info.ReturnEditPropertyError = "some error";
            string Edit(string value) =>
                $"{{'edit': {{'SubInterfaceAInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName': '{value}'}}}}}}}}";

            Assert.That(_linkedParameterManager.ApplyOverrides(Edit("bad"), out _), Is.False);

            // the failed edit left nothing behind, so the property can be edited again
            mockedItem1Info.ReturnEditPropertyError = null;
            var success = _linkedParameterManager.ApplyOverrides(Edit("good"), out var errors);
            Assert.That(success, Is.True, errors == null ? null : string.Join("\n", errors));
            Assert.That(mockedItem1Info.EditedValues["SomeColumnName"], Is.EqualTo("good"));
        }

        private static readonly string EditItem1 =
            $"{{'edit': {{'SubInterfaceAInfo.csv': {{'{Item1SubclassAId}': {{'SomeColumnName': 'SomeValue'}}}}}}}}";

        [Test]
        public void ApplyOverrides_CachedParameterError()
        {
            // get info to pre-cache
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            var mockedItem1Info = (MockSubclassAInfo)item1Info;

            // set an error to be returned by the info
            mockedItem1Info.ReturnEditPropertyError = "some error";

            // apply to one row
            var success = _linkedParameterManager.ApplyOverrides(EditItem1, out IReadOnlyList<string> errors);
            Assert.That(success, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo($"Error editing (ISubInterfaceAInfo)[{Item1SubclassAId}] property [SomeColumnName] with value [SomeValue]: {mockedItem1Info.ReturnEditPropertyError}"));
        }

        [Test]
        public void ApplyOverrides_GetParameterError()
        {
            // load into the parameter manager a class that will always error on adding an override
            var originalItem1Info = new MockErrorSubclassAInfo(Item1SubclassAId);
            _parameterManager.Load<ISubInterfaceAInfo, MockErrorSubclassAInfo>(originalItem1Info, originalItem1Info.Identifier, Item1SubclassAGuid);

            // apply to one row successfully since the adata isn't actually applied immediately
            var success = _linkedParameterManager.ApplyOverrides(EditItem1, out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // upon getting, the override will be applied and the error will surface
            LogAssert.Expect(LogType.Error, $"Error editing (MockErrorSubclassAInfo) property [SomeColumnName] with value [SomeValue]: {originalItem1Info.ReturnEditPropertyError}");
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);

            // sanity check that a new info was lazily created
            Assert.That(item1Info, Is.Not.EqualTo(originalItem1Info));
            Assert.That(item1Info.Identifier, Is.EqualTo(originalItem1Info.Identifier));
        }

        private const string AddedId = "addedId";
        private static readonly string AddJson =
            $"{{'add': {{'SubInterfaceAInfo.csv': {{'{AddedId}': {{'Name': 'added'}}}}}}}}";

        [Test]
        public void ApplyOverrides_AddIsLinked()
        {
            // the linked manager copied the source's factory when it was built, and keeps it
            var sourceFactory = _parameterManager.ParameterFactory;
            Assert.That(_linkedParameterManager.ParameterFactory, Is.SameAs(sourceFactory));
            _parameterManager.ParameterFactory = new FakeParameterFactory();
            Assert.That(_linkedParameterManager.ParameterFactory, Is.SameAs(sourceFactory));

            var success = _linkedParameterManager.ApplyOverrides(AddJson, out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // the added row itself, with the added values
            var added = (MockAddedSubclassAInfo)_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.SameAs(_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId)));
            Assert.That(added, Is.SameAs(_linkedParameterManager.Get<ISubInterfaceAInfo>().Single(info => info.Identifier == AddedId)));
            Assert.That(added.EditedValues["Name"], Is.EqualTo("added"));

            // read-only
            success = _linkedParameterManager.ApplyOverrides(
                $"{{'edit': {{'SubInterfaceAInfo.csv': {{'{AddedId}': {{'SomeColumnName': 'SomeValue'}}}}}}}}", out errors);
            Assert.That(success, Is.False);
            Assert.That(errors, Is.EqualTo(new[] { $"Cannot edit (ISubInterfaceAInfo)[{AddedId}] property [SomeColumnName]: rows added by overrides are read-only. Set the value in the add." }));
            // only the add's own edit
            Assert.That(added.EditPropertyCalls, Is.EqualTo(1));
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>().Count(), Is.EqualTo(2));

            // not visible on the source or on another linked manager
            Assert.That(_parameterManager.Get<ISubInterfaceAInfo>(AddedId), Is.Null);
            Assert.That(new LinkedParameterManager(_parameterManager).Get<ISubInterfaceAInfo>(AddedId), Is.Null);
        }

        [Test]
        public void ApplyOverrides_DeleteIsLinked()
        {
            var success = _linkedParameterManager.ApplyOverrides(
                $"{{'delete': {{'SubInterfaceAInfo.csv': ['{Item1SubclassAId}']}}}}", out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId), Is.Null);
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(), Is.Empty);
            Assert.That(_parameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId), Is.Not.Null);

            // an edit on the deleted row names it, rather than reporting a missing row
            success = _linkedParameterManager.ApplyOverrides(EditItem1, out errors);
            Assert.That(success, Is.False);
            Assert.That(errors, Is.EqualTo(new[] { $"Cannot edit (ISubInterfaceAInfo)[{Item1SubclassAId}] property [SomeColumnName]: the row is deleted by overrides." }));

            _linkedParameterManager.ClearAllOverrides();
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId), Is.Not.Null);
        }

        [Test]
        public void ApplyOverrides_DeleteAfterEditFails()
        {
            var delete = $"{{'delete': {{'SubInterfaceAInfo.csv': ['{Item1SubclassAId}']}}}}";
            Assert.That(_linkedParameterManager.ApplyOverrides(EditItem1, out IReadOnlyList<string> errors), Is.True);

            // an edit from an earlier call keeps the row
            var success = _linkedParameterManager.ApplyOverrides(delete, out errors);
            Assert.That(success, Is.False);
            Assert.That(errors, Is.EqualTo(new[] { $"Cannot delete csv [SubInterfaceAInfo.csv] identifier/guid [{Item1SubclassAId}]: it's edited by overrides." }));
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId), Is.Not.Null);

            // an edit on the source doesn't
            _linkedParameterManager.ClearAllOverrides();
            Assert.That(_parameterManager.ApplyOverrides(EditItem1, out errors), Is.True);
            Assert.That(_linkedParameterManager.ApplyOverrides(delete, out errors), Is.True);
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId), Is.Null);
        }

        [Test]
        public void ApplyOverrides_SourceAddsAndDeletesStayOnTheSource()
        {
            string Json(string name) => $"{{'delete': {{'SubInterfaceAInfo.csv': ['{Item1SubclassAId}']}}," +
                                        $" 'add': {{'SubInterfaceAInfo.csv': {{'{AddedId}': {{'Name': '{name}'}}}}}}}}";

            var success = _parameterManager.ApplyOverrides(Json("source"), out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // the linked manager still has the deleted row and doesn't have the added one
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId), Is.Not.Null);
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId), Is.Null);

            // so it can make the same changes on its own, once
            success = _linkedParameterManager.ApplyOverrides(Json("linked"), out errors);
            Assert.That(success, Is.True);
            Assert.That(((MockAddedSubclassAInfo)_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId)).EditedValues["Name"], Is.EqualTo("linked"));
            Assert.That(((MockAddedSubclassAInfo)_parameterManager.Get<ISubInterfaceAInfo>(AddedId)).EditedValues["Name"], Is.EqualTo("source"));
            success = _linkedParameterManager.ApplyOverrides(Json("again"), out errors);
            Assert.That(success, Is.False);
            Assert.That(errors, Has.Member($"Cannot delete csv [SubInterfaceAInfo.csv] identifier/guid [{Item1SubclassAId}]: it's already deleted."));

            // clearing the source leaves the linked manager's changes
            _parameterManager.ClearAllOverrides();
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId), Is.Null);
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId), Is.Not.Null);
        }

        [Test]
        public void ApplyOverrides_AddedRowsAreNotCopied()
        {
            // the linked manager's own added rows need no copy, so they never enter its copy cache
            var cacheCount = _linkedParameterManager.LinkedMutableParameterCacheCount;
            var success = _linkedParameterManager.ApplyOverrides(AddJson, out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            var added = (MockAddedSubclassAInfo)_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId);
            Assert.That(added, Is.Not.Null);
            Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(cacheCount));

            // clearing removes the row, and its values, which are its edits, aren't cleared
            _linkedParameterManager.ClearAllOverrides();
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId), Is.Null);
            Assert.That(added.RemoveAllEditCalls, Is.EqualTo(0));

            // a failed call removes its added row
            success = _linkedParameterManager.ApplyOverrides(
                $"{{'add': {{'SubInterfaceAInfo.csv': {{'{AddedId}': {{'Name': 'added'}}}}}}," +
                " 'edit': {'SubInterfaceAInfo.csv': {'missing': {'SomeColumnName': 'SomeValue'}}}}", out errors);
            Assert.That(success, Is.False);
            Assert.That(_linkedParameterManager.Get<ISubInterfaceAInfo>(AddedId), Is.Null);
            Assert.That(_linkedParameterManager.LinkedMutableParameterCacheCount, Is.EqualTo(cacheCount));
        }
    }
}
