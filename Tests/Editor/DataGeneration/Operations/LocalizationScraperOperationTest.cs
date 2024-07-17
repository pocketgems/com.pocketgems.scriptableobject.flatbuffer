using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    public class LocalizationScraperOperationTest : BaseOperationTest<IDataOperationContext>
    {
        // private LocalizationScraperOperation _operation;
        // // private List<IParameterFile> _parameterFileContainer;
        // private List<string> _localizationKey;
        //
        // [SetUp]
        // public override void SetUp()
        // {
        //     base.SetUp();
        //     _localizationKey = new List<string>();
        //     _parameterFileContainer = new List<IParameterFile>();
        //     _operation = new LocalizationScraperOperation(_localizationKey);
        // }
        //
        // [Test]
        // public void NoLocalizedStrings()
        // {
        //     // no parameter files = no localization keys
        //     AssertExecute(_operation, _parameterFileContainer, false, false);
        //     Assert.AreEqual(0, _localizationKey.Count);
        //
        //     // no parameter files with localization data type = no localization keys
        //     var csvName = "test";
        //     var columnNames = new[] { "identifier", "hp" };
        //     var columnTypes = new[] { "String", "Integer" };
        //     var rowDatas = new List<string[]> { new[] { "id1", "10" }, new[] { "id2", "20" } };
        //     var parameterFile = MockedParameterFile.ParameterFile(false, csvName, columnNames, columnTypes, rowDatas);
        //     _parameterFileContainer.Add(parameterFile);
        //
        //     // execute operation
        //     AssertExecute(_operation, _parameterFileContainer, false, false);
        //
        //     // 0 outcome
        //     Assert.AreEqual(0, _localizationKey.Count);
        // }
        //
        // [Test]
        // public void GetLocalizedStrings()
        // {
        //     var localizedStringDataType = new LocalizedStringDataType();
        //     var arrayLocalizedStringDataType = new ArrayLocalizedStringDataType();
        //
        //     // 4 unique strings
        //     const string locKey1 = "tom";
        //     const string locKey2 = "tim";
        //     const string locKey3 = "troy";
        //     const string locKey4 = "tina";
        //
        //     // localization column types
        //     var csvName = "test";
        //     var columnNames = new[] { "identifier", "name", "names" };
        //     var columnTypes = new[] { "String", localizedStringDataType.ColumnType, arrayLocalizedStringDataType.ColumnType };
        //     var rowDats = new List<string[]>();
        //     rowDats.Add(new[] { "id1", locKey1, $"{locKey3}|{locKey4}" }); // 3 unique strings
        //     rowDats.Add(new[] { "id2", locKey2, "" }); // 1 unique string
        //     rowDats.Add(new[] { "id3", " ", $"{locKey1}|{locKey2}|" }); // empty strings & previously seen strings
        //     var parameterFile = MockedParameterFile.ParameterFile(false, csvName, columnNames, columnTypes, rowDats);
        //     _parameterFileContainer.Add(parameterFile);
        //
        //     // execute operation
        //     AssertExecute(_operation, _parameterFileContainer, false, false);
        //
        //     // 4 unique string results
        //     Assert.AreEqual(4, _localizationKey.Count);
        //     Assert.Contains(locKey1, _localizationKey);
        //     Assert.Contains(locKey2, _localizationKey);
        //     Assert.Contains(locKey3, _localizationKey);
        //     Assert.Contains(locKey4, _localizationKey);
        // }
    }
}
