using System;
using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Editor;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Types;
using UnityEngine;
#if ADDRESSABLE_PARAMS
using UnityEngine.AddressableAssets;
#endif

namespace PocketGems.Parameters.Common.PropertyTypes.Editor
{
    public class PropertyTypesTest
    {
        private enum MyEnum
        {
            Value1,
            Value2
        }

        private interface ITestStruct : IBaseStruct
        {
            string Identifier { get; }
        }

        private interface ITestInfo : IBaseInfo
        {
            // unsupported
            IEnumerable UnsupportedType { get; }

            // supported
            bool MyBool { get; }
            short MyShort { get; }
            int MyInt { get; }
            long MyLong { get; }
            float MyFloat { get; }
            ushort MyUShort { get; }
            uint MyUInt { get; }
            ulong MyULong { get; }
            MyEnum MyEnum { get; }

            // supported but has additional flat buffer data prep
            IReadOnlyList<bool> MyBools { get; }
            IReadOnlyList<short> MyShorts { get; }
            IReadOnlyList<int> MyInts { get; }
            IReadOnlyList<long> MyLongs { get; }
            IReadOnlyList<float> MyFloats { get; }
            IReadOnlyList<ushort> MyUShorts { get; }
            IReadOnlyList<uint> MyUInts { get; }
            IReadOnlyList<ulong> MyULongs { get; }
            IReadOnlyList<MyEnum> MyEnums { get; }

            // unity types
            Color MyColor { get; }
            Vector2 MyVector2 { get; }
            Vector2Int MyVector2Int { get; }
            Vector3 MyVector3 { get; }
            Vector3Int MyVector3Int { get; }
            IReadOnlyList<Color> MyColors { get; }
            IReadOnlyList<Vector2> MyVector2s { get; }
            IReadOnlyList<Vector2Int> MyVector2Ints { get; }
            IReadOnlyList<Vector3> MyVector3s { get; }
            IReadOnlyList<Vector3Int> MyVector3Ints { get; }

            // date & time
            DateTime MyDateTime { get; }
            IReadOnlyList<DateTime> MyDateTimes { get; }
            TimeSpan MyTimeSpan { get; }
            IReadOnlyList<TimeSpan> MyTimeSpans { get; }

            string MyString { get; }
            IReadOnlyList<string> MyStrings { get; }

            LocalizedString MyLocString { get; }
            IReadOnlyList<LocalizedString> MyLocStrings { get; }

            ParameterReference<ITestInfo> MyTestInfo { get; }
            IReadOnlyList<ParameterReference<ITestInfo>> MyTestInfos { get; }

            ParameterStructReference<ITestStruct> MyStruct { get; }
            IReadOnlyList<ParameterStructReference<ITestStruct>> MyStructs { get; }

#if ADDRESSABLE_PARAMS
            AssetReference MyAsset { get; }
            AssetReferenceSprite MyAssetSprite { get; }
            AssetReferenceAtlasedSprite MyAssetAltasdSprite { get; }
            IReadOnlyList<AssetReference> MyAssets { get; }
            IReadOnlyList<AssetReferenceSprite> MyAssetSprites { get; }
            IReadOnlyList<AssetReferenceAtlasedSprite> MyAssetAltasdSprites { get; }
#endif

            [AttachFieldAttribute("MyAttribute(\"blah\")")]
            int IntWithAttribute { get; }

            [AttachFieldAttribute("MyAttribute(\"blah1\")")]
            [AttachFieldAttribute("MyAttribute(\"blah2\")")]
            int IntWithAttributes { get; }
        }

        private const string TableName = "table_name";
        private SchemaBuilder _schemaBuilder;

        [SetUp]
        public void SetUp()
        {
            _schemaBuilder = new SchemaBuilder("test");
        }

