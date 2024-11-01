using System;
using System.Collections.Generic;
using MyNameSpace;
using NUnit.Framework;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public class ParameterEnumTest
    {
        [Test]
        [TestCase(typeof(ValidEnum))]
        [TestCase(typeof(ValidFlagEnum))]
        public void ValidEnum(Type enumType)
        {
            IParameterEnum parameterEnum = new ParameterEnum(enumType);
            Assert.AreEqual(enumType, parameterEnum.Type);
            Assert.IsTrue(parameterEnum.Validate(out List<string> errors));
            Assert.IsEmpty(errors);
        }

        [Test]
        [TestCase(typeof(InvalidInternalEnum))]
        [TestCase(typeof(InvalidNamespaceEnum))]
        public void InvalidEnum(Type enumType)
        {
            IParameterEnum parameterEnum = new ParameterEnum(enumType);
            Assert.AreEqual(enumType, parameterEnum.Type);
            Assert.IsFalse(parameterEnum.Validate(out List<string> errors));
            Assert.AreEqual(1, errors.Count);
        }
    }
}
