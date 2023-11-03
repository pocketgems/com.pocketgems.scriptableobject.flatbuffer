using System;
using System.Collections.Generic;
using NUnit.Framework;
using PocketGems.Parameters.PropertyTypes;

namespace PocketGems.Parameters.Models
{
    public abstract class ParameterInterfaceTest
    {
        protected void AssertValidInterface(IParameterInterface parameterInterface,
            string baseName, Type interfaceType, bool expectIdentifierPropertyType,
            Type expectedBaseInterface0, Type expectedBaseInterface1)
        {
            Assert.AreEqual(interfaceType, parameterInterface.Type);
            Assert.IsNotNull(parameterInterface.ToString());
            Assert.AreEqual("Parameters", parameterInterface.GeneratedNameSpace);
            Assert.AreEqual(interfaceType.Name, parameterInterface.InterfaceName);
            Assert.AreEqual(baseName, parameterInterface.BaseName);
            Assert.AreEqual($"{baseName}FlatBuffer", parameterInterface.FlatBufferClassName(false));
            Assert.AreEqual($"{baseName}FlatBufferStruct", parameterInterface.FlatBufferStructName(false));
            Assert.AreEqual($"{baseName}FlatBuffer.cs", parameterInterface.FlatBufferClassName(true));
            Assert.AreEqual($"{baseName}FlatBufferStruct.cs", parameterInterface.FlatBufferStructName(true));
            Assert.AreEqual($"{baseName}Validator.cs", parameterInterface.ValidatorClassName(true));

            Assert.AreEqual(2, parameterInterface.OrderedBaseInterfaceTypes.Count);
            Assert.AreEqual(expectedBaseInterface0, parameterInterface.OrderedBaseInterfaceTypes[0]);
            Assert.AreEqual(expectedBaseInterface1, parameterInterface.OrderedBaseInterfaceTypes[1]);

            Assert.AreEqual(4, parameterInterface.PropertyTypes.Count);
            if (expectIdentifierPropertyType)
            {
                // Identifier - IBaseInfo
                Assert.AreEqual(typeof(IdentifierPropertyType), parameterInterface.PropertyTypes[0].GetType());
            }
            else
            {
                // Identifier - ISuperStruct
                Assert.AreEqual(typeof(StringPropertyType), parameterInterface.PropertyTypes[0].GetType());
            }
            // MyString - ISuperInfo/ISuperStruct
            Assert.AreEqual(typeof(StringPropertyType), parameterInterface.PropertyTypes[1].GetType());
            // MyFloat - ISuperInfo/ISuperStruct
            Assert.AreEqual(typeof(StandardPropertyType), parameterInterface.PropertyTypes[2].GetType());
            // MyInt - IPassingInfo/IPassingStruct
            Assert.AreEqual(typeof(StandardPropertyType), parameterInterface.PropertyTypes[3].GetType());

            // no errors
            Assert.IsTrue(parameterInterface.Validate(out IReadOnlyList<string> errors));
            Assert.IsEmpty(errors);
        }

        protected void AssertInvalidInterface(IParameterInterface parameterInterface)
        {
            Assert.IsFalse(parameterInterface.Validate(out IReadOnlyList<string> errors));
            Assert.AreEqual(1, errors.Count);
        }
    }
}