        private IPropertyType CreatePropertyType(string propertyName, Type type = null)
        {
            if (type == null)
                type = typeof(ITestInfo);

            var parameterInterfaceMock = MockedInterfaces.ParameterInfo("TestInfo");
            parameterInterfaceMock.Type.ReturnsForAnyArgs(type);

            var propertyInfo = type.GetProperty(propertyName);
            Assert.IsNotNull(propertyInfo);
            var propertyType = PropertyTypeFactory.Create(parameterInterfaceMock, propertyInfo, out string error);
            Assert.IsNull(error);
            return propertyType;
        }

        private void AssertPropertyType(IPropertyType propertyType,
            bool expectNullPrepareSource = false,
            bool expectImmutableFlatBufferData = false,
            bool isBaseInfoIdentifier = false)
        {
            Assert.IsNotNull(propertyType.PropertyInfo);
            Assert.IsNull(propertyType.ScriptableObjectFieldAttributesCode());
            if (isBaseInfoIdentifier)
                Assert.IsNull(propertyType.ScriptableObjectFieldDefinitionCode());
            else
                Assert.IsNotNull(propertyType.ScriptableObjectFieldDefinitionCode());
            Assert.IsNotNull(propertyType.ScriptableObjectPropertyImplementationCode());

            if (expectImmutableFlatBufferData)
                Assert.IsNull(propertyType.FlatBufferFieldDefinitionCode());
            else
                Assert.IsNotEmpty(propertyType.FlatBufferFieldDefinitionCode());
            Assert.IsNotEmpty(propertyType.FlatBufferPropertyImplementationCode());
            Assert.IsNotEmpty(propertyType.FlatBufferEditPropertyCode("value"));
            if (isBaseInfoIdentifier || expectImmutableFlatBufferData)
                Assert.IsNull(propertyType.FlatBufferRemoveEditCode());
            else
                Assert.IsNotEmpty(propertyType.FlatBufferRemoveEditCode());

            if (expectNullPrepareSource)
                Assert.IsNull(propertyType.FlatBufferBuilderPrepareCode(TableName));
            else
                Assert.IsNotEmpty(propertyType.FlatBufferBuilderPrepareCode(TableName));
            Assert.IsNotEmpty(propertyType.FlatBufferBuilderCode(TableName));

            Assert.IsNotEmpty(propertyType.CSVBridgeColumnNameText);
            Assert.IsNotEmpty(propertyType.CSVBridgeColumnTypeText);
            Assert.IsNotEmpty(propertyType.CSVBridgeReadFromCSVCode("blah"));
            Assert.IsNotEmpty(propertyType.CSVBridgeUpdateCSVRowCode("blah"));

            propertyType.DefineFlatBufferSchema(_schemaBuilder, TableName);
        }

        [Test]
        public void PropertyTypeFactoryError()
        {
            var propertyInfo = typeof(ITestInfo).GetProperty(nameof(ITestInfo.UnsupportedType));
            Assert.IsNotNull(propertyInfo);
            var parameterInterfaceMock = MockedInterfaces.ParameterInfo("TestInfo");
            var propertyType = PropertyTypeFactory.Create(parameterInterfaceMock, propertyInfo, out string error);
            Assert.IsNull(propertyType);
            Assert.IsNotNull(error);
        }

        [Test]
        public void ValidateIdentifier()
        {
            var identifierPropertyType = CreatePropertyType(nameof(IBaseInfo.Identifier), typeof(IBaseInfo));
            AssertPropertyType(identifierPropertyType, isBaseInfoIdentifier: true);

            identifierPropertyType = CreatePropertyType(nameof(ITestStruct.Identifier), typeof(ITestStruct));
            AssertPropertyType(identifierPropertyType);
        }

