using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Validation
{
    public class ParameterManagerValidatorTest
    {
        private IMutableParameterManager _parameterManager;

        // info 1
        private MockTestValidationInfo _info1;
        private MockKeyValueStruct _info1KeyValueStruct1;
        private MockInnerKeyValueStruct _info1KeyValueStruct1Inner1;
        private MockInnerKeyValueStruct _info1KeyValueStruct1Inner2;
        private MockInnerKeyValueStruct _info1KeyValueStruct1Inner3;
        private MockKeyValueStruct _info1KeyValueStruct2;
        private MockInnerKeyValueStruct _info1KeyValueStruct2Inner;
        private MockKeyValueStruct _info1KeyValueStruct3;
        private MockInnerKeyValueStruct _info1KeyValueStruct3Inner;

        // info 2
        private MockTestValidationInfo _info2;
        private MockKeyValueStruct _info2KeyValueStruct;
        private MockInnerKeyValueStruct _info2KeyValueStructInner;

        // info 3
        private MockTestValidationInfo _info3;
        private MockKeyValueStruct _info3KeyValueStruct;
        private MockInnerKeyValueStruct _info3KeyValueStructInner;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ParameterManagerValidatorCache.Clear();
        }

        [SetUp]
        public void SetUp()
        {
            // info validators
            MySpecialInfoDataValidator.Instance = null;
            MySpecialInfoDataValidator.NextInstanceErrors = null;
            TestValidationInfoDataValidator.Instance = null;
            TestValidationInfoDataValidator.NextInstanceErrors = null;
            TestBaseValidationInfoDataValidator.Instance = null;
            TestBaseValidationInfoDataValidator.NextInstanceErrors = null;
            BadValidationInfoDataValidator.Instance = null;
            BadValidationInfoDataValidator.NextInstanceErrors = null;
            TestSuperInterfaceInfoDataValidator.Instance = null;
            TestSuperInterfaceInfoDataValidator.NextInstanceErrors = null;
            TestSubInterfaceInfoDataValidator.Instance = null;
            TestSubInterfaceInfoDataValidator.NextInstanceErrors = null;

            // struct validators
            KeyValueStructDataValidator.Instance = null;
            KeyValueStructDataValidator.NextInstanceErrors = null;
            InnerKeyValueStructDataValidator.Instance = null;
            InnerKeyValueStructDataValidator.NextInstanceErrors = null;

            _parameterManager = Substitute.For<IMutableParameterManager>();
            Params.SetInstance(_parameterManager);
        }

        [TearDown]
        public void TearDown()
        {
            Params.SetInstance(null);
        }

        private void SetupGeneralValidationParameters()
        {
            // structs from infos
            const string keyValueGuid1 = "keyValueGuid1";
            const string keyValueGuid2 = "keyValueGuid2";
            const string keyValueGuid3 = "keyValueGuid3";
            const string keyValueGuid4 = "keyValueGuid4";
            const string keyValueGuid5 = "keyValueGuid5";
            // nested structs
            const string innerKeyValueGuid1 = "innerKeyValueGuid1";
            const string innerKeyValueGuid2 = "innerKeyValueGuid2";
            const string innerKeyValueGuid3 = "innerKeyValueGuid3";
            const string innerKeyValueGuid4 = "innerKeyValueGuid4";
            const string innerKeyValueGuid5 = "innerKeyValueGuid5";
            const string innerKeyValueGuid6 = "innerKeyValueGuid6";
            const string innerKeyValueGuid7 = "innerKeyValueGuid7";

            // setup infos
            _info1 = new MockTestValidationInfo();
            _info1.Identifier = "Info1";
            _info1.DisplayName = "Some Display Name";
            _info1.Description = "Some Description";
            _info1.StructRef = new ParameterStructReferenceRuntime<IKeyValueStruct>(keyValueGuid1);
            _info1._structRefs = new[]
            {
                new ParameterStructReferenceRuntime<IKeyValueStruct>(keyValueGuid2),
                new ParameterStructReferenceRuntime<IKeyValueStruct>(keyValueGuid3)
            };

            _info2 = new MockTestValidationInfo();
            _info2.Identifier = "Info2";
            _info2.DisplayName = "Some Display Name";
            _info2.Description = "Some Description";
            _info2.StructRef = new ParameterStructReferenceRuntime<IKeyValueStruct>(keyValueGuid4);
            _info2._structRefs = Array.Empty<ParameterStructReferenceRuntime<IKeyValueStruct>>();

            _info3 = new MockTestValidationInfo();
            _info3.Identifier = "Info3";
            _info3.DisplayName = "Some Display Name";
            _info3.Description = "Some Description";
            _info3.StructRef = new ParameterStructReferenceRuntime<IKeyValueStruct>(keyValueGuid5);
            _info3._structRefs = Array.Empty<ParameterStructReferenceRuntime<IKeyValueStruct>>();

            var mocks = new[] { _info1, _info2, _info3 };
            _parameterManager.Get<ITestValidationInfo>().Returns(mocks);
            _parameterManager.Get<ITestBaseValidationInfo>().Returns(mocks);
            _parameterManager.Get<IMySpecialInfo>().Returns(mocks);

            // create key value structs
            _info1KeyValueStruct1 = new MockKeyValueStruct(
                "Some Description", 100, innerKeyValueGuid1, new[] { innerKeyValueGuid2, innerKeyValueGuid3 });
            _info1KeyValueStruct2 = new MockKeyValueStruct(
                "Some Description", 100, innerKeyValueGuid4, Array.Empty<string>());
            _info1KeyValueStruct3 = new MockKeyValueStruct(
                "Some Description", 100, innerKeyValueGuid5, Array.Empty<string>());
            _info2KeyValueStruct = new MockKeyValueStruct(
                "Some Description", 100, innerKeyValueGuid6, Array.Empty<string>());
            _info3KeyValueStruct = new MockKeyValueStruct(
                "Some Description", 100, innerKeyValueGuid7, Array.Empty<string>());
            _parameterManager.GetStructWithGuid<IKeyValueStruct>(keyValueGuid1).Returns(_info1KeyValueStruct1);
            _parameterManager.GetStructWithGuid<IKeyValueStruct>(keyValueGuid2).Returns(_info1KeyValueStruct2);
            _parameterManager.GetStructWithGuid<IKeyValueStruct>(keyValueGuid3).Returns(_info1KeyValueStruct3);
            _parameterManager.GetStructWithGuid<IKeyValueStruct>(keyValueGuid4).Returns(_info2KeyValueStruct);
            _parameterManager.GetStructWithGuid<IKeyValueStruct>(keyValueGuid5).Returns(_info3KeyValueStruct);

            // create inner key value structs
            _info1KeyValueStruct1Inner1 = new MockInnerKeyValueStruct("Description", 10);
            _info1KeyValueStruct1Inner2 = new MockInnerKeyValueStruct("Description", 10);
            _info1KeyValueStruct1Inner3 = new MockInnerKeyValueStruct("Description", 10);
            _info1KeyValueStruct2Inner = new MockInnerKeyValueStruct("Description", 10);
            _info1KeyValueStruct3Inner = new MockInnerKeyValueStruct("Description", 10);
            _info2KeyValueStructInner = new MockInnerKeyValueStruct("Description", 10);
            _info3KeyValueStructInner = new MockInnerKeyValueStruct("Description", 10);

            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(innerKeyValueGuid1).Returns(_info1KeyValueStruct1Inner1);
            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(innerKeyValueGuid2).Returns(_info1KeyValueStruct1Inner2);
            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(innerKeyValueGuid3).Returns(_info1KeyValueStruct1Inner3);
            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(innerKeyValueGuid4).Returns(_info1KeyValueStruct2Inner);
            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(innerKeyValueGuid5).Returns(_info1KeyValueStruct3Inner);
            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(innerKeyValueGuid6).Returns(_info2KeyValueStructInner);
            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(innerKeyValueGuid7).Returns(_info3KeyValueStructInner);
        }

        private void ValidateGeneralValidationParameters(out ParameterManagerValidator validator)
        {
            validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.Validate<ITestValidationInfo>();
            validator.Validate<ITestBaseValidationInfo>();
            validator.Validate<IMySpecialInfo>();
        }

        [Test]
        public void Pass_NoData()
        {
            // run validation
            var validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.Validate<IMySpecialInfo>();

            // results
            Assert.IsNotNull(MySpecialInfoDataValidator.Instance);
            Assert.AreEqual(0, MySpecialInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, MySpecialInfoDataValidator.Instance.ValidateParametersCalls);
            Assert.IsEmpty(validator.ValidationErrors);
        }

        [Test]
        public void Pass_Validation()
        {
            SetupGeneralValidationParameters();
            ValidateGeneralValidationParameters(out var validator);

            // results - info
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(3, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(5, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(7, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            Assert.IsEmpty(validator.ValidationErrors);
        }

        [Test]
        public void Fail_InfoValidator()
        {
            // setup
            var error1 = new ValidationError(typeof(int), null, null, null);
            var error2 = new ValidationError(typeof(int), null, null, null);
            TestValidationInfoDataValidator.NextInstanceErrors = new List<ValidationError>() { error1, error2 };

            SetupGeneralValidationParameters();
            ValidateGeneralValidationParameters(out var validator);

            // results - info
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(3, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(5, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(7, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            Assert.AreEqual(2, validator.ValidationErrors.Count);
            Assert.AreEqual(error1, validator.ValidationErrors[0]);
            Assert.AreEqual(error2, validator.ValidationErrors[1]);
        }

        [Test]
        public void Fail_StructValidator()
        {
            // setup
            var error1 = new ValidationError(typeof(int), null, null, null);
            var error2 = new ValidationError(typeof(int), null, null, null);
            KeyValueStructDataValidator.NextInstanceErrors = new List<ValidationError>() { error1 };
            InnerKeyValueStructDataValidator.NextInstanceErrors = new List<ValidationError>() { error2 };

            SetupGeneralValidationParameters();
            ValidateGeneralValidationParameters(out var validator);

            // results - info
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(3, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNotNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(5, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(7, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            Assert.AreEqual(2, validator.ValidationErrors.Count);
            Assert.AreEqual(error1, validator.ValidationErrors[0]);
            Assert.AreEqual(error2, validator.ValidationErrors[1]);
        }

        [Test]
        public void Failed_ExceptionHandling()
        {
            // setup
            var structGuid = "someStructGuid";
            var structReference = new ParameterStructReferenceRuntime<IExceptionStruct>(structGuid);
            var testExceptionInfo1 = Substitute.For<ITestExceptionInfo>();
            testExceptionInfo1.ExceptionStruct.Returns(structReference);
            var testExceptionInfo2 = Substitute.For<ITestExceptionInfo>();
            testExceptionInfo2.ExceptionStruct.Returns(structReference);
            _parameterManager.Get<ITestExceptionInfo>().Returns(new[] { testExceptionInfo1, testExceptionInfo2 });
            _parameterManager.GetStructWithGuid<IExceptionStruct>(structGuid)
                .Returns(Substitute.For<IExceptionStruct>());

            // run validation
            var validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.Validate<ITestExceptionInfo>();

            // results - info
            Assert.IsNotNull(TestExceptionInfoDataValidator.Instance);
            Assert.AreEqual(2, TestExceptionInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestExceptionInfoDataValidator.Instance.ValidateParametersCalls);

            // results - struct
            Assert.IsNotNull(TestExceptionStructDataValidator.Instance);
            Assert.AreEqual(2, TestExceptionStructDataValidator.Instance.ValidateStructCalls);

            // results - catches exception while running attributes & validator
            // 1 error: exception in TestExceptionInfoDataValidator.ValidateParameters
            // 2 errors: one exception for each call to TestExceptionInfoDataValidator.ValidateInfo
            // 1 errors: one exception for TestException(0) attribute called during TestExceptionAttribute.CanValidate
            // 1 errors: one exception for TestException(1) attribute called during TestExceptionAttribute.WillValidateProperty
            // 2 errors: one exception for each TestException(2) attribute called during TestExceptionAttribute.Validate
            // 2 from structs errors: one exception for TestException(0) attribute called during TestExceptionAttribute.CanValidate
            // 2 from structs errors: one exception for TestException(1) attribute called during TestExceptionAttribute.WillValidateProperty
            // 2 from structs errors: one exception for each TestException(2) attribute called during TestExceptionAttribute.Validate
            const int expectedErrors = 13;
            Assert.AreEqual(expectedErrors, validator.ValidationErrors.Count);
            for (int i = 0; i < expectedErrors; i++)
                LogAssert.Expect(LogType.Error, new Regex(".*Exception.*"));
        }

        [Test]
        public void Fail_CorrectlyCountingSubInterfaces()
        {
            var mock1 = new MockSubBadValidationInfo() { Value = 6, SubValue = 6, };
            var mock2 = new MockSubBadValidationInfo() { Value = 0, SubValue = 0, };
            _parameterManager.Get<ITestSuperInterfaceInfo>().Returns(new[]
            {
                mock1, mock2
            });

            _parameterManager.Get<ITestSubInterfaceInfo>().Returns(new[]
            {
                mock1, mock2
            });

            // run validation
            var validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.Validate<ITestSuperInterfaceInfo>();
            validator.Validate<ITestSubInterfaceInfo>();

            Assert.IsNotNull(TestSuperInterfaceInfoDataValidator.Instance);
            Assert.AreEqual(2, TestSuperInterfaceInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestSuperInterfaceInfoDataValidator.Instance.ValidateParametersCalls);

            Assert.IsNotNull(TestSubInterfaceInfoDataValidator.Instance);
            Assert.AreEqual(2, TestSubInterfaceInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestSubInterfaceInfoDataValidator.Instance.ValidateParametersCalls);

            // results - correctly counting & validating errors of interfaces & sub interfaces
            // results - 2 errors for value and subvalue not greater than 0
            Assert.AreEqual(2, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_MissingValidator()
        {
            // run validation
            var validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.Validate<IMyVerySpecialInfo>();
            validator.Validate<IMySpecialInfo>();

            Assert.IsNotNull(MySpecialInfoDataValidator.Instance);

            /*
             * 1 error total:
             * validator for IMyVerySpecialInfo missing
             * validator for IMissingValidator1Struct missing but not fetched due to no data referencing structs (referenced by IMyVerySpecialInfo)
             * validator for IMissingValidator2Struct missing but not fetched due to no data referencing structs (referenced by IMyVerySpecialInfo)
             */
            Assert.AreEqual(1, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_MissingValidatorWithStructs()
        {
            // populate 1 MockMyVerySpecialInfo so that structs are detected
            var structGuid1 = "structGuid1";
            var structGuid2 = "structGuid2";
            var mocks = new[]
            {
                new MockMyVerySpecialInfo() {
                    Struct = new ParameterStructReferenceRuntime<IMissingValidator1Struct>(structGuid1),
                    Structs = new []{ new ParameterStructReferenceRuntime<IMissingValidator2Struct>(structGuid2) }
                },
            };
            _parameterManager.Get<IMyVerySpecialInfo>().Returns(mocks);
            _parameterManager.GetStructWithGuid<IMissingValidator1Struct>(structGuid1)
                .Returns(new MockMyVerySpecial1Struct());
            _parameterManager.GetStructWithGuid<IMissingValidator2Struct>(structGuid2)
                .Returns(new MockMyVerySpecial2Struct());

            // run validation
            var validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.Validate<IMyVerySpecialInfo>();
            validator.Validate<IMySpecialInfo>();

            Assert.IsNotNull(MySpecialInfoDataValidator.Instance);

            /*
             * 3 errors total:
             * validator for IMyVerySpecialInfo missing
             * validator for IMissingValidator1Struct missing (referenced by IMyVerySpecialInfo)
             * validator for IMissingValidator2Struct missing (referenced by IMyVerySpecialInfo)
             */
            Assert.AreEqual(3, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_NonCompatibleAttribute()
        {
            // run validation
            var validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.Validate<IBadValidationInfo>();

            Assert.IsNotNull(BadValidationInfoDataValidator.Instance);
            Assert.AreEqual(0, BadValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, BadValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - 1 error in-compatible attribute
            Assert.AreEqual(1, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_BuiltInAttribute()
        {
            SetupGeneralValidationParameters();

            // setup bad parameters
            string badInfoGuid = "bad_info_guid";
            _parameterManager.GetWithGUID<ITestValidationInfo>(badInfoGuid).ReturnsNull();
            string badKeyValueStructGuid = "bad_key_value_struct_guid";
            _parameterManager.GetStructWithGuid<IKeyValueStruct>(badKeyValueStructGuid).ReturnsNull();
            string badInnerStructGuid = "bad_inner_struct_guid";
            _parameterManager.GetStructWithGuid<IInnerKeyValueStruct>(badInnerStructGuid).ReturnsNull();

            _info1.Ref = new ParameterReference<ITestValidationInfo>(badInfoGuid);
            _info1._structRefs[1] = new ParameterStructReferenceRuntime<IKeyValueStruct>(badKeyValueStructGuid);
            _info1KeyValueStruct1._innerStructs[0] = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>(badInnerStructGuid);
            _info1KeyValueStruct2.InnerStruct = new ParameterStructReferenceRuntime<IInnerKeyValueStruct>(badInnerStructGuid);
            _info2.StructRef = new ParameterStructReferenceRuntime<IKeyValueStruct>(badKeyValueStructGuid);

            ValidateGeneralValidationParameters(out var validator);

            // results - info
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(3, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs (those that weren't nulled)
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(3, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs (those that weren't nulled or had their KeyValue parents nulled)
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(3, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results:
            // built-in attribute for checking broken 1 ParameterReference
            // built-in attribute for checking broken 4 ParameterStructReferences & lists
            Assert.AreEqual(5, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_AttributeInInfo()
        {
            SetupGeneralValidationParameters();

            // setup bad parameters
            _info2.DisplayName = "";
            _info3.DisplayName = null;
            _info3.Description = null;

            ValidateGeneralValidationParameters(out var validator);

            // results - info
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(3, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(5, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(7, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            /*
             * 3 Errors in total:
             * 2 DisplayName properties are empty/null
             * 1 Description is empty/null
             */
            Assert.AreEqual(3, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_AttributeInStruct()
        {
            SetupGeneralValidationParameters();

            // setup bad parameters
            _info1KeyValueStruct1.Description = "";
            _info1KeyValueStruct1.Value = -1;
            _info1KeyValueStruct2.Value = -1;
            _info1KeyValueStruct3.Description = null;
            _info1KeyValueStruct1Inner1.Value = -1;
            _info1KeyValueStruct1Inner2.Description = "";
            _info1KeyValueStruct1Inner3.Value = -100;
            _info1KeyValueStruct1Inner3.Description = "";

            ValidateGeneralValidationParameters(out var validator);

            // results - info
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(3, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(5, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(7, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            /*
             * 8 Errors in total:
             * _info1KeyValueStruct1: empty description
             * _info1KeyValueStruct1: -1 value
             * _info1KeyValueStruct2: -1 value
             * _info1KeyValueStruct3: null description
             * _info1KeyValueStruct1Inner1: -1 value
             * _info1KeyValueStruct1Inner2: empty description
             * _info1KeyValueStruct1Inner3: -100 value
             * _info1KeyValueStruct1Inner3: empty description
             */
            Assert.AreEqual(8, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_NullParameterStructReferences()
        {
            SetupGeneralValidationParameters();

            // setup bad parameters
            _info1._structRefs[1] = null;
            _info1KeyValueStruct1._innerStructs[0] = null;
            _info1KeyValueStruct2.InnerStruct = null;
            _info2.StructRef = null;

            ValidateGeneralValidationParameters(out var validator);

            // results - info
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(3, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs that weren't nulled
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(3, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs that weren't nulled or ad the KeyValue struct nulled
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(3, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results:
            // null struct references
            Assert.AreEqual(4, validator.ValidationErrors.Count);
        }

        [Test]
        public void Fail_ValidateSpecificObject()
        {
            SetupGeneralValidationParameters();

            // setup error
            var error1 = new ValidationError(typeof(int), null, null, null);
            TestValidationInfoDataValidator.NextInstanceErrors = new List<ValidationError>() { error1 };

            // setup bad parameters
            _info1.DisplayName = "";
            _info1KeyValueStruct1.Description = "";
            _info1KeyValueStruct2.Value = -1;
            _info1KeyValueStruct1Inner1.Description = null;
            _info1KeyValueStruct1Inner2.Value = -100;
            _info1KeyValueStruct1Inner3.Description = "";

            // run validation
            var validator = new ParameterManagerValidator(_parameterManager);
            Assert.IsEmpty(validator.ValidationErrors);
            validator.ValidateObject<ITestValidationInfo>(_info1);

            // results - info (only 1 info)
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance);
            Assert.IsNotNull(TestValidationInfoDataValidator.Instance.Errors);
            Assert.AreEqual(1, TestValidationInfoDataValidator.Instance.ValidateInfoCalls);
            Assert.AreEqual(0, TestValidationInfoDataValidator.Instance.ValidateParametersCalls);

            // results - structs (only structs in info)
            Assert.IsNotNull(KeyValueStructDataValidator.Instance);
            Assert.IsNull(KeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(3, KeyValueStructDataValidator.Instance.ValidateStructCalls);

            // results - structs (only structs from struts above)
            Assert.IsNotNull(InnerKeyValueStructDataValidator.Instance);
            Assert.IsNull(InnerKeyValueStructDataValidator.Instance.Errors);
            Assert.AreEqual(5, InnerKeyValueStructDataValidator.Instance.ValidateStructCalls);

            /*
             * 7 Errors in total:
             * 1 DisplayName properties are empty/null
             * 1 error in TestValidationDataValidator
             * _info1KeyValueStruct1: empty description
             * _info1KeyValueStruct2: -1 value
             * _info1KeyValueStruct1Inner1: null description
             * _info1KeyValueStruct1Inner2: -1 value
             * _info1KeyValueStruct1Inner3: empty description
             */
            Assert.AreEqual(7, validator.ValidationErrors.Count);
        }

    }
}
