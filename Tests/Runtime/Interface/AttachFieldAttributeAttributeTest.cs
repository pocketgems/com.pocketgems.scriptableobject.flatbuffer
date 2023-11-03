using NUnit.Framework;

namespace PocketGems.Parameters.Interface
{
    public class AttachFieldAttributeAttributeTest
    {
        [Test]
        public void Test()
        {
            var a = new AttachFieldAttributeAttribute("test");
            Assert.AreEqual("test", a.AttributeText);
        }
    }
}
