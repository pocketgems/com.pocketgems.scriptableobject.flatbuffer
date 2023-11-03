using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.LocalCSV
{
    public class CSVUtilTest
    {
        [Test]
        public void CSVToInterfaceFileName()
        {
            var path = Path.Combine(new[] { "Assets", "Parameters", "LocalCSV", "CurrencyInfo.csv" });
            Assert.AreEqual("ICurrencyInfo", CSVUtil.CSVToInterfaceFileName(path));
            Assert.AreEqual("ICurrencyInfo", CSVUtil.CSVToInterfaceFileName("CurrencyInfo.csv"));
        }
    }
}
