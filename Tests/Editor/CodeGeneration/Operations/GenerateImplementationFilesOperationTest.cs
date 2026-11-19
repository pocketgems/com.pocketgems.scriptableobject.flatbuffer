using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operation.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    public class GenerateImplementationFilesOperationTest : BaseCodeOperationTest
    {
        private const string TestScriptableObjectsDir = "TestScriptableObjectsDir";
        private const string TestGeneratedCodeDir = "TestGeneratedCodeDir";
        private static readonly string TestRuntimeInstancesDir = Path.Combine(TestGeneratedCodeDir, "RuntimeInstances");
        private const string TestStructsDir = "TestStructsDir";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            TearDown();

            _contextMock.GeneratedCodeScriptableObjectsDir.ReturnsForAnyArgs(TestScriptableObjectsDir);
            _contextMock.GeneratedCodeRuntimeInstancesDir.ReturnsForAnyArgs(TestRuntimeInstancesDir);
            _contextMock.GeneratedCodeStructsDir.ReturnsForAnyArgs(TestStructsDir);
        }

        private int FileCount(string dir)
        {
            if (!Directory.Exists(dir))
                return 0;
            return Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly).Length;
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestScriptableObjectsDir))
                Directory.Delete(TestScriptableObjectsDir, true);
            if (Directory.Exists(TestGeneratedCodeDir))
                Directory.Delete(TestGeneratedCodeDir, true);
            if (Directory.Exists(TestStructsDir))
                Directory.Delete(TestStructsDir, true);
        }

        [Test]
        public void Execute()
        {
            // initial creation
            var operation = new GenerateImplementationFilesOperation();
            AssertExecute(operation, OperationState.Finished);

            int infoCount = _mockParameterInfos.Count;
            int structsCount = _mockParameterStructs.Count;

            void AssertFileCounts(int scriptableObjects, int structs, int flatbuffers)
            {
                // +1 for the menu item file generated
                Assert.AreEqual(scriptableObjects + 1, FileCount(TestScriptableObjectsDir));
                Assert.AreEqual(structs, FileCount(TestStructsDir));
                Assert.AreEqual(flatbuffers, FileCount(TestRuntimeInstancesDir));
            }

            AssertFileCounts(infoCount, structsCount, infoCount + structsCount);

            // add excess files to emulate legacy files
            File.WriteAllText(Path.Combine(TestScriptableObjectsDir, "Test1.cs"), "blah");
            File.WriteAllText(Path.Combine(TestStructsDir, "Test2.cs"), "blah");
            File.WriteAllText(Path.Combine(TestRuntimeInstancesDir, "Test3.cs"), "blah");
            File.WriteAllText(Path.Combine(TestRuntimeInstancesDir, "Test4.cs"), "blah");

            AssertFileCounts(infoCount + 1, structsCount + 1, infoCount + structsCount + 2);

            // re-creation & remove excess old files
            operation = new GenerateImplementationFilesOperation();
            AssertExecute(operation, OperationState.Finished);

            AssertFileCounts(infoCount, structsCount, infoCount + structsCount);
        }

        [Test]
        public void ExecuteNoInputs()
        {
            // no interfaces
            _contextMock.ParameterInfos.ReturnsForAnyArgs(new List<IParameterInfo>());
            _contextMock.ParameterStructs.ReturnsForAnyArgs(new List<IParameterStruct>());

            var operation = new GenerateImplementationFilesOperation();
            AssertExecute(operation, OperationState.Finished);

            // only 1 file for the menu item file
            Assert.AreEqual(1, FileCount(TestScriptableObjectsDir));
            Assert.AreEqual(0, FileCount(TestStructsDir));
            Assert.AreEqual(0, FileCount(TestRuntimeInstancesDir));
        }

        [Test]
        public void ExecuteNoInputsWithCleanup()
        {
            Directory.CreateDirectory(TestScriptableObjectsDir);
            Directory.CreateDirectory(TestStructsDir);
            Directory.CreateDirectory(TestRuntimeInstancesDir);

            // add excess files to emulate legacy files
            File.WriteAllText(Path.Combine(TestScriptableObjectsDir, "Test1.cs"), "blah");
            File.WriteAllText(Path.Combine(TestStructsDir, "Test2.cs"), "blah");
            File.WriteAllText(Path.Combine(TestRuntimeInstancesDir, "Test3.cs"), "blah");
            File.WriteAllText(Path.Combine(TestRuntimeInstancesDir, "Test4.cs"), "blah");

            ExecuteNoInputs();
        }

        [Test]
        public void ExecuteRemovesOldClassesFolder()
        {
            var oldClassesDir = Path.Combine(TestGeneratedCodeDir, "FlatBufferClasses");
            Directory.CreateDirectory(oldClassesDir);
            File.WriteAllText(Path.Combine(oldClassesDir, "OldFlatBuffer.cs"), "blah");
            File.WriteAllText(oldClassesDir + ".meta", "blah");

            AssertExecute(new GenerateImplementationFilesOperation(), OperationState.Finished);

            Assert.IsFalse(Directory.Exists(oldClassesDir));
            Assert.IsFalse(File.Exists(oldClassesDir + ".meta"));
        }
    }
}
