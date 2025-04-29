using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PocketGems.Parameters.Types;
using UnityEngine;
#if ADDRESSABLE_PARAMS
using UnityEngine.AddressableAssets;
#endif
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace PocketGems.Parameters.Validation.Attributes
{
    public class AttributesTest
    {
        private IMutableParameterManager _parameterManager;

        private string String { get; set; }
        private IReadOnlyList<string> StringArray { get; set; }
        private LocalizedString LocalizedString { get; set; }
        private IReadOnlyList<LocalizedString> LocalizedStringArray { get; set; }
        private ParameterReference<IMySpecialInfo> ParameterReference { get; set; }
        private IReadOnlyList<ParameterReference<IMySpecialInfo>> ParameterReferenceArray { get; set; }
#if ADDRESSABLE_PARAMS
        private AssetReference AssetReference { get; set; }
        private AssetReferenceSprite AssetReferenceSprite { get; set; }
        private IReadOnlyList<AssetReference> AssetReferenceArray { get; set; }
        private IReadOnlyList<AssetReferenceSprite> AssetReferenceSpriteArray { get; set; }
#endif
        private short Short { get; set; }
        private int Int { get; set; }
        private long Long { get; set; }
        private IReadOnlyList<short> ShortArray { get; set; }
        private IReadOnlyList<int> IntArray { get; set; }
        private IReadOnlyList<long> LongArray { get; set; }
        private float Float { get; set; }
        private IReadOnlyList<float> FloatArray { get; set; }

        [SetUp]
        public void SetUp()
        {
            _parameterManager = new ParameterManager();

            String = null;
            StringArray = null;
            LocalizedString = null;
            LocalizedStringArray = null;
            ParameterReference = null;
            ParameterReferenceArray = null;
#if ADDRESSABLE_PARAMS
            AssetReference = null;
            AssetReferenceArray = null;
            AssetReferenceSprite = null;
            AssetReferenceSpriteArray = null;
#endif
            ShortArray = null;
            IntArray = null;
            LongArray = null;
            FloatArray = null;
        }

        private void AssertAttribute(IValidationAttribute attribute, string propertyName, object input, bool isValid)
        {
            var propertyInfo = GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue(attribute.CanValidate(propertyInfo));
            attribute.WillValidateProperty(_parameterManager, propertyInfo);
            var errorString = attribute.Validate(_parameterManager, propertyInfo, input);
            Assert.AreEqual(isValid, errorString == null);
        }

        private void AssertCanValidate(IValidationAttribute attribute, string propertyName, bool canValidate)
        {
            var propertyInfo = GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.AreEqual(canValidate, attribute.CanValidate(propertyInfo));
        }

        private void AssertConstraintBaseTests(IValidationAttribute attribute, bool isFloat)
        {
            AssertCanValidate(attribute, nameof(String), false);
            if (!isFloat)
            {
                // empty arrays
                AssertAttribute(attribute, nameof(ShortArray), ShortArray, true);
                ShortArray = Array.Empty<short>();
                AssertAttribute(attribute, nameof(ShortArray), ShortArray, true);
                AssertAttribute(attribute, nameof(IntArray), IntArray, true);
                IntArray = Array.Empty<int>();
                AssertAttribute(attribute, nameof(IntArray), IntArray, true);
                AssertAttribute(attribute, nameof(LongArray), LongArray, true);
                LongArray = Array.Empty<long>();
                AssertAttribute(attribute, nameof(LongArray), LongArray, true);

                // not compatible
                AssertCanValidate(attribute, nameof(Float), false);
                AssertCanValidate(attribute, nameof(FloatArray), false);
            }
            else
            {
                // empty arrays
                AssertAttribute(attribute, nameof(FloatArray), FloatArray, true);
                FloatArray = Array.Empty<float>();
                AssertAttribute(attribute, nameof(FloatArray), FloatArray, true);

                // not compatible
                AssertCanValidate(attribute, nameof(Int), false);
                AssertCanValidate(attribute, nameof(IntArray), false);
            }
        }

        private void AssertIntConstraintAttribute(IValidationAttribute attribute, int input, bool isValid)
        {
            Short = (short)input;
            AssertAttribute(attribute, nameof(Short), Short, isValid);
            Int = input;
            AssertAttribute(attribute, nameof(Int), Int, isValid);
            Long = input;
            AssertAttribute(attribute, nameof(Long), Long, isValid);

            ShortArray = new[] { (short)input, (short)input };
            AssertAttribute(attribute, nameof(ShortArray), ShortArray, isValid);
            IntArray = new[] { input, input };
            AssertAttribute(attribute, nameof(IntArray), IntArray, isValid);
            LongArray = new[] { (long)input, (long)input };
            AssertAttribute(attribute, nameof(LongArray), LongArray, isValid);
        }

        private void AssertFloatConstraintAttribute(IValidationAttribute attribute, float input, bool isValid)
        {
            Float = input;
            AssertAttribute(attribute, nameof(Float), Float, isValid);
        }

        [Test]
        public void AssertGreater()
        {
            var attribute = new AssertGreaterAttribute(10);
            AssertConstraintBaseTests(attribute, false);

            // true
            AssertIntConstraintAttribute(attribute, 11, true);

            // false
            AssertIntConstraintAttribute(attribute, 10, false);
            AssertIntConstraintAttribute(attribute, 0, false);
            AssertIntConstraintAttribute(attribute, -10, false);


            attribute = new AssertGreaterAttribute(10f);
            AssertConstraintBaseTests(attribute, true);

            // true
            AssertFloatConstraintAttribute(attribute, 11f, true);

            // false
            AssertFloatConstraintAttribute(attribute, 10f, false);
            AssertFloatConstraintAttribute(attribute, 0f, false);
            AssertFloatConstraintAttribute(attribute, -10f, false);
        }

        [Test]
        public void AssertGreaterOrEqual()
        {
            var attribute = new AssertGreaterOrEqualAttribute(10);
            AssertConstraintBaseTests(attribute, false);

            // true
            AssertIntConstraintAttribute(attribute, 10, true);
            AssertIntConstraintAttribute(attribute, 11, true);

            // false
            AssertIntConstraintAttribute(attribute, 0, false);
            AssertIntConstraintAttribute(attribute, -10, false);

            attribute = new AssertGreaterOrEqualAttribute(10f);
            AssertConstraintBaseTests(attribute, true);

            // true
            AssertFloatConstraintAttribute(attribute, 10f, true);
            AssertFloatConstraintAttribute(attribute, 11f, true);

            // false
            AssertFloatConstraintAttribute(attribute, 0f, false);
            AssertFloatConstraintAttribute(attribute, -10f, false);
        }

        [Test]
        public void AssertLess()
        {
            var attribute = new AssertLessAttribute(10);
            AssertConstraintBaseTests(attribute, false);

            // true
            AssertIntConstraintAttribute(attribute, 0, true);

            // false
            AssertIntConstraintAttribute(attribute, 10, false);
            AssertIntConstraintAttribute(attribute, 11, false);
            AssertIntConstraintAttribute(attribute, 15, false);

            attribute = new AssertLessAttribute(10f);
            AssertConstraintBaseTests(attribute, true);

            // true
            AssertFloatConstraintAttribute(attribute, 9f, true);

            // false
            AssertFloatConstraintAttribute(attribute, 10f, false);
            AssertFloatConstraintAttribute(attribute, 11f, false);
            AssertFloatConstraintAttribute(attribute, 15.5f, false);
        }

        [Test]
        public void AssertLessOrEqual()
        {
            var attribute = new AssertLessOrEqual(10);
            AssertConstraintBaseTests(attribute, false);

            // true
            AssertIntConstraintAttribute(attribute, 0, true);
            AssertIntConstraintAttribute(attribute, 10, true);

            // false
            AssertIntConstraintAttribute(attribute, 11, false);
            AssertIntConstraintAttribute(attribute, 15, false);

            attribute = new AssertLessOrEqual(10f);
            AssertConstraintBaseTests(attribute, true);

            // true
            AssertFloatConstraintAttribute(attribute, 9f, true);
            AssertFloatConstraintAttribute(attribute, 10f, true);

            // false
            AssertFloatConstraintAttribute(attribute, 11f, false);
            AssertFloatConstraintAttribute(attribute, 15.5f, false);
        }

        [Test]
        public void AssertRegex()
        {
            var attribute = new AssertRegexAttribute(".*\\.png$");

            // can validate
            AssertCanValidate(attribute, nameof(String), true);
            AssertCanValidate(attribute, nameof(StringArray), true);
            AssertCanValidate(attribute, nameof(Int), false);
            AssertCanValidate(attribute, nameof(IntArray), false);

            // true
            AssertAttribute(attribute, nameof(String), "test.png", true);
            AssertAttribute(attribute, nameof(StringArray), null, true);
            AssertAttribute(attribute, nameof(StringArray), Array.Empty<string>(), true);
            AssertAttribute(attribute, nameof(StringArray), new[] { "test.png" }, true);
            AssertAttribute(attribute, nameof(StringArray), new[] { "test.png", "test.png" }, true);

            // false
            AssertAttribute(attribute, nameof(String), "testpng", false);
            AssertAttribute(attribute, nameof(String), "test.PNG", false);
            AssertAttribute(attribute, nameof(String), "", false);
            AssertAttribute(attribute, nameof(String), null, false);
            AssertAttribute(attribute, nameof(StringArray), new[] { "testpng" }, false);
            AssertAttribute(attribute, nameof(StringArray), new[] { "test.png", "testpng" }, false);
            AssertAttribute(attribute, nameof(StringArray), new[] { "test.PNG", "test.png" }, false);

            // bad attributes
            AssertAttribute(new AssertRegexAttribute(""), nameof(String), "", false);
            AssertAttribute(new AssertRegexAttribute(""), nameof(StringArray), Array.Empty<string>(), false);
            AssertAttribute(new AssertRegexAttribute(null), nameof(String), null, false);
            AssertAttribute(new AssertRegexAttribute(null), nameof(StringArray), null, false);
        }

        [Test]
        public void AssertAssignedReference()
        {
            var attribute = new AssertAssignedReferenceAttribute();
            const string testGuid = "abcd";

            // true - references
            AssertAttribute(attribute, nameof(ParameterReference), new ParameterReference<IMySpecialInfo>(_parameterManager, testGuid), true);
#if ADDRESSABLE_PARAMS
            AssertAttribute(attribute, nameof(AssetReference), new AssetReference(testGuid), true);
            AssertAttribute(attribute, nameof(AssetReferenceSprite), new AssetReferenceSprite(testGuid), true);
#endif

            // true - list of  references
            ParameterReferenceArray = new[] {
                new ParameterReference<IMySpecialInfo>(_parameterManager, testGuid),
                new ParameterReference<IMySpecialInfo>(_parameterManager, testGuid) };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), null, true);
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, true);

