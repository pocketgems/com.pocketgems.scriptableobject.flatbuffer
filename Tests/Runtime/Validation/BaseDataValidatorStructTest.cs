using NSubstitute;
using NUnit.Framework;

namespace PocketGems.Parameters.Validation
{
    public class BaseDataValidatorStructTest
    {
        [Test]
        public void ValidateStruct()
        {
            const string infoIdentifier = "some id";
            const string parentPropertyName = "parent property name";
            const string structPath = "some path";

            var infoMock = Substitute.For<IMySpecialInfo>();
            infoMock.Identifier.Returns(infoIdentifier);

            var structMock = Substitute.For<IKeyValueStruct>();

            var validationObjectData = new ValidationObjectData(
                typeof(IMySpecialInfo),
                infoMock,
                parentPropertyName,
                structPath,
                structMock);

            IDataValidatorStruct validator = new TestBaseDataValidatorStruct<IKeyValueStruct>();
            validator.ValidateStruct(null, validationObjectData);

            var errors = validator.Errors;
            Assert.AreEqual(2, errors.Count);
            var error1 = errors[0];
            Assert.AreEqual(typeof(IMySpecialInfo), error1.InfoType);
            Assert.AreEqual(infoIdentifier, error1.InfoIdentifier);
            Assert.AreEqual(parentPropertyName, error1.InfoProperty);
            Assert.AreEqual(structPath, error1.StructKeyPath);
            Assert.AreEqual(TestBaseDataValidatorStruct<IKeyValueStruct>.StructPropertyName,
                error1.StructProperty);
            Assert.AreEqual(TestBaseDataValidatorStruct<IKeyValueStruct>.ErrorMessage1,
                error1.Message);

            var error2 = errors[1];
            Assert.AreEqual(typeof(IMySpecialInfo), error2.InfoType);
            Assert.AreEqual(infoIdentifier, error2.InfoIdentifier);
            Assert.AreEqual(parentPropertyName, error2.InfoProperty);
            Assert.AreEqual(structPath, error2.StructKeyPath);
            Assert.IsNull(error2.StructProperty);
            Assert.AreEqual(TestBaseDataValidatorStruct<IKeyValueStruct>.ErrorMessage2,
                error2.Message);
        }
    }
}
