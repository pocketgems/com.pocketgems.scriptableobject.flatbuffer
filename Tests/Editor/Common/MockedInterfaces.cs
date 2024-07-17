using System;
using System.Collections.Generic;
using System.Reflection;
using NSubstitute;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.PropertyTypes.Editor;

namespace PocketGems.Parameters.Editor
{
    public static class MockedInterfaces
    {
        private static int MockPropertyInfo { get; }

        public static IParameterInfo ParameterInfo(string baseName)
        {
            var mock = Substitute.For<IParameterInfo>();
            mock.InterfaceName.ReturnsForAnyArgs($"I{baseName}");
            mock.BaseName.ReturnsForAnyArgs(baseName);
            mock.GeneratedNameSpace.ReturnsForAnyArgs("MyNamespace");
            mock.ScriptableObjectClassName(true).Returns($"{baseName}ScriptableObject.cs");
            mock.ScriptableObjectClassName(false).Returns($"{baseName}ScriptableObject");
            mock.FlatBufferClassName(true).Returns($"{baseName}FBClass.cs");
            mock.FlatBufferClassName(false).Returns($"{baseName}FBClass");
            mock.FlatBufferStructName(true).Returns($"{baseName}FBStruct.cs");
            mock.FlatBufferStructName(false).Returns($"{baseName}FBStruct");
            mock.ValidatorClassName(true).Returns($"{baseName}Validator.cs");
            mock.ValidatorClassName(false).Returns($"{baseName}Validator");

            mock.Type.ReturnsForAnyArgs(typeof(IPassingInfo));
            List<Type> types = new List<Type> { typeof(ISuperInfo), typeof(IPassingInfo) };
            mock.OrderedBaseInterfaceTypes.ReturnsForAnyArgs(types);

            List<IPropertyType> propertyTypes = new List<IPropertyType>{
                PropertyType(), PropertyType()
            };
            mock.PropertyTypes.ReturnsForAnyArgs(propertyTypes);
            return mock;
        }

        public static IParameterStruct ParameterStruct(string baseName)
        {
            var mock = Substitute.For<IParameterStruct>();
            mock.InterfaceName.ReturnsForAnyArgs($"I{baseName}");
            mock.BaseName.ReturnsForAnyArgs(baseName);
            mock.GeneratedNameSpace.ReturnsForAnyArgs("MyNamespace");
            mock.StructName(true).Returns($"{baseName}.cs");
            mock.StructName(false).Returns($"{baseName}");
            mock.FlatBufferClassName(true).Returns($"{baseName}FBClass.cs");
            mock.FlatBufferClassName(false).Returns($"{baseName}FBClass");
            mock.FlatBufferStructName(true).Returns($"{baseName}FBStruct.cs");
            mock.FlatBufferStructName(false).Returns($"{baseName}FBStruct");
            mock.ValidatorClassName(true).Returns($"{baseName}Validator.cs");
            mock.ValidatorClassName(false).Returns($"{baseName}Validator");

            mock.Type.ReturnsForAnyArgs(typeof(IPassingStruct));
            List<Type> types = new List<Type> { typeof(ISuperStruct), typeof(IPassingStruct) };
            mock.OrderedBaseInterfaceTypes.ReturnsForAnyArgs(types);

            List<IPropertyType> propertyTypes = new List<IPropertyType> { PropertyType(), PropertyType() };
            mock.PropertyTypes.ReturnsForAnyArgs(propertyTypes);
            return mock;
        }

        public static IPropertyType PropertyType()
        {
            var mock = Substitute.For<IPropertyType>();
            var propertyInfo = typeof(MockedInterfaces).GetProperty(nameof(MockPropertyInfo), BindingFlags.Static | BindingFlags.NonPublic);
            mock.PropertyInfo.ReturnsForAnyArgs(propertyInfo);
            mock.FlatBufferBuilderPrepareCode(default).ReturnsForAnyArgs("");
            mock.FlatBufferBuilderCode(default).ReturnsForAnyArgs("");
            mock.FlatBufferFieldDefinitionCode().ReturnsForAnyArgs("");
            mock.FlatBufferPropertyImplementationCode().ReturnsForAnyArgs("");
            mock.FlatBufferEditPropertyCode("value").ReturnsForAnyArgs("");
            mock.FlatBufferRemoveEditCode().ReturnsForAnyArgs("");
            mock.ScriptableObjectPropertyImplementationCode().ReturnsForAnyArgs("");
            return mock;
        }
    }
}
