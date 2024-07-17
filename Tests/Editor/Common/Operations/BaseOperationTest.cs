using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Parameters;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Editor;

namespace PocketGems.Parameters.Common.Operations.Editor
{
    /// <summary>
    /// Abstract class for operation tests to provide convenience functionality for testing.
    /// </summary>
    public abstract class BaseOperationTest<T> where T : class
    {
        protected T _contextMock;

        protected IParameterInfo _mockParameterInfo1;
        protected IParameterInfo _mockParameterInfo2;
        protected IParameterInfo _mockParameterInfo3;
        protected IParameterStruct _mockParameterStruct1;
        protected IParameterStruct _mockParameterStruct2;
        protected List<IParameterInfo> _mockParameterInfos;
        protected List<IParameterStruct> _mockParameterStructs;

        [SetUp]
        public virtual void SetUp()
        {
            CSVBridge.Reset();

            _contextMock = Substitute.For<T>();

            _mockParameterInfo1 = MockedInterfaces.ParameterInfo("CurrencyInfo");
            _mockParameterInfo2 = MockedInterfaces.ParameterInfo("BuildingInfo");
            _mockParameterInfo3 = MockedInterfaces.ParameterInfo("DragonInfo");
            _mockParameterStruct1 = MockedInterfaces.ParameterStruct("RewardStruct");
            _mockParameterStruct2 = MockedInterfaces.ParameterStruct("KeyValueStruct");
            _mockParameterInfos = new List<IParameterInfo> { _mockParameterInfo1, _mockParameterInfo2, _mockParameterInfo3 };
            _mockParameterStructs = new List<IParameterStruct> { _mockParameterStruct1, _mockParameterStruct2 };
        }

        protected void AssertExecute(IParameterOperation<T> operation, OperationState expectedState)
        {
            operation.Execute(_contextMock);

            Assert.AreEqual(expectedState, operation.OperationState);
            if (expectedState == OperationState.Error)
                Assert.Greater(operation.Errors.Count, 0);
            else
                Assert.True(operation.Errors == null || operation.Errors.Count == 0);

            if (expectedState == OperationState.Canceled)
                Assert.IsNotNull(operation.CancelMessage);
            else
                Assert.IsNull(operation.CancelMessage);
        }
    }
}
