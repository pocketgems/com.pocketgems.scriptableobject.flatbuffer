using System;
using System.Security.Cryptography;
using System.Text;

namespace PocketGems.Parameters.Util
{
    /// <summary>
    /// Generates deterministic 32 character lowercase hex guids (Unity guid format) from strings.
    ///
    /// The same input always produces the same guid across processes and platforms, so guids generated
    /// at build time and at runtime (client or server) can be matched against each other.
    ///
    /// The MD5 instance and hex converter are created lazily on the first ToGuid call, so bulk
    /// conversion should reuse one converter for all inputs. Dispose to release the MD5.
    ///
    /// ToGuid is thread safe: parameters can be read from background threads (see
    /// ParameterStructReference), and MD5 instances are stateful, so hashing is serialized.
    /// </summary>
    public class GuidConverter : IDisposable
    {
        /// <summary>
        /// Hashes a string into a 32 character lowercase hex guid.
        /// Null or empty input is returned unchanged so unassigned values stay unassigned.
        /// </summary>
        /// <param name="input">the string to hash</param>
        /// <returns>the hashed guid, or the input when null/empty</returns>
        public string ToGuid(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            lock (_lock)
            {
                _md5 ??= MD5.Create();
                _hexConverter ??= new HexStringConverter();
                byte[] hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return _hexConverter.ToHexString(hash);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _md5?.Dispose();
                _md5 = null;
            }
        }

        private readonly object _lock = new object();
        private MD5 _md5;
        private HexStringConverter _hexConverter;
    }
}
