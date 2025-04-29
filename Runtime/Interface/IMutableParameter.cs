namespace PocketGems.Parameters.Interface
{
    public interface IMutableParameter
    {
        bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error);
        void RemoveAllEdits();
    }
}
