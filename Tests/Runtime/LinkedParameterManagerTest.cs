using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PocketGems.Parameters.Interface;
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

        private const string StructGuid = "guid4";
        private const string StructDesc = "desc";
        private const int StructValue = 10;

        public LinkedParameterManagerTest(LinkedParameterManagerSetUpType setUpType)
        {
            _setUpType = setUpType;
        }

        [SetUp]
        public void SetUp()
        {
            _parameterManager = new ParameterManager();

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
            _parameterManager.Load<IKeyValueStruct, MockKeyValueStruct>(mockKeyValueStruct, StructGuid);
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
            LogAssert.Expect(LogType.Error, $"Bug: Cannot find parameter by GUID NonExistentGuid for type {nameof(ISubInterfaceAInfo)}");
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
            var originalStruct = _parameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid);
            var linkedStruct = _linkedParameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid);

            // struct data matches
            Assert.That(originalStruct.Description, Is.EqualTo(StructDesc));
            Assert.That(originalStruct.Value, Is.EqualTo(StructValue));
            Assert.That(linkedStruct.Description, Is.EqualTo(StructDesc));
            Assert.That(linkedStruct.Value, Is.EqualTo(StructValue));

            // infos are not the exact same info
            Assert.That(linkedStruct != originalStruct, Is.True);

            // querying the info more than once returns the same info
            Assert.That(linkedStruct == _linkedParameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid), Is.True);
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
            var success = _linkedParameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                                               "[" +
                                                                               $"  [\"SubInterfaceAInfo.csv\"," +
                                                                               $"  \"{Item1SubclassAId}\"," +
                                                                               "   \"SomeColumnName1\"," +
                                                                               "   \"SomeValue1\"]," +
                                                                               "  [\"KeyValueStruct.csv\"," +
                                                                               $"  \"{StructGuid}\"," +
                                                                               "   \"SomeColumnName2\"," +
                                                                               "   \"SomeValue2\"]" +
                                                                               "]" +
                                                                               "}"), out IReadOnlyList<string> errors);
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
            var structInfo = _linkedParameterManager.GetStructWithGuid<IKeyValueStruct>(StructGuid);
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
            success = _linkedParameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                                     "[" +
                                                                     "  [\"SubInterfaceAInfo.csv\"," +
                                                                     $"  \"{Item1SubclassAId}\"," +
                                                                     "   \"SomeColumnName\"," +
                                                                     "   \"SomeValue\"]," +
                                                                     "  [\"SubInterfaceBInfo.csv\"," +
                                                                     $"  \"{Item2SubclassBId}\"," +
                                                                     "   \"SomeColumnName2\"," +
                                                                     "   \"SomeValue2\"]" +
                                                                     "]" +
                                                                     "}"), out errors);
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
            var success = _linkedParameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                                               "[" +
                                                                               $"  [\"SubInterfaceAInfo.csv\"," +
                                                                               $"  \"{Item1SubclassAId}\"," +
                                                                               "   \"SomeColumnName1\"," +
                                                                               "   \"SomeValue1\"]" +
                                                                               "]" +
                                                                               "}"), out IReadOnlyList<string> errors);
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
            LogAssert.Expect(LogType.Error, $"Missing: Cannot find parameter by GUID {Item1SubclassAId} for type IMySpecialInfo");
            var success = _linkedParameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                                     "[" +
                                                                     "  [\"MySpecialInfo.csv\"," +
                                                                     $"  \"{Item1SubclassAId}\"," +
                                                                     "   \"SomeColumnName\"," +
                                                                     "   \"SomeValue\"]" +
                                                                     "]" +
                                                                     "}"), out var errors);
            Assert.That(success, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo($"Cannot find parameter for csv [MySpecialInfo.csv] and identifier/guid [{Item1SubclassAId}]."));
        }

        [Test]
        public void ApplyOverrides_CachedParameterError()
        {
            // get info to pre-cache
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);
            var mockedItem1Info = (MockSubclassAInfo)item1Info;

            // set an error to be returned by the info
            mockedItem1Info.ReturnEditPropertyError = "some error";

            // apply to one row
            var success = _linkedParameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                                               "[" +
                                                                               $"  [\"SubInterfaceAInfo.csv\"," +
                                                                               $"  \"{Item1SubclassAId}\"," +
                                                                               "   \"SomeColumnName\"," +
                                                                               "   \"SomeValue\"]" +
                                                                               "]" +
                                                                               "}"), out IReadOnlyList<string> errors);
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
            var success = _linkedParameterManager.ApplyOverrides(JObject.Parse("{\"edit\":" +
                                                                               "[" +
                                                                               $"  [\"SubInterfaceAInfo.csv\"," +
                                                                               $"  \"{Item1SubclassAId}\"," +
                                                                               "   \"SomeColumnName\"," +
                                                                               "   \"SomeValue\"]" +
                                                                               "]" +
                                                                               "}"), out IReadOnlyList<string> errors);
            Assert.That(success, Is.True);
            Assert.That(errors, Is.Null);

            // upon getting, the override will be applied and the error will surface
            LogAssert.Expect(LogType.Error, $"Error editing (MockErrorSubclassAInfo) property [SomeColumnName] with value [SomeValue]: {originalItem1Info.ReturnEditPropertyError}");
            var item1Info = _linkedParameterManager.Get<ISubInterfaceAInfo>(Item1SubclassAId);

            // sanity check that a new info was lazily created
            Assert.That(item1Info, Is.Not.EqualTo(originalItem1Info));
            Assert.That(item1Info.Identifier, Is.EqualTo(originalItem1Info.Identifier));
        }
    }
}
