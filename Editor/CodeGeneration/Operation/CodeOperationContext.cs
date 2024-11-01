using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Operation.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operation.Editor
{
    public class CodeOperationContext : CommonOperationContext, ICodeOperationContext
    {
        public GenerateCodeType GenerateCodeType { get; set; }
        public string SchemaFilePath => EditorParameterConstants.CodeGeneration.SchemaFilePath;
        public string GeneratedCodeDir => EditorParameterConstants.CodeGeneration.RootDir;
        public string GeneratedCodeScriptableObjectsDir => EditorParameterConstants.CodeGeneration.ScriptableObjectsDir;
        public string GeneratedCodeStructsDir => EditorParameterConstants.CodeGeneration.StructsDir;
        public string GeneratedCodeFlatBufferClassesDir => EditorParameterConstants.CodeGeneration.FlatBufferClassesDir;
        public string GeneratedCodeFlatBufferStructsDir => EditorParameterConstants.CodeGeneration.FlatBufferStructsDir;
        public string GeneratedCodeFlatBufferBuilderDir => EditorParameterConstants.CodeGeneration.FlatBufferBuilderDir;
        public string GeneratedCodeCSVBridgeDir => EditorParameterConstants.CodeGeneration.CSVBridgeDir;
        public string GeneratedCodeValidatorsDir => EditorParameterConstants.CodeGeneration.ValidatorsDir;
    }
}
