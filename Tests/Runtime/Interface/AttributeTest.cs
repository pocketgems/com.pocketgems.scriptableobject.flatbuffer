using NUnit.Framework;

namespace PocketGems.Parameters.Interface.Attributes
{
    public class AttributeTest
    {
        private void AssertAttribute(IAttachScriptableObjectAttribute attribute, string expectedCode)
        {
            Assert.AreEqual(expectedCode, attribute.ScriptableObjectFieldAttributesCode);
        }

        [Test]
        public void AttachFieldAttributeAttribute()
        {
            var a = new AttachFieldAttributeAttribute("[Header(\"test\')");
            AssertAttribute(a, "[Header(\"test\')");
        }

        [Test]
        public void ParameterAttachFieldAttributeAttribute()
        {
            var a = new ParameterAttachFieldAttributeAttribute("[Header(\"test\')");
            AssertAttribute(a, "[Header(\"test\')");
        }

        [Test]
        public void ParameterFoldOutAttribute()
        {
            var a = new ParameterFoldOutAttribute("test", true);
            Assert.IsTrue(a.InitialFoldout);
            AssertAttribute(a, "[ParameterFoldOutAttribute(\"test\", true)]");

            a = new ParameterFoldOutAttribute("test", false);
            Assert.IsFalse(a.InitialFoldout);
            AssertAttribute(a, "[ParameterFoldOutAttribute(\"test\", false)]");
        }

        [Test]
        public void ParameterTooltipAttribute()
        {
            var a = new ParameterTooltipAttribute("test");
            AssertAttribute(a, "[Tooltip(\"test\")]");
        }

        [Test]
        public void ParameterHeaderAttribute()
        {
            var a = new ParameterHeaderAttribute("test");
            AssertAttribute(a, "[Header(\"test\")]");
        }

        [Test]
        public void ParameterTextAreaAttribute()
        {
            var a = new ParameterTextAreaAttribute();
            AssertAttribute(a, "[TextArea]");

            a = new ParameterTextAreaAttribute(1, 2);
            AssertAttribute(a, "[TextArea(1, 2)]");
        }
    }
}