        [Test]
        [TestCase(nameof(ITestInfo.MyBool), true)]
        [TestCase(nameof(ITestInfo.MyShort), true)]
        [TestCase(nameof(ITestInfo.MyInt), true)]
        [TestCase(nameof(ITestInfo.MyLong), true)]
        [TestCase(nameof(ITestInfo.MyFloat), true)]
        [TestCase(nameof(ITestInfo.MyUShort), true)]
        [TestCase(nameof(ITestInfo.MyUInt), true)]
        [TestCase(nameof(ITestInfo.MyULong), true)]
        [TestCase(nameof(ITestInfo.MyEnum), true)]
        [TestCase(nameof(ITestInfo.MyBools))]
        [TestCase(nameof(ITestInfo.MyShorts))]
        [TestCase(nameof(ITestInfo.MyInts))]
        [TestCase(nameof(ITestInfo.MyLongs))]
        [TestCase(nameof(ITestInfo.MyFloats))]
        [TestCase(nameof(ITestInfo.MyUShorts))]
        [TestCase(nameof(ITestInfo.MyUInts))]
        [TestCase(nameof(ITestInfo.MyULongs))]
        [TestCase(nameof(ITestInfo.MyEnums))]
        [TestCase(nameof(ITestInfo.MyColor), true)]
        [TestCase(nameof(ITestInfo.MyVector2), true)]
        [TestCase(nameof(ITestInfo.MyVector2Int), true)]
        [TestCase(nameof(ITestInfo.MyVector3), true)]
        [TestCase(nameof(ITestInfo.MyVector3Int), true)]
        [TestCase(nameof(ITestInfo.MyColors))]
        [TestCase(nameof(ITestInfo.MyVector2s))]
        [TestCase(nameof(ITestInfo.MyVector2Ints))]
        [TestCase(nameof(ITestInfo.MyVector3s))]
        [TestCase(nameof(ITestInfo.MyVector3Ints))]
        [TestCase(nameof(ITestInfo.MyDateTime), true)]
        [TestCase(nameof(ITestInfo.MyDateTimes))]
        [TestCase(nameof(ITestInfo.MyTimeSpan), true)]
        [TestCase(nameof(ITestInfo.MyTimeSpans))]
        [TestCase(nameof(ITestInfo.MyString))]
        [TestCase(nameof(ITestInfo.MyStrings))]
        [TestCase(nameof(ITestInfo.MyLocString))]
        [TestCase(nameof(ITestInfo.MyLocStrings))]
        [TestCase(nameof(ITestInfo.MyTestInfo))]
        [TestCase(nameof(ITestInfo.MyTestInfos))]
        [TestCase(nameof(ITestInfo.MyTestInfos))]
        [TestCase(nameof(ITestInfo.MyStruct), false, true)]
        [TestCase(nameof(ITestInfo.MyStructs))]
#if ADDRESSABLE_PARAMS
        [TestCase(nameof(ITestInfo.MyAsset))]
        [TestCase(nameof(ITestInfo.MyAssetSprite))]
        [TestCase(nameof(ITestInfo.MyAssetAltasdSprite))]
        [TestCase(nameof(ITestInfo.MyAssets))]
        [TestCase(nameof(ITestInfo.MyAssetSprites))]
        [TestCase(nameof(ITestInfo.MyAssetAltasdSprites))]
#endif
        public void ValidateTypes(string propertyName,
            bool expectNullPrepareSource = false,
            bool expectNullFlatBufferFieldDefinitionCode = false)
        {
            var propertyType = CreatePropertyType(propertyName);
            AssertPropertyType(propertyType,
                expectNullPrepareSource: expectNullPrepareSource,
                expectImmutableFlatBufferData: expectNullFlatBufferFieldDefinitionCode);
        }

        [Test]
        public void ValidateAttribute()
        {
            var propType = CreatePropertyType(nameof(ITestInfo.IntWithAttribute));
            var attributes = propType.ScriptableObjectFieldAttributesCode();
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("MyAttribute(\"blah\")", attributes[0]);
        }

        [Test]
        public void ValidateAttributes()
        {
            var propType = CreatePropertyType(nameof(ITestInfo.IntWithAttributes));
            var attributes = propType.ScriptableObjectFieldAttributesCode();
            Assert.AreEqual(2, attributes.Count);
            Assert.AreEqual("MyAttribute(\"blah1\")", attributes[0]);
            Assert.AreEqual("MyAttribute(\"blah2\")", attributes[1]);
        }
    }
}
