using System;
using System.Collections.Generic;
using MyNameSpace;
using NUnit.Framework;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public class ParameterStructTest : ParameterInterfaceTest
    {
        [Test]
        public void ValidInterface()
        {
            Type type = typeof(IPassingStruct);
            string baseName = "PassingStruct";
            var parameterStruct = new ParameterStruct(type);
            AssertValidInterface(parameterStruct, baseName, type, false, typeof(IBaseStruct), typeof(ISuperStruct));

            Assert.AreEqual($"{baseName}", parameterStruct.StructName(false));
            Assert.AreEqual($"{baseName}.cs", parameterStruct.StructName(true));
        }

        [Test]
        [TestCase(typeof(IInternalStruct))] // interface is internal
        [TestCase(typeof(INamedBadly))] // interface doesn't follow naming conventions
        [TestCase(typeof(INonBaseStruct))] // interface isn't sub interface of IBaseStruct
        [TestCase(typeof(IDeriveFromBadInterfaceStruct))] // interface derives from external interface
        [TestCase(typeof(IBadParameterNameStruct))] // parameter in interface doesn't follow naming conventions
        [TestCase(typeof(IDuplicatePropertyStruct))] // duplicate property in sub interface
        [TestCase(typeof(INonSupportedTypeStruct))] // non supported type defined
        [TestCase(typeof(ISetterStruct))] // property has a setter
        [TestCase(typeof(IInternalPropertyStruct))] // property is defined as non public
        [TestCase(typeof(IMethodStruct))] // interface defined a method
        [TestCase(typeof(INameSpacedStruct))] // interface has a name space
        [TestCase(typeof(IReferenceBaseStruct))] // interface has reference to base struct
        [TestCase(typeof(IListReferenceBaseStruct))] // interface has reference to base struct
        public void InvalidInterface(Type interfaceType)
        {
            AssertInvalidInterface(new ParameterStruct(interfaceType));
        }

        [Test]
        [TestCase(typeof(ICircularAStruct))] // references B
        [TestCase(typeof(ICircularBStruct))] // references A
        [TestCase(typeof(ICircularCStruct))] // subclass of B
        [TestCase(typeof(ICircularDStruct))] // references D (self)

        [TestCase(typeof(ICircularEStruct))] // reference F
        [TestCase(typeof(ICircularFStruct))] // reference G
        [TestCase(typeof(ICircularGStruct))] // reference E
        public void InvalidWithCircularReferences(Type interfaceType)
        {
            AssertInvalidInterface(new ParameterStruct(interfaceType));
        }

        [Test]
        [TestCase(typeof(IReferenceStructStruct))]
        [TestCase(typeof(ICircularIStruct))] // references J with list
        [TestCase(typeof(ICircularJStruct))] // references I with list
        [TestCase(typeof(ICircularKStruct))] // references K (self) with list
        public void ValidListReferences(Type interfaceType)
        {
            var parameterStruct = new ParameterStruct(interfaceType);
            Assert.IsTrue(parameterStruct.Validate(out IReadOnlyList<string> errors));
            Assert.IsEmpty(errors);
        }
    }
}