#if ADDRESSABLE_PARAMS
            AssetReferenceArray = new[] {
                new AssetReference(testGuid),
                new AssetReference(testGuid) };
            AssertAttribute(attribute, nameof(AssetReferenceArray), AssetReferenceArray, true);

            AssetReferenceSpriteArray = new[] {
                    new AssetReferenceSprite(testGuid),
                    new AssetReferenceSprite(testGuid) };
            AssertAttribute(attribute, nameof(AssetReferenceSpriteArray), AssetReferenceSpriteArray, true);
            AssertAttribute(attribute, nameof(AssetReferenceArray), Array.Empty<AssetReference>(), true);
            AssertAttribute(attribute, nameof(AssetReferenceSpriteArray), Array.Empty<AssetReferenceSprite>(), true);
#endif
            AssertAttribute(attribute, nameof(ParameterReferenceArray), Array.Empty<ParameterReference<IMySpecialInfo>>(), true);

            // false references
            AssertAttribute(attribute, nameof(ParameterReference), null, false);
            AssertAttribute(attribute, nameof(ParameterReference), new ParameterReference<IMySpecialInfo>(_parameterManager), false);
            AssertAttribute(attribute, nameof(ParameterReference), new ParameterReference<IMySpecialInfo>(_parameterManager, ""), false);
            AssertAttribute(attribute, nameof(ParameterReference), new ParameterReference<IMySpecialInfo>(_parameterManager, "identifier", true), false);
