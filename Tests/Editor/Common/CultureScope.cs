using System;
using System.Globalization;
using NUnit.Framework;

namespace PocketGems.Parameters.Editor
{
    /// <summary>
    /// Runs a block under the Turkish culture, where "I".ToLower() is "ı" and "i".ToUpper() is "İ", to catch
    /// culture-sensitive casing in generated output. Ignores the test if the runtime lacks Turkish casing rules.
    /// </summary>
    public sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previous;

        private CultureScope(CultureInfo culture)
        {
            _previous = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = culture;
        }

        public static CultureScope Turkish()
        {
            CultureInfo turkish = new("tr-TR");
            if ("I".ToLower(turkish) != "ı")
                Assert.Ignore("Turkish casing rules are unavailable in this runtime.");
            return new CultureScope(turkish);
        }

        public void Dispose() => CultureInfo.CurrentCulture = _previous;
    }
}
