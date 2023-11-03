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
            Assert.AreEqual(1, errors.Count);
            var error = errors[0];
            Assert.AreEqual(typeof(IMySpecialInfo), error.InfoType);
            Assert.AreEqual(infoIdentifier, error.InfoIdentifier);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.PropertyName, error.InfoProperty);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage1, error.Message);
        }

        [Test]
        public void ValidateParameters()
        {
            IDataValidator validator = new TestBaseDataValidator<IMySpecialInfo>();
            validator.ValidateParameters(null);

            var errors = validator.Errors;
            Assert.AreEqual(1, errors.Count);
            var error = errors[0];
            Assert.AreEqual(typeof(IMySpecialInfo), error.InfoType);
            Assert.IsNull(error.InfoIdentifier);
            Assert.IsNull(error.InfoProperty);
            Assert.AreEqual(TestBaseDataValidator<IMySpecialInfo>.ErrorMessage2, error.Message);
        }
    }
}