#if ADDRESSABLE_PARAMS
            AssertAttribute(attribute, nameof(AssetReference), new AssetReference(), false);
            AssertAttribute(attribute, nameof(AssetReference), new AssetReference(""), false);
            AssertAttribute(attribute, nameof(AssetReferenceSprite), new AssetReferenceSprite(""), false);
#endif

            // false - list of references
            ParameterReferenceArray = new[] {
                new ParameterReference<IMySpecialInfo>(_parameterManager, testGuid),
                null };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);
            ParameterReferenceArray = new[] {
                new ParameterReference<IMySpecialInfo>(_parameterManager, testGuid),
                new ParameterReference<IMySpecialInfo>(_parameterManager) };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);
            ParameterReferenceArray = new[] {
                new ParameterReference<IMySpecialInfo>(_parameterManager, testGuid),
                new ParameterReference<IMySpecialInfo>(_parameterManager, "") };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);
            ParameterReferenceArray = new[] {
                new ParameterReference<IMySpecialInfo>(_parameterManager, testGuid),
                new ParameterReference<IMySpecialInfo>(_parameterManager, "identifier", true) };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);

#if ADDRESSABLE_PARAMS
            AssetReferenceArray = new[] {
                new AssetReference(testGuid),
                new AssetReference() };
            AssertAttribute(attribute, nameof(AssetReferenceArray), AssetReferenceArray, false);
            AssetReferenceArray = new[] {
                new AssetReference(testGuid),
                new AssetReference("") };
            AssertAttribute(attribute, nameof(AssetReferenceArray), AssetReferenceArray, false);

            AssetReferenceSpriteArray = new[] {
                    new AssetReferenceSprite(testGuid),
                    new AssetReferenceSprite("") };
            AssertAttribute(attribute, nameof(AssetReferenceSpriteArray), AssetReferenceSpriteArray, false);
