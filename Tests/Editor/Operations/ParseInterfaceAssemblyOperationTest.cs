using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Operations
{
    public class ParseInterfaceAssemblyOperationTest : BaseOperationTest<IDataOperationContext>
    {
        private ParseInterfaceAssemblyOperation _operation;

        private static string InterfaceDirPath =>
            Path.Combine(new[] { "Packages", ParameterConstants.PackageName, "Tests", "Editor", "TestAssemblies", "TestInterfaceAssembly" });
        private static string InterfaceAssemblyName => "TestInterfaceAssembly";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _operation = new ParseInterfaceAssemblyOperation();
        }

        [Test]
        public void Success()
        {
            List<IParameterInfo> parameterInfos = new List<IParameterInfo>();
            List<IParameterStruct> parameterStructs = new List<IParameterStruct>();
            List<IParameterEnum> parameterEnums = new List<IParameterEnum>();
            _contextMock.ParameterInfos.ReturnsForAnyArgs(parameterInfos);
            _contextMock.ParameterStructs.ReturnsForAnyArgs(parameterStructs);
            _contextMock.ParameterEnums.ReturnsForAnyArgs(parameterEnums);
            _contextMock.InterfaceDirectoryRootPath.ReturnsForAnyArgs(InterfaceDirPath);
            _contextMock.InterfaceAssemblyName.ReturnsForAnyArgs(InterfaceAssemblyName);
            _contextMock.InterfaceAssemblyHash = null;

            Assert.IsEmpty(parameterInfos);
            Assert.IsEmpty(parameterStructs);
            Assert.IsEmpty(parameterEnums);
            Assert.IsNull(_contextMock.InterfaceAssemblyHash);

            AssertExecute(_operation, OperationState.Finished);

            Assert.AreEqual(1, parameterInfos.Count);
            Assert.AreEqual(3, parameterStructs.Count);
            Assert.AreEqual(2, parameterEnums.Count);
            Assert.IsNotNull(_contextMock.InterfaceAssemblyHash);
            Assert.AreEqual(128 / 4, _contextMock.InterfaceAssemblyHash.Length); // MD5 hex
        }

        [Test]
        public void MissingAssembly()
        {
            _contextMock.InterfaceDirectoryRootPath.ReturnsForAnyArgs(InterfaceDirPath);
            _contextMock.InterfaceAssemblyName.ReturnsForAnyArgs("BadInterfaceAssemblyName");
            AssertExecute(_operation, OperationState.Error);
        }

        [Test]
        public void NoInterfaceFolder()
        {
            string badFolderPath = "blah";
            _contextMock.InterfaceDirectoryRootPath.ReturnsForAnyArgs(badFolderPath);
            AssertExecute(_operation, OperationState.ShortCircuit);
        }

        [Test]
        public void NoInterfaceFolder_CodeOperationPath()
        {
            var contextMock = Substitute.For<ICodeOperationContext>();
            string badFolderPath = "blah";
            contextMock.InterfaceDirectoryRootPath.ReturnsForAnyArgs(badFolderPath);
            _operation.Execute(contextMock);
            Assert.AreEqual(OperationState.ShortCircuit, _operation.OperationState);
        }
    }
}
