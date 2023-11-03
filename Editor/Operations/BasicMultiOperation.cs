using PocketGems.Parameters.Editor.Operation;

namespace PocketGems.Parameters.Operations
{
    public abstract class BasicMultiOperation<T, V> : BasicOperation<T>, IParameterOperation<V>
    {
        public virtual void Execute(V context) => Execute();
    }
}
