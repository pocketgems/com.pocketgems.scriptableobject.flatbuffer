using System;
using System.Collections.Generic;
using System.Diagnostics;
using PocketGems.Parameters.Util;
using Unity.Profiling;

namespace PocketGems.Parameters.Editor.Operation
{
    internal class OperationExecutor<T>
    {
        public ExecutorState ExecutorState { get; private set; }
        public bool ShortCircuited { get; private set; }
        public long ExecuteMilliseconds { get; private set; }
        public IParameterOperation<T> LastOperation { get; private set; }

        public OperationExecutor()
        {
            ExecutorState = ExecutorState.Ready;
            ExecuteMilliseconds = -1;
            ShortCircuited = false;
        }

        /// <summary>
        /// Executes the IParameterOperation in order and stops when it encounters an error or user cancel event.
        /// </summary>
        /// <param name="operations">Operations to run in order</param>
        /// <param name="context">Context to execute operations with.</param>
        public void ExecuteOperations(List<IParameterOperation<T>> operations, T context)
        {
            var executorMarker = new ProfilerMarker($"Parameters.{typeof(T).Name}.ExecuteOperations");
            executorMarker.Begin();

            ShortCircuited = false;
            ExecutorState = ExecutorState.Running;
            Stopwatch overallStopwatch = Stopwatch.StartNew();
            Stopwatch stopwatch = new Stopwatch();
            for (int i = 0; i < operations.Count; i++)
            {
                var operation = operations[i];
                LastOperation = operation;
                var operationMarker = new ProfilerMarker($"Parameters.{typeof(T).Name}.ExecuteOperations.{operation.GetType().Name}");
                operationMarker.Begin();
                stopwatch.Restart();
                operation.Execute(context);
                stopwatch.Stop();
                operationMarker.End();
                ParameterDebug.LogVerbose($"Operation [{operation.GetType()}] executed in {stopwatch.ElapsedMilliseconds}ms");
                switch (operation.OperationState)
                {
                    case OperationState.Ready:
                        ExecutorState = ExecutorState.StateError;
                        break;
                    case OperationState.Error:
                        ExecutorState = ExecutorState.Error;
                        break;
                    case OperationState.Canceled:
                        ExecutorState = ExecutorState.Canceled;
                        break;
                    case OperationState.Finished:
                        break;
                    case OperationState.ShortCircuit:
                        ExecutorState = ExecutorState.Finished;
                        ShortCircuited = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (ExecutorState != ExecutorState.Running)
                    break;
            }

            if (ExecutorState == ExecutorState.Running)
                ExecutorState = ExecutorState.Finished;

            overallStopwatch.Stop();
            ExecuteMilliseconds = overallStopwatch.ElapsedMilliseconds;

            executorMarker.End();
        }
    }
}
