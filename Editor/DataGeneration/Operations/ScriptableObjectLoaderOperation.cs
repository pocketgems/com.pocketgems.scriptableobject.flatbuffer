using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Rows.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.DataGeneration.Util.Editor;
using PocketGems.Parameters.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    /// <summary>
    /// Operation to load, create or delete Scriptable Objects for the current execution.
    /// </summary>
    [ExcludeFromCoverage] // exclude for now TODO revisit
    public class ScriptableObjectLoaderOperation : BasicOperation<IDataOperationContext>
    {
        // memoize Scriptable Object class to direct IParameterInterface
        private readonly Dictionary<Type, IParameterInfo> _classToInterfaceCache;
        private IDataOperationContext _context;

        public ScriptableObjectLoaderOperation()
        {
            _classToInterfaceCache = new Dictionary<Type, IParameterInfo>();
            _guidToMetaMapping = new Dictionary<IParameterInfo, Dictionary<string, IScriptableObjectMetadata>>();
            _identifierToMetaMapping = new Dictionary<IParameterInfo, Dictionary<string, IScriptableObjectMetadata>>();
        }

        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);
            _context = context;

            if (context.GenerateDataType == GenerateDataType.CSVDiff)
            {
                Dictionary<string, IParameterInfo> baseNameToParameterInfo = new Dictionary<string, IParameterInfo>();
                foreach (var parameterInfo in context.ParameterInfos)
                    baseNameToParameterInfo[parameterInfo.BaseName] = parameterInfo;

                Dictionary<string, IParameterStruct> baseNameToParameterStruct = new Dictionary<string, IParameterStruct>();
                foreach (var parameterStruct in context.ParameterStructs)
                    baseNameToParameterStruct[parameterStruct.BaseName] = parameterStruct;

                foreach (var kvp in context.InfoCSVFileCache.LoadedFiles())
                {
                    var baseName = kvp.Key;
                    var parameterInfo = baseNameToParameterInfo[baseName];
                    AttemptToPopulateContext(context, parameterInfo, kvp.Value);
                }

                foreach (var kvp in context.StructCSVFileCache.LoadedFiles())
                {
                    var baseName = kvp.Key;
                    var parameterStruct = baseNameToParameterStruct[baseName];
                    AttemptToPopulateContext(context, parameterStruct, kvp.Value);
                }
            }

            // fetch only specified files
            if (context.GenerateDataType == GenerateDataType.All)
            {
                Dictionary<Type, HashSet<string>> identifierMappings = new Dictionary<Type, HashSet<string>>();

                void ValidateUniqueIdentifier(Type type, string identifier)
                {
                    // everything is derived from IBaseInfo
                    if (type == typeof(IBaseInfo))
                        return;
                    if (!identifierMappings.TryGetValue(type, out HashSet<string> identifiers))
                    {
                        identifiers = new HashSet<string>();
                        identifierMappings[type] = identifiers;
                    }
                    if (identifiers.Contains(identifier))
                        Error($"Duplicate Scriptable Objects of the same name [{identifier}] type [{type}]");
                    identifiers.Add(identifier);
                }

                // fetch all scriptable objects
                string[] guids = ScriptableObjectUtil.FindAllParameterScriptableObjects();
                for (int i = 0; i < guids.Length; i++)
                {
                    var metadata = LoadMetadata(context, guids[i], null, out IParameterInfo parameterInterface);
                    if (metadata == null)
                        continue;

                    PopulateContext(parameterInterface, metadata);

                    // validate unique identifier
                    var identifier = metadata.ScriptableObject.name;
                    ValidateUniqueIdentifier(parameterInterface.Type, identifier);
                    var baseInterfaces = parameterInterface.OrderedBaseInterfaceTypes;
                    for (int j = 0; j < baseInterfaces.Count; j++)
                        ValidateUniqueIdentifier(baseInterfaces[j], identifier);
                }
            }

            if (context.GenerateDataType == GenerateDataType.ScriptableObjectDiff)
            {
                var filePaths = context.ModifiedScriptableObjectPaths;
                for (int i = 0; i < filePaths.Count; i++)
                {
                    var metadata = LoadMetadata(context, null, filePaths[i], out IParameterInfo parameterInfo);
                    if (metadata != null)
                        PopulateContext(parameterInfo, metadata);
                    else
                        Error($"Unable to load asset for path {filePaths[i]}");
                }
            }
        }

        private ScriptableObjectMetadata LoadMetadata(IDataOperationContext context, string guid, string path, out IParameterInfo parameterInfo)
        {
            parameterInfo = null;

            if (guid == null && path == null)
                return null;

            if (guid == null)
            {
                var unityGUID = AssetDatabase.GUIDFromAssetPath(path);
                if (unityGUID.Empty())
                    return null;
                guid = unityGUID.ToString();
            }

            if (path == null)
            {
                path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == null)
                    return null;
            }

            var obj = AssetDatabase.LoadAssetAtPath<ParameterScriptableObject>(path);
            if (!(obj is IBaseInfo))
                return null;

            // memoize the found interface so that searching for it isn't so expensive for every scriptable object
            var classType = obj.GetType();
            if (!_classToInterfaceCache.TryGetValue(classType, out parameterInfo))
            {
                var allInterfaces = obj.GetType().GetInterfaces();
                var directInterfaces = allInterfaces.Except
                    (allInterfaces.SelectMany(t => t.GetInterfaces())).ToArray();
                if (directInterfaces.Length != 1)
                {
                    Error($"Scriptable Object {classType} should only implement one {typeof(IBaseInfo)} interface.");
                    return null;
                }

                for (int i = 0; i < context.ParameterInfos.Count; i++)
                    if (context.ParameterInfos[i].Type == directInterfaces[0])
                        parameterInfo = context.ParameterInfos[i];
                _classToInterfaceCache[classType] = parameterInfo;
            }

            var metadata = new ScriptableObjectMetadata(guid, path, obj);
            return metadata;
        }

        private void PopulateContext(IParameterInfo parameterInfo, IScriptableObjectMetadata metadata)
        {
            if (!_context.ScriptableObjectMetadatas.TryGetValue(parameterInfo, out List<IScriptableObjectMetadata> objects))
            {
                objects = new List<IScriptableObjectMetadata>();
                _context.ScriptableObjectMetadatas[parameterInfo] = objects;
            }
            objects.Add(metadata);
        }

        private Dictionary<IParameterInfo, Dictionary<string, IScriptableObjectMetadata>> _guidToMetaMapping;
        private Dictionary<IParameterInfo, Dictionary<string, IScriptableObjectMetadata>> _identifierToMetaMapping;

        private void GetMetaMappings(IDataOperationContext context, IParameterInfo parameterInfo,
            out IReadOnlyDictionary<string, IScriptableObjectMetadata> guidToMeta,
            out IReadOnlyDictionary<string, IScriptableObjectMetadata> identifierToMeta)
        {
            if (!_guidToMetaMapping.TryGetValue(parameterInfo, out var mutableGuidToMeta) ||
                !_identifierToMetaMapping.TryGetValue(parameterInfo, out var mutableIdentifierToMeta))
            {
                mutableGuidToMeta = new Dictionary<string, IScriptableObjectMetadata>();
                mutableIdentifierToMeta = new Dictionary<string, IScriptableObjectMetadata>();
                _guidToMetaMapping[parameterInfo] = mutableGuidToMeta;
                _identifierToMetaMapping[parameterInfo] = mutableIdentifierToMeta;

                var guids = ScriptableObjectUtil.FindAllParameterScriptableObjects(
                    parameterInfo.ScriptableObjectClassName(false));
                for (int i = 0; i < guids.Length; i++)
                {
                    var metadata = LoadMetadata(context, guids[i], null, out IParameterInfo _);
                    mutableGuidToMeta[metadata.GUID] = metadata;
                    mutableIdentifierToMeta[metadata.ScriptableObject.name] = metadata;
                }
            }
            guidToMeta = mutableGuidToMeta;
            identifierToMeta = mutableIdentifierToMeta;
        }

        /// <summary>
        /// Attempts to delete, create, or find scriptable objects that need to be modified based on CSV changes.
        ///
        /// This does not update the values within the Scriptable Objects themselves.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parameterInfo"></param>
        /// <param name="csvFile"></param>
        private void AttemptToPopulateContext(IDataOperationContext context, IParameterInfo parameterInfo, CSVFile csvFile)
        {
            // load all relevant scriptable objects for CSV syncing
            GetMetaMappings(context, parameterInfo,
                out IReadOnlyDictionary<string, IScriptableObjectMetadata> guidToMeta,
                out IReadOnlyDictionary<string, IScriptableObjectMetadata> identifierToMeta);

            // Preprocess:
            // - check for duplicate or no identifiers
            // - book keep the rows with duplicate GUIDs
            HashSet<string> identifiersInCSV = new HashSet<string>();
            HashSet<string> guidsInCSV = new HashSet<string>();
            HashSet<string> multipleGUIDsInCSV = new HashSet<string>();
            for (int i = 0; i < csvFile.RowData.Count; i++)
            {
                var rowData = csvFile.RowData[i];
                var identifier = rowData.Identifier;
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    Error($"Error in [{csvFile.FilePath}] csv found row with empty identifier.");
                    continue;
                }
                if (identifiersInCSV.Contains(identifier))
                {
                    Error($"Error in [{csvFile.FilePath}] csv found two identifiers [{identifier}].");
                    continue;
                }
                identifiersInCSV.Add(rowData.Identifier);

                var guid = rowData.GUID;
                if (string.IsNullOrWhiteSpace(guid))
                    continue;
                if (guidsInCSV.Contains(guid))
                    multipleGUIDsInCSV.Add(guid);
                else
                    guidsInCSV.Add(guid);

                if (!guidToMeta.TryGetValue(guid, out _))
                    Error($"Error in [{csvFile.FilePath}] couldn't find asset for GUID [{guid}].");
            }

            if (Errors.Count > 0)
                return;

            // matching rows to scriptable objects
            List<CSVRowData> remainingRowDatas = new List<CSVRowData>(csvFile.RowData);
            HashSet<IScriptableObjectMetadata> matchedMetadatas = new HashSet<IScriptableObjectMetadata>();

            // match rows where id & guid matches the scriptable object
            for (int i = remainingRowDatas.Count - 1; i >= 0; i--)
            {
                var rowData = remainingRowDatas[i];
                var identifier = rowData.Identifier;
                var guid = rowData.GUID;

                if (!identifierToMeta.TryGetValue(identifier, out IScriptableObjectMetadata idMetadata))
                    continue;

                if (string.IsNullOrWhiteSpace(guid) || !guidToMeta.TryGetValue(guid, out IScriptableObjectMetadata guidMetadata))
                    continue;

                if (idMetadata != guidMetadata)
                    continue;

                remainingRowDatas.RemoveAt(i);

                // matched id & GUID
                matchedMetadatas.Add(guidMetadata);
                // ParameterDebug.LogVerbose($"Matched on id [{identifier}] & GUID [{guid}]: {guidMetadata.ScriptableObject}");
                if (!rowData.HashMatches)
                    PopulateContext(parameterInfo, guidMetadata);
            }

            // match rows based on just GUID
            for (int i = remainingRowDatas.Count - 1; i >= 0; i--)
            {
                var rowData = remainingRowDatas[i];
                var guid = rowData.GUID;

                if (string.IsNullOrWhiteSpace(guid))
                    continue;

                // Someone created a new row and didn't change the GUID - create new SO later on
                var metadata = guidToMeta[guid];
                if (matchedMetadatas.Contains(metadata))
                    continue;

                if (multipleGUIDsInCSV.Contains(guid))
                {
                    Error(
                        $"Error in [{csvFile.FilePath}].  Multiple rows with the same guid [{guid}] but the current Scriptable Object [{metadata.ScriptableObject}] doesn't match any of them .");
                    continue;
                }

                remainingRowDatas.RemoveAt(i);

                // MATCHED on GUID
                matchedMetadatas.Add(metadata);
                // ParameterDebug.LogVerbose($"Matched on GUID [{guid}]: {metadata.ScriptableObject}");
                PopulateContext(parameterInfo, metadata);
            }

            if (Errors.Count > 0)
                return;

            // match rows based identifier
            for (int i = remainingRowDatas.Count - 1; i >= 0; i--)
            {
                var rowData = remainingRowDatas[i];
                var identifier = rowData.Identifier;
                if (!identifierToMeta.TryGetValue(identifier, out IScriptableObjectMetadata metadata))
                    continue;
                if (matchedMetadatas.Contains(metadata))
                    continue;

                remainingRowDatas.RemoveAt(i);

                // Matched on Identifier - the GUID field might be outdated in the CSV or empty
                rowData.GUID = metadata.GUID;
                // add _new suffix so it doesn't collide when renaming scriptable objects later
                metadata.Rename($"{rowData.Identifier}_new");
                matchedMetadatas.Add(metadata);
                // ParameterDebug.LogVerbose($"Matched on Identifier [{identifier}]: {metadata.ScriptableObject}");
                PopulateContext(parameterInfo, metadata);
            }

            // create new Scriptable Objects for remaining rows
            string createDirectoryPath = null;
            for (int i = remainingRowDatas.Count - 1; i >= 0; i--)
            {
                if (createDirectoryPath == null)
                {
                    if (identifierToMeta.Count > 0)
                    {
                        // grab a directory where a current Scriptable Object exists.  Create new ones in that directory.
                        createDirectoryPath = Path.GetDirectoryName(identifierToMeta.Values.First().FilePath);
                    }
                    else
                    {
                        // default file path
                        createDirectoryPath = Path.Combine(ParameterConstants.ScriptableObject.Dir,
                            parameterInfo.BaseName);
                        Directory.CreateDirectory(createDirectoryPath);
                    }
                }

                var rowData = remainingRowDatas[i];
                var instance = ScriptableObject.CreateInstance(parameterInfo.ScriptableObjectType());
                // add _new suffix so it doesn't collide when renaming scriptable objects later
                var newPath = Path.Combine(createDirectoryPath, $"{rowData.Identifier}_new.asset");
                AssetDatabase.CreateAsset(instance, newPath);
                var metadata = LoadMetadata(context, null, newPath, out IParameterInfo _);
                PopulateContext(parameterInfo, metadata);
                rowData.GUID = metadata.GUID;
                ParameterDebug.Log($"Created Scriptable Object at [{newPath}] GUID:{metadata.GUID}");
            }

            // delete Scriptable Objects for deleted rows
            foreach (var kvp in identifierToMeta)
            {
                if (matchedMetadatas.Contains(kvp.Value))
                    continue;
                AssetDatabase.DeleteAsset(kvp.Value.FilePath);
                ParameterDebug.Log($"Deleted Scriptable Object at [{kvp.Value.FilePath}]");

                /*
                 * For deleted rows, we need to regenerate the whole FlatBuffer again
                 * after this current run because the old rows will still be in the old FlatBuffer.
                 *
                 * We cannot switch to GenerateAll at this point because we still need to update
                 * the Scriptable Objects from the updated CSV data first.
                 */
                context.GenerateAllAgain = true;
            }
        }

        private void AttemptToPopulateContext(IDataOperationContext context, IParameterStruct parameterStruct, CSVFile csvFile)
        {
            Dictionary<string, IParameterInfo> baseNameToParameterInfo = new Dictionary<string, IParameterInfo>();
            for (int i = 0; i < context.ParameterInfos.Count; i++)
            {
                var parameterInfo = context.ParameterInfos[i];
                baseNameToParameterInfo[parameterInfo.BaseName] = parameterInfo;
            }

            // find modified or new rows & collect type of scriptable objects we need to collect
            HashSet<IScriptableObjectMetadata> _addedMetadatas = new HashSet<IScriptableObjectMetadata>();
            HashSet<string> guidsInCSV = new HashSet<string>();
            for (int i = 0; i < csvFile.RowData.Count; i++)
            {
                var rowData = csvFile.RowData[i];
                var guid = rowData.GUID;
                if (string.IsNullOrWhiteSpace(guid))
                {
                    Error($"Error in [{csvFile.FilePath}] csv found row with empty guid.");
                    continue;
                }
                if (guidsInCSV.Contains(guid))
                {
                    Error($"Error in [{csvFile.FilePath}] multiple guid paths {guid}.");
                    continue;
                }

                guidsInCSV.Add(guid);
                if (rowData.HashMatches)
                    continue;

                // var keyPathBuilder = new StructKeyPathBuilder(guid);
                if (!StructKeyPathBuilder.FetchRootKey(guid,
                        out var rootKeyType, out var rootKeyIdentifier, out var error))
                {
                    Error($"Error in [{csvFile.FilePath}] while parsing KeyPath {guid}: {error}");
                    continue;
                }

                if (!baseNameToParameterInfo.TryGetValue(rootKeyType, out var parameterInfo))
                {
                    Error($"Error in [{csvFile.FilePath}] cannot find parameter info type {rootKeyType}.");
                    continue;
                }

                GetMetaMappings(context, parameterInfo,
                    out IReadOnlyDictionary<string, IScriptableObjectMetadata> _,
                    out IReadOnlyDictionary<string, IScriptableObjectMetadata> identifierToMeta);

                if (!identifierToMeta.TryGetValue(rootKeyIdentifier, out var metadata))
                {
                    Error($"Error in [{csvFile.FilePath}] cannot find parameter {rootKeyType} with identifier {rootKeyIdentifier}.");
                    continue;
                }

                ParameterDebug.LogVerbose($"Matched on GUID [{guid}] & Path [{guid}]: {metadata.ScriptableObject}");
                if (_addedMetadatas.Contains(metadata))
                    continue;

                PopulateContext(parameterInfo, metadata);
                _addedMetadatas.Add(metadata);
            }
        }
    }
}
