using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Validation;

namespace PocketGems.Parameters.Operations
{
    public class OperationErrorTest
    {
        [Test]
        public void GeneralError()
        {
            var e = new OperationError("test");
            Assert.AreEqual(OperationError.ErrorType.General, e.Type);
            Assert.AreEqual("test", e.Message);
            Assert.IsNull(e.ValidationError);
        }

        [Test]
        public void ValidationError()
        {
            var vError = new ValidationError(null, null, null, null);
            var e = new OperationError(vError);
            Assert.AreEqual(OperationError.ErrorType.Validation, e.Type);
            Assert.IsNull(e.Message);
            Assert.AreEqual(vError, e.ValidationError);
        }
    }
}
