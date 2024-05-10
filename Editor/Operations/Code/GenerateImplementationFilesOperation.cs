using System.Linq;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Operations.Code
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
            var orderedInfos = context.ParameterInfos.OrderBy(t => t.BaseName);
            int index = 0;
            foreach (var parameterInfo in orderedInfos)
            {
                var soFilename = CodeGenerator.GenerateScriptableObjectFile(parameterInfo, index, scriptableObjectDir);
                soFileRemover.UsedFile(soFilename);

                var fbClassFile = CodeGenerator.GenerateFlatBufferClassFile(parameterInfo, flatBufferClassesDir);
                fbFileRemover.UsedFile(fbClassFile);
                index++;
            }

            var parameterStructs = context.ParameterStructs;
            for (int i = 0; i < parameterStructs.Count; i++)
            {
                var parameterStruct = parameterStructs[i];
                var structFilename = CodeGenerator.GenerateStructFile(parameterStruct, structsDir);
                structFileRemover.UsedFile(structFilename);

                var fbClassFile = CodeGenerator.GenerateFlatBufferClassFile(parameterStruct, flatBufferClassesDir);
                fbFileRemover.UsedFile(fbClassFile);
            }

            ParameterDebug.Log($"Generated source files in {scriptableObjectDir}");
            ParameterDebug.Log($"Generated source files in {parameterStructs}");
            ParameterDebug.Log($"Generated source files in {flatBufferClassesDir}");

            // remove old files afterwards - this is to preserve the GUID of existing files and not break SO instances.
            // alternatively we could've deleted the directory first but this doesn't preserve the GUIDs
            soFileRemover.RemoveUnusedFiles();
            structFileRemover.RemoveUnusedFiles();
            fbFileRemover.RemoveUnusedFiles();
        }
    }
}
