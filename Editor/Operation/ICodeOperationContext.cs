namespace PocketGems.Parameters.Editor.Operation
{
    public interface ICodeOperationContext : ICommonOperationContext
    {
        GenerateCodeType GenerateCodeType { get; set; }

        // flat buffer schema file path
        string SchemaFilePath { get; }

        // generated code directories
        string GeneratedCodeDir { get; }
        string GeneratedCodeScriptableObjectsDir { get; }
        string GeneratedCodeStructsDir { get; }
        string GeneratedCodeFlatBufferClassesDir { get; }
        string GeneratedCodeFlatBufferStructsDir { get; }
        string GeneratedCodeFlatBufferBuilderDir { get; }
        string GeneratedCodeCSVBridgeDir { get; }
        string GeneratedCodeValidatorsDir { get; }
    }
}