#endif

            // invalid
            AssertCanValidate(attribute, nameof(String), false);
            AssertCanValidate(attribute, nameof(IntArray), false);
            AssertCanValidate(attribute, nameof(Int), false);
        }

        [Test]
        public void AssertStringNotEmpty()
        {
            var attribute = new AssertStringNotEmptyAttribute();

            // true
            String = "a string";
            LocalizedString = new LocalizedString("a string");
            AssertAttribute(attribute, nameof(String), String, true);
            AssertAttribute(attribute, nameof(LocalizedString), LocalizedString, true);

            // true empty arrays
            StringArray = Array.Empty<string>();
            LocalizedStringArray = Array.Empty<LocalizedString>();
            AssertAttribute(attribute, nameof(StringArray), StringArray, true);
            AssertAttribute(attribute, nameof(LocalizedStringArray), LocalizedStringArray, true);
            StringArray = null;
            LocalizedStringArray = null;
            AssertAttribute(attribute, nameof(StringArray), StringArray, true);
            AssertAttribute(attribute, nameof(LocalizedStringArray), LocalizedStringArray, true);

            // true filled arrays
            StringArray = new[] { "blah", "boo" };
            LocalizedStringArray = new[] { new LocalizedString("blah"), new LocalizedString("boo") };
            AssertAttribute(attribute, nameof(StringArray), StringArray, true);
            AssertAttribute(attribute, nameof(LocalizedStringArray), LocalizedStringArray, true);

            // false - null or empty strings
            String = "";
            AssertAttribute(attribute, nameof(String), String, false);
            String = null;
            AssertAttribute(attribute, nameof(String), String, false);
            LocalizedString = new LocalizedString("");
            AssertAttribute(attribute, nameof(LocalizedString), LocalizedString, false);
            LocalizedString = new LocalizedString(null);
            AssertAttribute(attribute, nameof(LocalizedString), LocalizedString, false);

            // false - null or empty strings in arrays
            StringArray = new[] { "", "boo" };
            AssertAttribute(attribute, nameof(StringArray), StringArray, false);
            StringArray = new[] { null, "boo" };
            AssertAttribute(attribute, nameof(StringArray), StringArray, false);
            LocalizedStringArray = new[] { null, new LocalizedString("boo") };
            AssertAttribute(attribute, nameof(LocalizedStringArray), LocalizedStringArray, false);
            LocalizedStringArray = new[] { new LocalizedString("blah"), new LocalizedString("") };
            AssertAttribute(attribute, nameof(LocalizedStringArray), LocalizedStringArray, false);

            // invalid
            AssertCanValidate(attribute, nameof(Int), false);
        }

        [Test]
        public void AssertAssignedReferenceExists()
        {
            var attribute = new AssertAssignedReferenceExistsAttribute();

            // can validate
            AssertCanValidate(attribute, nameof(ParameterReference), true);
            AssertCanValidate(attribute, nameof(ParameterReferenceArray), true);
#if ADDRESSABLE_PARAMS
            AssertCanValidate(attribute, nameof(AssetReference), true);
            AssertCanValidate(attribute, nameof(AssetReferenceArray), true);
            AssertCanValidate(attribute, nameof(AssetReferenceSprite), true);
            AssertCanValidate(attribute, nameof(AssetReferenceSpriteArray), true);
#endif
            AssertCanValidate(attribute, nameof(Int), false);
            AssertCanValidate(attribute, nameof(IntArray), false);

            // valid - null
            AssertAttribute(attribute, nameof(ParameterReference), ParameterReference, true);
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, true);
#if ADDRESSABLE_PARAMS
            AssertAttribute(attribute, nameof(AssetReference), AssetReference, true);
            AssertAttribute(attribute, nameof(AssetReferenceArray), AssetReferenceArray, true);
            AssertAttribute(attribute, nameof(AssetReferenceSprite), AssetReferenceSprite, true);
            AssertAttribute(attribute, nameof(AssetReferenceSpriteArray), AssetReferenceSpriteArray, true);
