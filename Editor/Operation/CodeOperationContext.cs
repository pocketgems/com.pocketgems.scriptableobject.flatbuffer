namespace PocketGems.Parameters.Editor.Operation
{
    public class CodeOperationContext : CommonOperationContext, ICodeOperationContext
    {
        public GenerateCodeType GenerateCodeType { get; set; }
        public string SchemaFilePath => EditorParameterConstants.GeneratedCode.SchemaFilePath;
        public string GeneratedCodeDir => EditorParameterConstants.GeneratedCode.RootDir;
        public string GeneratedCodeScriptableObjectsDir => EditorParameterConstants.GeneratedCode.ScriptableObjectsDir;
        public string GeneratedCodeStructsDir => EditorParameterConstants.GeneratedCode.StructsDir;
        public string GeneratedCodeFlatBufferClassesDir => EditorParameterConstants.GeneratedCode.FlatBufferClassesDir;
        public string GeneratedCodeFlatBufferStructsDir => EditorParameterConstants.GeneratedCode.FlatBufferStructsDir;
        public string GeneratedCodeFlatBufferBuilderDir => EditorParameterConstants.GeneratedCode.FlatBufferBuilderDir;
        public string GeneratedCodeCSVBridgeDir => EditorParameterConstants.GeneratedCode.CSVBridgeDir;
        public string GeneratedCodeValidatorsDir => EditorParameterConstants.GeneratedCode.ValidatorsDir;
    }
}
