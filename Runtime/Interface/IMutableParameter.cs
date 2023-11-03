namespace PocketGems.Parameters.Interface
{
    public interface IMutableParameter
    {
        bool EditProperty(string propertyName, string value, out string error);
        void RemoveAllEdits();
    }
}
