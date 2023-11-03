using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.AssetLoader;
using PocketGems.Parameters.Validation;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters
{
    public class EditorParamsTest
    {
        [SetUp]
        public void SetUp()
        {
            EditorParams.Destroy();
        }

        [TearDown]
        public void TearDown()
        {
            EditorParams.Destroy();
            Assert.IsNull(EditorParams.InternalParameterManager);
            Assert.IsNull(EditorParams.HotLoader);
        }

        [Test]
        public void Init()
        {
            Assert.IsNull(EditorParams.InternalParameterManager);
            Assert.IsNull(EditorParams.HotLoader);

            var pm = Substitute.For<IMutableParameterManager>();
            var hl = Substitute.For<IParameterHotLoader>();
            hl.Status.ReturnsForAnyArgs(ParameterAssetLoaderStatus.Loaded);
            var dl = Substitute.For<IParameterDataLoader>();

            // test init
            EditorParams.Init(pm, hl, dl);
            Assert.AreEqual(pm, EditorParams.ParameterManager);
            Assert.AreEqual(hl, EditorParams.HotLoader);
            hl.Received(1).LoadData(pm, dl);

            // init error
            hl.Status.ReturnsForAnyArgs(ParameterAssetLoaderStatus.Failed);
            LogAssert.Expect(LogType.Error, new Regex("Unable to initialize .*"));
            EditorParams.Init(pm, hl, dl);
            Assert.IsNull(EditorParams.InternalParameterManager);
            Assert.IsNull(EditorParams.HotLoader);
            hl.Received(2).LoadData(pm, dl);
        }

        [Test]
        public void FindSingleInterfaceImplementation()
        {
            // found none
            var type = EditorParams.FindSingleInterfaceImplementation(typeof(INonImplementedInterface));
            Assert.IsNull(type);

            // found one
            type = EditorParams.FindSingleInterfaceImplementation(typeof(ISomeInterfaceForTesting));
            Assert.AreEqual(typeof(SomeInterfaceForTesting), type);

            // found many
            LogAssert.Expect(LogType.Error, new Regex("Found more than one .*"));
            type = EditorParams.FindSingleInterfaceImplementation(typeof(ISomeInterfaceForTesting2));
            Assert.IsTrue(type == typeof(SomeInterfaceForTesting2_1) ||
                          type == typeof(SomeInterfaceForTesting2_2));
        }
    }
}