#endif

            // valid - unassigned
            ParameterReference = new ParameterReference<IMySpecialInfo>(_parameterManager);
            ParameterReferenceArray = new[] { new ParameterReference<IMySpecialInfo>(_parameterManager) };
            AssertAttribute(attribute, nameof(ParameterReference), ParameterReference, true);
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, true);

#if ADDRESSABLE_PARAMS
            AssetReference = new AssetReference();
            AssetReferenceArray = new[] { new AssetReference() };
            AssetReferenceSprite = new AssetReferenceSprite("");
            AssetReferenceSpriteArray = new[] { new AssetReferenceSprite("") };
            AssertAttribute(attribute, nameof(AssetReference), AssetReference, true);
            AssertAttribute(attribute, nameof(AssetReferenceArray), AssetReferenceArray, true);
            AssertAttribute(attribute, nameof(AssetReferenceSprite), AssetReferenceSprite, true);
            AssertAttribute(attribute, nameof(AssetReferenceSpriteArray), AssetReferenceSpriteArray, true);
#endif

            // invalid
            AssertCanValidate(attribute, nameof(Int), false);
            AssertCanValidate(attribute, nameof(IntArray), false);

            // valid - valid references
            const string validGuid = "some_guid";
            const string validIdentifier = "someIdentifier";
            var info = new MockMySpecialInfo();
            _parameterManager.Load<IMySpecialInfo, MockMySpecialInfo>(info, validIdentifier, validGuid);
            ParameterReference = new ParameterReference<IMySpecialInfo>(_parameterManager, validGuid);
            AssertAttribute(attribute, nameof(ParameterReference), ParameterReference, true);
            ParameterReference = new ParameterReference<IMySpecialInfo>(_parameterManager, validIdentifier, true);
            AssertAttribute(attribute, nameof(ParameterReference), ParameterReference, true);
            ParameterReferenceArray = new[] { new ParameterReference<IMySpecialInfo>(_parameterManager, validGuid) };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, true);
            ParameterReferenceArray = new[] { new ParameterReference<IMySpecialInfo>(_parameterManager, validIdentifier, true) };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, true);

            // valid addressables - difficult to test without messing with existing groups
            // AssertAttribute(attribute, nameof(AssetReference), AssetReference, true);
            // AssertAttribute(attribute, nameof(AssetReferenceArray), AssetReferenceArray, true);
            // AssertAttribute(attribute, nameof(AssetReferenceSprite), AssetReferenceSprite, true);
            // AssertAttribute(attribute, nameof(AssetReferenceSpriteArray), AssetReferenceSpriteArray, true);

            // false
            const string falseGuid = "abcd";
            ParameterReference = new ParameterReference<IMySpecialInfo>(_parameterManager, falseGuid);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            AssertAttribute(attribute, nameof(ParameterReference), ParameterReference, false);
            ParameterReference = new ParameterReference<IMySpecialInfo>(_parameterManager, falseGuid, true);
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            AssertAttribute(attribute, nameof(ParameterReference), ParameterReference, false);
            ParameterReferenceArray = new[] { new ParameterReference<IMySpecialInfo>(_parameterManager, falseGuid) };
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);
            ParameterReferenceArray = new[] { new ParameterReference<IMySpecialInfo>(_parameterManager, falseGuid, true) };
            LogAssert.Expect(LogType.Error, new Regex(".*"));
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);

