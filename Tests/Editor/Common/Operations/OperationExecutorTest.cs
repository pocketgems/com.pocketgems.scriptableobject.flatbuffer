using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Operation.Editor;

namespace PocketGems.Parameters.Common.Operations.Editor
{
    public class OperationExecutorTest
    {
        public class TestContext
        {

        }

        private IParameterOperation<TestContext> MockOperation(OperationState state)
        {
            var mock = Substitute.For<IParameterOperation<TestContext>>();
            mock.OperationState.ReturnsForAnyArgs(state);
            return mock;
        }

        [Test]
        public void Success()
        {
            var lastOperation = MockOperation(OperationState.Finished);
            var executor = new OperationExecutor<TestContext>();
            var context = new TestContext();
            executor.ExecuteOperations(new List<IParameterOperation<TestContext>>() {
                MockOperation(OperationState.Finished),
                MockOperation(OperationState.Finished),
                lastOperation
            }, context);
            Assert.AreEqual(ExecutorState.Finished, executor.ExecutorState);
            Assert.AreEqual(lastOperation, executor.LastOperation);
            Assert.GreaterOrEqual(executor.ExecuteMilliseconds, 0);
        }

        [TestCase(OperationState.ShortCircuit, ExecutorState.Finished)]
        [TestCase(OperationState.Canceled, ExecutorState.Canceled)]
        [TestCase(OperationState.Ready, ExecutorState.StateError)]
        [TestCase(OperationState.Error, ExecutorState.Error)]
        public void States(OperationState operationState, ExecutorState executorState)
        {
            var lastOperation = MockOperation(operationState);
            var executor = new OperationExecutor<TestContext>();
            var context = new TestContext();
            executor.ExecuteOperations(
                new List<IParameterOperation<TestContext>>()
                {
                    MockOperation(OperationState.Finished),
                    lastOperation,
                    MockOperation(OperationState.Finished)
                }, context);
            Assert.AreEqual(executorState, executor.ExecutorState);
            Assert.AreEqual(lastOperation, executor.LastOperation);
            Assert.GreaterOrEqual(executor.ExecuteMilliseconds, 0);
        }
    }
}
