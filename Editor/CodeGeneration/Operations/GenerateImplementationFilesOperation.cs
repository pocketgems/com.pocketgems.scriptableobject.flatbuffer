using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.CodeGeneration.Util.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    /// <summary>
    /// Generate the implementation files that implement the interfaces
    /// </summary>
    public class GenerateImplementationFilesOperation : BasicOperation<ICodeOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            var scriptableObjectDir = context.GeneratedCodeScriptableObjectsDir;
            var structsDir = context.GeneratedCodeStructsDir;
            var flatBufferClassesDir = context.GeneratedCodeFlatBufferClassesDir;
            var soFileRemover = new UnusedFileRemover(scriptableObjectDir);
            var structFileRemover = new UnusedFileRemover(structsDir);
            var fbFileRemover = new UnusedFileRemover(flatBufferClassesDir);

            // generate files
            foreach (var parameterInfo in context.ParameterInfos)
            {
                var soFilename = CodeGenerator.GenerateScriptableObjectFile(parameterInfo, scriptableObjectDir);
                soFileRemover.UsedFile(soFilename);

                var fbClassFile = CodeGenerator.GenerateFlatBufferClassFile(parameterInfo, true, flatBufferClassesDir);
                fbFileRemover.UsedFile(fbClassFile);
            }

            var menuItemFilename = CodeGenerator.GenerateScriptableObjectMenuItems(context.ParameterInfos, scriptableObjectDir);
            soFileRemover.UsedFile(menuItemFilename);

            var parameterStructs = context.ParameterStructs;
            for (int i = 0; i < parameterStructs.Count; i++)
            {
                var parameterStruct = parameterStructs[i];
                var structFilename = CodeGenerator.GenerateStructFile(parameterStruct, structsDir);
                structFileRemover.UsedFile(structFilename);

                var fbClassFile = CodeGenerator.GenerateFlatBufferClassFile(parameterStruct, false, flatBufferClassesDir);
                fbFileRemover.UsedFile(fbClassFile);
            }

            ParameterDebug.Log($"Generated source files in {scriptableObjectDir}");
            ParameterDebug.Log($"Generated source files in {structsDir}");
            ParameterDebug.Log($"Generated source files in {flatBufferClassesDir}");

            // remove old files afterwards - this is to preserve the GUID of existing files and not break SO instances.
            // alternatively we could've deleted the directory first but this doesn't preserve the GUIDs
            soFileRemover.RemoveUnusedFiles();
            structFileRemover.RemoveUnusedFiles();
            fbFileRemover.RemoveUnusedFiles();
        }
    }
}
