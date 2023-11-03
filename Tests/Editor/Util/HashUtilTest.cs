using NUnit.Framework;

namespace PocketGems.Parameters.Util
{
    public class HashUtilTest
    {
        [Test]
        public void MD5Test()
        {
            Assert.IsNull(HashUtil.MD5Hash(null));
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", HashUtil.MD5Hash(""));
            Assert.AreEqual("900150983cd24fb0d6963f7d28e17f72", HashUtil.MD5Hash("abc"));
            Assert.AreEqual("902fbdd2b1df0c4f70b4a5d23525e932", HashUtil.MD5Hash("ABC"));
            Assert.AreEqual("c3fcd3d76192e4007dfb496cca67e13b", HashUtil.MD5Hash("abcdefghijklmnopqrstuvwxyz"));
            Assert.AreEqual("cb20bf9177e73d5ffa71e95d22389d6d", HashUtil.MD5Hash("abcdefghijklmnopqrstuvwxyz "));
        }
    }
}
