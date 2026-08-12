namespace PocketGems.Parameters.Util
{
    /// <summary>
    /// Converts byte arrays into lowercase hex strings.
    ///
    /// The lookup table built at construction is faster than calling ToString("x2") per byte, so bulk
    /// conversion should reuse one converter.
    /// </summary>
    public class HexStringConverter
    {
        public HexStringConverter()
        {
            _hexLookUp = new char[256 * 2];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                _hexLookUp[2 * i] = s[0];
                _hexLookUp[2 * i + 1] = s[1];
            }
        }

        public string ToHexString(byte[] byteArray)
        {
            var result = new char[byteArray.Length * 2];
            for (int i = 0; i < byteArray.Length; i++)
            {
                result[2 * i] = _hexLookUp[2 * byteArray[i]];
                result[2 * i + 1] = _hexLookUp[2 * byteArray[i] + 1];
            }
            return new string(result);
        }

        private readonly char[] _hexLookUp;
    }
}
