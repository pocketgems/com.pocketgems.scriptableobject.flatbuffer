using System;
using MyNameSpace;
using NUnit.Framework;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Models
{
    public class ParameterInfoTest : ParameterInterfaceTest
    {
        [Test]
        public void ValidInterface()
        {
            Type type = typeof(IPassingInfo);
            string baseName = "PassingInfo";
            var parameterInfo = new ParameterInfo(type);
            AssertValidInterface(parameterInfo, baseName, type, true, typeof(IBaseInfo), typeof(ISuperInfo));

            Assert.AreEqual($"{baseName}ScriptableObject", parameterInfo.ScriptableObjectClassName(false));
            Assert.AreEqual($"{baseName}ScriptableObject.cs", parameterInfo.ScriptableObjectClassName(true));
        }

        [Test]
        [TestCase(typeof(IInternalInfo))] // interface is internal
        [TestCase(typeof(IBadlyNamed))] // interface doesn't follow naming conventions
        [TestCase(typeof(INonBaseInfo))] // interface isn't sub interface of IBaseInfo
        [TestCase(typeof(IDeriveFromBadInterfaceInfo))] // interface derives from external interface
        [TestCase(typeof(IBadParameterNameInfo))] // parameter in interface doesn't follow naming conventions
        [TestCase(typeof(IDuplicatePropertyInfo))] // duplicate property in sub interface
        [TestCase(typeof(IKeywordPropertyInfo))] // using reserved keyword name
        [TestCase(typeof(INonSupportedTypeInfo))] // non supported type defined
        [TestCase(typeof(ISetterInfo))] // property has a setter
        [TestCase(typeof(IInternalPropertyInfo))] // property is defined as non public
        [TestCase(typeof(IMethodInfo))] // interface defined a method
        [TestCase(typeof(INameSpacedInfo))] // interface has a name space
        [TestCase(typeof(IReferenceBaseInfo))] // interface has reference to base info
        [TestCase(typeof(IListReferenceBaseInfo))] // interface has reference to base info
        public void InvalidInterface(Type interfaceType)
        {
            AssertInvalidInterface(new ParameterInfo(interfaceType));
        }
    }
}
