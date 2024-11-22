using NSubstitute;
using NUnit.Framework;

namespace PocketGems.Parameters.Validation
{
    public class BaseDataValidatorTest
    {
        [Test]
        public void ValidateInfo()
        {
            const string infoIdentifier = "some id";

            var mock = Substitute.For<IMySpecialInfo>();
            mock.Identifier.Returns(infoIdentifier);
            IDataValidator validator = new TestBaseDataValidator<IMySpecialInfo>();
            validator.ValidateInfo(null, mock);

            var errors = validator.Errors;
            Assert.AreEqual(2, errors.Count);

            var error = errors[0];
            Assert.AreEqual(typeof(IMySpecialInfo), error.InfoType);
            Assert.AreEqual(infoIdentifier, error.InfoIdentifier);
            Assert.AreEqual(ValidationError.Severity.Error, error.ErrorSeverity);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.PropertyName, error.InfoProperty);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage1, error.Message);

            var warning = errors[1];
            Assert.AreEqual(typeof(IMySpecialInfo), warning.InfoType);
            Assert.AreEqual(infoIdentifier, warning.InfoIdentifier);
            Assert.AreEqual(ValidationError.Severity.Warning, warning.ErrorSeverity);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.PropertyName, warning.InfoProperty);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.WarningMessage1, warning.Message);
        }

        [Test]
        public void ValidateParameters()
        {
            IDataValidator validator = new TestBaseDataValidator<IMySpecialInfo>();
            validator.ValidateParameters(null);

            var errors = validator.Errors;
            Assert.AreEqual(2, errors.Count);

            var error = errors[0];
            Assert.AreEqual(typeof(IMySpecialInfo), error.InfoType);
            Assert.IsNull(error.InfoIdentifier);
            Assert.IsNull(error.InfoProperty);
            Assert.AreEqual(ValidationError.Severity.Error, error.ErrorSeverity);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage2, error.Message);

            var warning = errors[1];
            Assert.AreEqual(typeof(IMySpecialInfo), warning.InfoType);
            Assert.IsNull(warning.InfoIdentifier);
            Assert.IsNull(warning.InfoProperty);
            Assert.AreEqual(ValidationError.Severity.Warning, warning.ErrorSeverity);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.WarningMessage2, warning.Message);
        }
    }
}
