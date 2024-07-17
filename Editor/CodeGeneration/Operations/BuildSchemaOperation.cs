using System.IO;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEditor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    /// <summary>
    /// This operation uses the parameter files to build the flatbuffer schema file which is used with the faltbuffer
    /// binary during source code generation.
    /// </summary>
    internal class BuildSchemaOperation : BasicOperation<ICodeOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            var containerTableName = ParameterConstants.CollectionTypeName;
            var schemaGenerator = new SchemaBuilder(containerTableName);
            schemaGenerator.DefineField(containerTableName, EditorParameterConstants.DataLoaderClass.HashFieldName.ToSnakeCase(), FlatBufferFieldType.String);

            EditorUtility.DisplayProgressBar("Generating Parameter Code", "Building Schema", 100);

            void DefineSchema(IParameterInterface parameterInterface)
            {
                // define FlatBuffer fields for each property
                var structName = parameterInterface.FlatBufferStructName(false);
                var guidFieldName = EditorParameterConstants.FlatBufferBuilderClass.FlatBufferGUIDFieldName;
                schemaGenerator.DefineField(structName, guidFieldName, FlatBufferFieldType.String);

                for (int j = 0; j < parameterInterface.PropertyTypes.Count; j++)
                {
                    var propertyType = parameterInterface.PropertyTypes[j];
                    propertyType.DefineFlatBufferSchema(schemaGenerator, structName);
                }

                // define the properties for the container to hold the data for each type
                schemaGenerator.DefineArrayField(containerTableName, structName.ToSnakeCase(), structName);
            }

            for (int i = 0; i < context.ParameterInfos.Count; i++)
                DefineSchema(context.ParameterInfos[i]);
            for (int i = 0; i < context.ParameterStructs.Count; i++)
                DefineSchema(context.ParameterStructs[i]);

            // generate file contents & write to file
            var schemaFilePath = context.SchemaFilePath;
            string schemaString = schemaGenerator.GenerateSchemaContent();
            File.WriteAllText(schemaFilePath, schemaString);

            EditorUtility.ClearProgressBar();

            var relativeFilePath = NamingUtil.RelativePath(schemaFilePath);
            ParameterDebug.LogVerbose($"Generated Schema File {relativeFilePath}");
        }
    }
}
