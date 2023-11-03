using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.LocalCSV;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Operations.Data
{
    public class BaseDataOperationTest : BaseOperationTest<IDataOperationContext>
    {
        private const string kTestAssemblyName = "TestGeneratedCodeEditorAssembly";
        protected const string kTestCSVDir = "TestCSVDir";

        protected IInfoCSVFileCache _infoCSVFileCacheMock;
        protected IStructCSVFileCache _structCSVFileCacheMock;

        protected Dictionary<string, CSVFile> _loadedInfoCSVFileCacheFiles;
        protected Dictionary<string, CSVFile> _loadedStructCSVFileCacheFiles;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _infoCSVFileCacheMock = Substitute.For<IInfoCSVFileCache>();
            _loadedInfoCSVFileCacheFiles = new Dictionary<string, CSVFile>();
            _infoCSVFileCacheMock.LoadedFiles().ReturnsForAnyArgs(_loadedInfoCSVFileCacheFiles);

            _structCSVFileCacheMock = Substitute.For<IStructCSVFileCache>();
            _loadedStructCSVFileCacheFiles = new Dictionary<string, CSVFile>();
            _structCSVFileCacheMock.LoadedFiles().ReturnsForAnyArgs(_loadedStructCSVFileCacheFiles);

            _contextMock.InfoCSVFileCache.ReturnsForAnyArgs(_infoCSVFileCacheMock);
            _contextMock.StructCSVFileCache.ReturnsForAnyArgs(_structCSVFileCacheMock);

            _contextMock.GeneratedCodeEditorAssemblyName.ReturnsForAnyArgs(kTestAssemblyName);
            _contextMock.ParameterInfos.ReturnsForAnyArgs(_mockParameterInfos);
            _contextMock.ParameterStructs.ReturnsForAnyArgs(_mockParameterStructs);
        }

        protected CSVFile CreateCSV(IParameterInterface parameterInterface)
        {
            if (!Directory.Exists(kTestCSVDir))
                Directory.CreateDirectory(kTestCSVDir);
            var path = Path.Combine(kTestCSVDir, $"{parameterInterface.BaseName}.csv");
            var csvFile = new CSVFile(path, false, true);
            csvFile.DefineSchema(new[] { "Identifier" }, new[] { "string" });
            csvFile.GetOrCreateRow("some_guid").UpdateData(new[] { "some id" });
            Assert.IsTrue(csvFile.Write());
            return csvFile;
        }
    }
}
