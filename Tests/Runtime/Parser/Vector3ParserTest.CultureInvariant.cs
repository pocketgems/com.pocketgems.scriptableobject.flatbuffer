using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace PocketGems.Parameters.Parser
{
    public partial class Vector3ParserTest
    {
        public class CultureInvariant : Vector3ParserTest
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
