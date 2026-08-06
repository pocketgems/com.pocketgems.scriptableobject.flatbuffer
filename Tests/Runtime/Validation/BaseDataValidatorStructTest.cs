using NSubstitute;
using NUnit.Framework;

namespace PocketGems.Parameters.Validation
{
    public class BaseDataValidatorStructTest
    {
        private const string infoIdentifier = "some id";
        private const string parentPropertyName = "parent property name";
        private const string structPath = "some path";

        [Test]
        public void ValidateStruct()
        {
            var infoMock = Substitute.For<ISubInterfaceAInfo>();
            infoMock.Identifier.Returns(infoIdentifier);

            var structMock = Substitute.For<IKeyValueStruct>();

            var validationObjectData = new ValidationObjectData(
                typeof(ISubInterfaceAInfo),
                infoMock,
                parentPropertyName,
                structPath,
                structMock);

            IDataValidatorStruct validator = new TestBaseDataValidatorStruct<IKeyValueStruct>();
            validator.ValidateStruct(null, validationObjectData);

            var errors = validator.Errors;
            Assert.AreEqual(4, errors.Count);

            ValidateError(
                errors[0],
                TestBaseDataValidatorStruct<IKeyValueStruct>.ErrorMessage1,
                true,
                ValidationError.Severity.Error);
            ValidateError(
                errors[1],
                TestBaseDataValidatorStruct<IKeyValueStruct>.ErrorMessage2,
                false,
                ValidationError.Severity.Error);
            ValidateError(
                errors[2],
                TestBaseDataValidatorStruct<IKeyValueStruct>.WarningMessage1,
                true,
                ValidationError.Severity.Warning);
            ValidateError(
                errors[3],
                TestBaseDataValidatorStruct<IKeyValueStruct>.WarningMessage2,
                false,
                ValidationError.Severity.Warning);
        }

        private void ValidateError(
            ValidationError error,
            string errorMessage,
            bool shouldHaveStructProperty,
            ValidationError.Severity severity)
        {
            Assert.AreEqual(typeof(ISubInterfaceAInfo), error.InfoType);
            Assert.AreEqual(infoIdentifier, error.InfoIdentifier);
            Assert.AreEqual(parentPropertyName, error.InfoProperty);
            Assert.AreEqual(structPath, error.StructKeyPath);
            if (shouldHaveStructProperty)
            {
                Assert.AreEqual(TestBaseDataValidatorStruct<IKeyValueStruct>.StructPropertyName, error.StructProperty);
            }
            else
            {
                Assert.IsNull(error.StructProperty);
            }
            Assert.AreEqual(errorMessage, error.Message);
            Assert.AreEqual(severity, error.ErrorSeverity);
        }
    }
}
