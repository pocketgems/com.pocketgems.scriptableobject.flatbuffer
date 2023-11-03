using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Operations.Code
{
    public class ValidateTypesOperationTest : BaseOperationTest<ICodeOperationContext>
    {
        private ValidateTypesOperation _operation;
        private List<IParameterInfo> _parameterInfos;
        private List<IParameterStruct> _parameterStructs;
        private List<IParameterEnum> _parameterEnums;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _parameterInfos = new List<IParameterInfo>();
            _parameterStructs = new List<IParameterStruct>();
            _parameterEnums = new List<IParameterEnum>();
            _contextMock.ParameterInfos.ReturnsForAnyArgs(_parameterInfos);
            _contextMock.ParameterStructs.ReturnsForAnyArgs(_parameterStructs);
            _contextMock.ParameterEnums.ReturnsForAnyArgs(_parameterEnums);

            _operation = new ValidateTypesOperation();
        }

        [Test]
        public void Success()
        {
            var parameterInfoMock = Substitute.For<IParameterInfo>();
            parameterInfoMock.Validate(out _).ReturnsForAnyArgs(true);
            _parameterInfos.Add(parameterInfoMock);

            var parameterStructMock = Substitute.For<IParameterStruct>();
            parameterStructMock.Validate(out _).ReturnsForAnyArgs(true);
            _parameterStructs.Add(parameterStructMock);

            var parameterEnumMock = Substitute.For<IParameterEnum>();
            parameterEnumMock.Validate(out _).ReturnsForAnyArgs(true);
            _parameterEnums.Add(parameterEnumMock);

            AssertExecute(_operation, OperationState.Finished);

            parameterInfoMock.ReceivedWithAnyArgs(1).Validate(out _);
            parameterStructMock.ReceivedWithAnyArgs(1).Validate(out _);
            parameterEnumMock.ReceivedWithAnyArgs(1).Validate(out _);
        }

        [Test]
        public void SuccessNoOp()
        {
            AssertExecute(_operation, OperationState.Finished);
        }

        [Test]
        public void InfoError()
        {
            var parameterInfoMock = Substitute.For<IParameterInfo>();
            parameterInfoMock.Validate(out IReadOnlyList<string> errors).Returns(x =>
            {
                var errorList = new List<string>();
                errors = errorList;
                errorList.Add("error1");
                errorList.Add("error2");
                errorList.Add("error3");
                x[0] = errors;
                return false;
            });
            _parameterInfos.Add(parameterInfoMock);

            AssertExecute(_operation, OperationState.Error);
            Assert.AreEqual(3, _operation.Errors.Count);
            parameterInfoMock.ReceivedWithAnyArgs(1).Validate(out _);
        }

        [Test]
        public void StructError()
        {
            var parameterStructMock = Substitute.For<IParameterStruct>();
            parameterStructMock.Validate(out IReadOnlyList<string> errors).Returns(x =>
            {
                var errorList = new List<string>();
                errors = errorList;
                errorList.Add("error1");
                errorList.Add("error2");
                x[0] = errors;
                return false;
            });
            _parameterStructs.Add(parameterStructMock);

            AssertExecute(_operation, OperationState.Error);
            Assert.AreEqual(2, _operation.Errors.Count);
            parameterStructMock.ReceivedWithAnyArgs(1).Validate(out _);
        }

        [Test]
        public void EnumError()
        {
            var parameterEnumMock = Substitute.For<IParameterEnum>();
            parameterEnumMock.Validate(out List<string> errors).Returns(x =>
            {
                errors = new List<string>();
                errors.Add("error1");
                errors.Add("error2");
                x[0] = errors;
                return false;
            });
            _parameterEnums.Add(parameterEnumMock);

            AssertExecute(_operation, OperationState.Error);
            Assert.AreEqual(2, _operation.Errors.Count);
            parameterEnumMock.ReceivedWithAnyArgs(1).Validate(out _);
        }
    }
}
