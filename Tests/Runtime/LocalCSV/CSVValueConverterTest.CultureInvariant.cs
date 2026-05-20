using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace PocketGems.Parameters.LocalCSV
{
    public partial class CSVValueConverterTest
    {
        public class CultureInvariant : CSVValueConverterTest
        {
            private CultureInfo _originalCulture;

            [SetUp]
            public void SetUp()
            {
                _originalCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            }

            [TearDown]
            public void TearDown()
            {
                Thread.CurrentThread.CurrentCulture = _originalCulture;
            }
        }
    }
}
