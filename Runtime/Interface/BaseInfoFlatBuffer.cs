namespace PocketGems.Parameters.Interface
{
    public abstract class BaseInfoFlatBuffer : IMutableParameter
    {
        protected readonly IParameterManager _parameterManager;
        protected ParameterOverrides _parameterOverrides;

        protected BaseInfoFlatBuffer(IParameterManager parameterManager)
        {
            _parameterManager = parameterManager;
        }

        public abstract bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error);

        protected bool TryGetOverride<T>(int index, out T parameterOverride)
        {
            if (_parameterOverrides == null)
            {
                parameterOverride = default;
                return false;
            }

            if (!_parameterOverrides.TryGetOverride(index, out var obj))
            {
                parameterOverride = default;
                return false;
            }

            parameterOverride = (T)obj;
            return true;
        }

        protected bool TrySetOverride<T>(int index, T parameterOverride, int maxIndex, out string error)
        {
            if (_parameterOverrides == null)
                _parameterOverrides = new();

            return _parameterOverrides.SetEditedProperty(index, parameterOverride, maxIndex, out error);
        }

        public void RemoveAllEdits()
        {
            _parameterOverrides = null;
        }

        public abstract IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager);
    }
}
