using NSubstitute;
using NUnit.Framework;

namespace PocketGems.Parameters.Validation
{
    public class BaseDataValidatorTest
    {
        private IMySpecialInfo _mockInfo;
        private IParameterManager _mockParameterManager;

        private IDataValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _mockInfo = Substitute.For<IMySpecialInfo>();
            _mockInfo.Identifier.Returns(TestBaseDataValidator<IMySpecialInfo>.InfoIdentifier);

            _mockParameterManager = Substitute.For<IMutableParameterManager>();
            _mockParameterManager.Get<IMySpecialInfo>(_mockInfo.Identifier).Returns(_mockInfo);

            _validator = new TestBaseDataValidator<IMySpecialInfo>();
        }

        [Test]
        public void ValidateInfo()
        {
            _validator.ValidateInfo(_mockParameterManager, _mockInfo);

            var errors = _validator.Errors;
            Assert.That(errors.Count, Is.EqualTo(2));

            var error = errors[0];
            Assert.That(error.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(error.InfoIdentifier, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.InfoIdentifier));
            Assert.That(error.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Error));
            Assert.That(error.InfoProperty, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.PropertyName));
            Assert.That(error.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage1));

            var warning = errors[1];
            Assert.That(warning.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(warning.InfoIdentifier, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.InfoIdentifier));
            Assert.That(warning.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Warning));
            Assert.That(warning.InfoProperty, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.PropertyName));
            Assert.That(warning.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.WarningMessage1));
        }

        [Test]
        public void ValidateParameters()
        {
            IDataValidator validator = new TestBaseDataValidator<IMySpecialInfo>();
            validator.ValidateParameters(_mockParameterManager);

            var errors = validator.Errors;
            Assert.That(errors.Count, Is.EqualTo(6));

            var error1 = errors[0];
            Assert.That(error1.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(error1.InfoIdentifier, Is.Null);
            Assert.That(error1.InfoProperty, Is.Null);
            Assert.That(error1.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Error));
            Assert.That(error1.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage2));

            var error2 = errors[1];
            Assert.That(error2.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(error2.InfoIdentifier, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.InfoIdentifier));
            Assert.That(error2.InfoProperty, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.PropertyName));
            Assert.That(error2.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Error));
            Assert.That(error2.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage3));

            var error3 = errors[2];
            Assert.That(error3.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(error3.InfoIdentifier, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.InfoIdentifier));
            Assert.That(error3.InfoProperty, Is.Null);
            Assert.That(error3.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Error));
            Assert.That(error3.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage3));

            var warning1 = errors[3];
            Assert.That(warning1.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(warning1.InfoIdentifier, Is.Null);
            Assert.That(warning1.InfoProperty, Is.Null);
            Assert.That(warning1.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Warning));
            Assert.That(warning1.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.WarningMessage2));

            var warning2 = errors[4];
            Assert.That(warning2.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(warning2.InfoIdentifier, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.InfoIdentifier));
            Assert.That(warning2.InfoProperty, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.PropertyName));
            Assert.That(warning2.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Warning));
            Assert.That(warning2.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.WarningMessage3));

            var warning3 = errors[5];
            Assert.That(warning3.InfoType, Is.EqualTo(typeof(IMySpecialInfo)));
            Assert.That(warning3.InfoIdentifier, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.InfoIdentifier));
            Assert.That(warning3.InfoProperty, Is.Null);
            Assert.That(warning3.ErrorSeverity, Is.EqualTo(ValidationError.Severity.Warning));
            Assert.That(warning3.Message, Is.EqualTo(TestBaseDataValidator<IMySpecialInfo>.WarningMessage3));
        }
    }
}