#if ADDRESSABLE_PARAMS
            AssetReference = new AssetReference(falseGuid);
            AssetReferenceArray = new[] { new AssetReference(falseGuid) };
            AssetReferenceSprite = new AssetReferenceSprite(falseGuid);
            AssetReferenceSpriteArray = new[] { new AssetReferenceSprite(falseGuid) };
            AssertAttribute(attribute, nameof(AssetReference), AssetReference, false);
            AssertAttribute(attribute, nameof(AssetReferenceArray), AssetReferenceArray, false);
            AssertAttribute(attribute, nameof(AssetReferenceSprite), AssetReferenceSprite, false);
            AssertAttribute(attribute, nameof(AssetReferenceSpriteArray), AssetReferenceSpriteArray, false);
#endif
        }

        [Test]
        public void AssertListNotEmpty()
        {
            var attribute = new AssertListNotEmptyAttribute();

            // false - invalid
            AssertCanValidate(attribute, nameof(Int), false);

            // false - null or size 0
            AssertAttribute(attribute, nameof(IntArray), IntArray, false);
            IntArray = Array.Empty<int>();
            AssertAttribute(attribute, nameof(IntArray), IntArray, false);
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);
            ParameterReferenceArray = Array.Empty<ParameterReference<IMySpecialInfo>>();
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, false);

            // true
            IntArray = new[] { 10, 11 };
            AssertAttribute(attribute, nameof(IntArray), IntArray, true);
            ParameterReferenceArray = new[] { new ParameterReference<IMySpecialInfo>(_parameterManager) };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, true);
            ParameterReferenceArray = new ParameterReference<IMySpecialInfo>[] { null };
            AssertAttribute(attribute, nameof(ParameterReferenceArray), ParameterReferenceArray, true);
        }
    }
}
