using System;
using System.Collections.Generic;
using MyNameSpace;
using NUnit.Framework;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Models
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
        public void ValidWithNoCircularReferences()
        {
            var parameterStruct = new ParameterStruct(typeof(IReferenceStructStruct));
            Assert.IsTrue(parameterStruct.Validate(out IReadOnlyList<string> errors));
            Assert.IsEmpty(errors);
        }

        [Test]
        [TestCase(typeof(ICircularAStruct))]
        [TestCase(typeof(ICircularBStruct))]
        [TestCase(typeof(ICircularCStruct))]
        [TestCase(typeof(ICircularDStruct))]
        [TestCase(typeof(ICircularEStruct))]
        [TestCase(typeof(ICircularFStruct))]
        [TestCase(typeof(ICircularGStruct))]
        [TestCase(typeof(ICircularHStruct))]
        [TestCase(typeof(ICircularIStruct))]
        [TestCase(typeof(ICircularJStruct))]
        [TestCase(typeof(ICircularKStruct))]
        [TestCase(typeof(ICircularAStruct))]
        public void InvalidWithCircularReferences(Type interfaceType)
        {
            AssertInvalidInterface(new ParameterStruct(interfaceType));
        }
    }
}
