using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PocketGems.Parameters.Interface;
using UnityEngine;

namespace PocketGems.Parameters
{
    // ApplyOverrides: edits, and the rows that overrides add and delete
    public partial class ParameterManager
    {
        // rows that ApplyOverrides edited, and how many of their properties have an edit
        private readonly Dictionary<IMutableParameter, int> _editCountByParameter = new();

        /*
         * Rows added and deleted by ApplyOverrides.  They belong to this manager only and never change the
         * shared mappings in ParameterManager.cs.  All are null until the first add or delete.
         */
        private Dictionary<string, Dictionary<string, IMutableParameter>> _addedIdentifierMappings;
        private Dictionary<string, Dictionary<string, IMutableParameter>> _addedGuidMappings;
        private HashSet<IMutableParameter> _addedParameters;
        private HashSet<IMutableParameter> _deletedParameters;

        #region IMutableParameterManager

        /// <inheritdoc cref="IMutableParameterManager.ApplyOverrides"/>
        public virtual bool ApplyOverrides(string json, out IReadOnlyList<string> errors)
        {
            if (string.IsNullOrEmpty(json))
            {
                errors = null;
                return true;
            }

            JObject overrides;
            try
            {
                // a repeated key would otherwise replace the earlier one without an error
                overrides = JObject.Parse(json,
                    new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
            }
            catch (JsonReaderException e)
            {
                errors = new[] { $"Overrides json isn't valid: {e.Message}" };
                return false;
            }

            List<string> localErrors = null;
            bool success = true;
            void Error(string error)
            {
                success = false;
                if (localErrors == null)
                    localErrors = new List<string>();
                localErrors.Add(error);
            }

            /*
             * The json's keys in ordinal order.  A server writes the json and can order its keys differently from one
             * call to the next, so reading them in order keeps the order of added rows, and of errors, the same.
             */
            static IEnumerable<JProperty> InOrder(JObject json) =>
                json.Properties().OrderBy(property => property.Name, StringComparer.Ordinal);

            /*
             * The rows of an add or edit: { "csv": { "identifier": { "column": value } } }.  Reports each csv or row
             * that isn't in that shape, and skips it.  A value can be any json value, which is read as text, but not
             * an array or an object.
             */
            IEnumerable<(string csvName, string key, JObject values)> GetRows(string actionName, JObject csvs)
            {
                foreach (var csv in InOrder(csvs))
                {
                    if (!(csv.Value is JObject rows))
                    {
                        Error($"Override {actionName} for csv [{csv.Name}] expects an object of rows.");
                        continue;
                    }

                    foreach (var row in InOrder(rows))
                    {
                        if (!(row.Value is JObject values) || values.Properties().Any(column => !(column.Value is JValue)))
                        {
                            Error($"Override {actionName} for csv [{csv.Name}] row [{row.Name}] expects an object of column values that aren't arrays or objects.");
                            continue;
                        }

                        yield return (csv.Name, row.Name, values);
                    }
                }
            }

            JObject adds = null;
            JObject deletes = null;
            JObject edits = null;
            const string kAddKey = "add";
            const string kDeleteKey = "delete";
            const string kEditKey = "edit";
            foreach (var action in InOrder(overrides))
            {
                var entries = action.Value as JObject;
                if (action.Name != kEditKey && action.Name != kAddKey && action.Name != kDeleteKey)
                    Error($"Override action {action.Name} isn't valid.");
                else if (entries == null)
                    Error($"Override action {action.Name} expects an object keyed by csv name.");
                else if (action.Name == kEditKey)
                    edits = entries;
                else if (action.Name == kAddKey)
                    adds = entries;
                else
                    deletes = entries;
            }

            // changes made by this call, undone if any of the overrides fail
            List<IMutableParameter> deletedParameters = new();
            List<ICreatedParameter> addedParameters = new();
            List<(string csvName, string identifierOrGuid, string propertyName, string interfaceType)> successfulOverrides = new();

            // deletes, then adds, then edits.  A row can't be both deleted and added: edit it instead.
            if (deletes != null)
            {
                foreach (var csv in InOrder(deletes))
                {
                    if (!(csv.Value is JArray keys) || keys.Any(token => token.Type != JTokenType.String))
                    {
                        Error($"Override delete for csv [{csv.Name}] expects an array of identifiers, guids or key paths.");
                        continue;
                    }

                    foreach (var key in keys.Select(token => (string)token).OrderBy(key => key, StringComparer.Ordinal))
                    {
                        if (ApplyDeleteOverride(csv.Name, key, out IMutableParameter deletedParameter, out string error))
                            deletedParameters.Add(deletedParameter);
                        else
                            Error(error);
                    }
                }
            }

            if (adds != null)
            {
                bool reportedMissingFactory = false;
                foreach (var (csvName, identifierOrKeyPath, values) in GetRows(kAddKey, adds))
                {
                    // loaded data can't have an empty identifier or key path, so an add can't either
                    if (identifierOrKeyPath.Length == 0)
                    {
                        Error($"Cannot add parameter for csv [{csvName}]: the identifier or key path is empty.");
                        continue;
                    }

                    // without a factory no add can apply, so say so once rather than for every add
                    if (ParameterFactory == null)
                    {
                        if (!reportedMissingFactory)
                            Error("No parameter factory set. Regenerate parameter code.");
                        reportedMissingFactory = true;
                        continue;
                    }

                    if (ApplyAddOverride(csvName, identifierOrKeyPath, values, out ICreatedParameter addedParameter, out string error))
                        addedParameters.Add(addedParameter);
                    else
                        Error(error);
                }
            }

            if (edits != null)
            {
                foreach (var (csvName, identifierOrGuid, values) in GetRows(kEditKey, edits))
                {
                    var interfaceType = LocalCSV.CSVUtil.CSVToInterfaceFileName(csvName);
                    foreach (var property in InOrder(values))
                    {
                        var propertyName = property.Name;
                        var value = (string)property.Value;

                        // apply all overrides even if an error occurs so we surface all errors for developers rather than one by one
                        if (!ApplyEditOverride(csvName, interfaceType, identifierOrGuid, propertyName, value, out string error))
                            Error(error);
                        else
                            successfulOverrides.Add((csvName, identifierOrGuid, propertyName, interfaceType));
                    }
                }
            }

            if (!success)
            {
                // revert changes if any of the overrides failed: edits, then adds, then deletes.
                foreach (var (csvName, identifierOrGuid, propertyName, interfaceType) in successfulOverrides)
                {
                    if (!RemoveEditOverride(csvName, interfaceType, identifierOrGuid, propertyName, out string error))
                        Error(error);
                }

                for (int i = addedParameters.Count - 1; i >= 0; i--)
                    RemoveAddOverride(addedParameters[i]);

                foreach (var deletedParameter in deletedParameters)
                    RemoveDeleteOverride(deletedParameter);
            }

            errors = localErrors;
            return success;
        }

        /// <inheritdoc cref="IMutableParameterManager.ClearAllOverrides"/>
        public virtual void ClearAllOverrides()
        {
            foreach (var info in _editCountByParameter.Keys)
                info.RemoveAllEdits();
            _editCountByParameter.Clear();

            _addedIdentifierMappings = null;
            _addedGuidMappings = null;
            _addedParameters = null;
            _deletedParameters = null;
        }

        #endregion

        #region Lookups with overrides

        /*
         * The rows this manager holds: the loaded rows from the GetLoaded methods, minus the ones overrides
         * deleted, plus the ones they added.  The Get methods use these.
         */

        private IMutableParameter GetCurrent(string type, string identifier)
        {
            if (TryGetAdded(_addedIdentifierMappings, type, identifier, out IMutableParameter added))
                return added;
            var parameter = GetLoaded(type, identifier);
            if (parameter == null || IsDeletedByOverride(parameter))
                return null;
            return parameter;
        }

        internal IMutableParameter GetCurrentWithGUID(string typeName, string guid, bool errorForMissing)
        {
            if (TryGetAdded(_addedGuidMappings, typeName, guid, out IMutableParameter added))
                return added;
            var parameter = GetLoadedWithGUID(typeName, guid);
            if (parameter != null && !IsDeletedByOverride(parameter))
                return parameter;
            if (errorForMissing)
                LogMissing(typeName, guid, parameter != null);
            return null;
        }

        private IMutableParameter GetCurrentStructWithGUID(string typeName, string guid, bool errorForMissing)
        {
            /*
             * The key as given first, then as the guid of a key path, so a guid, or the key path of an added struct
             * (registered under both), never pays for the hash.  An added row can't share a key with a loaded one, so
             * the order doesn't change what's found.
             */
            var parameter = GetCurrentWithGUID(typeName, guid, false) ??
                            (guid != null ? GetCurrentWithGUID(typeName, _guidConverter.ToGuid(guid), false) : null);
            if (parameter == null && errorForMissing)
                LogMissing(typeName, guid, false);
            return parameter;
        }

        /*
         * Every (identifier, row) of a type: the loaded rows that aren't deleted, then the added rows.  An added
         * row never shares an identifier with a loaded row and can't be deleted.  It loops over the dictionaries
         * directly, so their enumerators aren't boxed.
         */
        private IEnumerable<KeyValuePair<string, IMutableParameter>> GetCurrent(string type)
        {
            if (_identifierMappings.TryGetValue(type, out Dictionary<string, IMutableParameter> loadedParameters))
            {
                foreach (var kvp in loadedParameters)
                {
                    if (!IsDeletedByOverride(kvp.Value))
                        yield return kvp;
                }
            }

            if (_addedIdentifierMappings != null &&
                _addedIdentifierMappings.TryGetValue(type, out Dictionary<string, IMutableParameter> addedParameters))
            {
                foreach (var kvp in addedParameters)
                    yield return kvp;
            }
        }

        // the row an override names, added rows included: by identifier, then guid, then struct key path
        protected IMutableParameter GetCurrentForOverride(string interfaceType, string identifierOrKeyPathOrGuid) =>
            GetCurrent(interfaceType, identifierOrKeyPathOrGuid) ??
            GetCurrentWithGUID(interfaceType, identifierOrKeyPathOrGuid, false) ??
            GetCurrentStructWithGUID(interfaceType, identifierOrKeyPathOrGuid, false);

        private void LogMissing(string typeName, string guid, bool deletedByOverride) =>
            Debug.LogError(deletedByOverride
                ? $"Cannot find {typeName} with GUID {guid}: an override deleted it"
                : _guidMappings.ContainsKey(typeName)
                    ? $"Cannot find {typeName} with GUID {guid}: the row was removed, or its data isn't loaded"
                    : $"Cannot find {typeName} with GUID {guid}: no {typeName} data is loaded");

        #endregion

        #region Edit overrides

        protected virtual bool ApplyEditOverride(string csvName, string interfaceType, string identifierOrKeyPathOrGuid, string propertyName, string value, out string error)
        {
            var mutableParameter = GetCurrentForOverride(interfaceType, identifierOrKeyPathOrGuid);
            if (mutableParameter == null)
            {
                error = EditTargetMissingError(csvName, interfaceType, identifierOrKeyPathOrGuid, propertyName);
                return false;
            }

            if (IsAddedByOverride(mutableParameter))
            {
                error = EditAddedRowError(interfaceType, identifierOrKeyPathOrGuid, propertyName);
                return false;
            }

            if (!mutableParameter.EditProperty(this, propertyName, value, out error))
            {
                error = $"Error editing ({interfaceType})[{identifierOrKeyPathOrGuid}] property [{propertyName}] with value [{value}]: {error}";
                return false;
            }

            _editCountByParameter.TryGetValue(mutableParameter, out int count);
            _editCountByParameter[mutableParameter] = count + 1;
            return true;
        }

        /// <summary>
        /// The error for an edit that finds no row.  It names a row deleted by overrides, since callers read "Cannot
        /// find parameter" as a row missing from this client's data.
        /// </summary>
        protected string EditTargetMissingError(string csvName, string interfaceType, string identifierOrKeyPathOrGuid,
            string propertyName) =>
            IsDeletedByOverride(GetLoadedForOverride(interfaceType, identifierOrKeyPathOrGuid))
                ? $"Cannot edit ({interfaceType})[{identifierOrKeyPathOrGuid}] property [{propertyName}]: the row is deleted by overrides."
                : $"Cannot find parameter for csv [{csvName}] and identifier/guid [{identifierOrKeyPathOrGuid}].";

        /// <summary>
        /// The error for an edit on a row added by overrides.  Its values are stored as overrides, so it's read-only.
        /// </summary>
        protected static string EditAddedRowError(string interfaceType, string identifierOrKeyPathOrGuid,
            string propertyName) =>
            $"Cannot edit ({interfaceType})[{identifierOrKeyPathOrGuid}] property [{propertyName}]: rows added by overrides are read-only. Set the value in the add.";

        protected virtual bool RemoveEditOverride(string csvName, string interfaceType, string identifierOrKeyPathOrGuid, string propertyName, out string error)
        {
            var mutableParameter = GetCurrentForOverride(interfaceType, identifierOrKeyPathOrGuid);
            if (mutableParameter == null)
            {
                error = EditTargetMissingError(csvName, interfaceType, identifierOrKeyPathOrGuid, propertyName);
                return false;
            }

            if (!mutableParameter.RevertEditedProperty(propertyName, out error))
            {
                error = $"Error reverting edit ({interfaceType})[{identifierOrKeyPathOrGuid}] property [{propertyName}]: {error}";
                return false;
            }

            if (--_editCountByParameter[mutableParameter] == 0)
                _editCountByParameter.Remove(mutableParameter);
            return true;
        }

        // whether a loaded row has an edit from ApplyOverrides
        protected virtual bool HasEditOverride(IMutableParameter parameter) =>
            _editCountByParameter.ContainsKey(parameter);

        #endregion

        #region Delete overrides

        private bool IsDeletedByOverride(IMutableParameter parameter) =>
            _deletedParameters != null && _deletedParameters.Contains(parameter);

        private bool ApplyDeleteOverride(string csvName, string identifierOrGuid, out IMutableParameter parameter, out string error)
        {
            // a delete only hides a loaded row: by identifier, then guid, then struct key path
            var interfaceType = LocalCSV.CSVUtil.CSVToInterfaceFileName(csvName);
            parameter = GetLoadedForOverride(interfaceType, identifierOrGuid);
            if (parameter == null)
            {
                // not loaded, so either added by overrides or missing.  Callers read "Cannot find parameter" as a row
                // missing from this client's data, so name an added row.
                bool isAdded = GetCurrentForOverride(interfaceType, identifierOrGuid) != null;
                error = isAdded
                    ? $"Cannot delete csv [{csvName}] identifier/guid [{identifierOrGuid}]: it was added by overrides, so leave it out of the add instead."
                    : $"Cannot find parameter for csv [{csvName}] and identifier/guid [{identifierOrGuid}].";
                return false;
            }

            if (IsDeletedByOverride(parameter))
            {
                error = $"Cannot delete csv [{csvName}] identifier/guid [{identifierOrGuid}]: it's already deleted.";
                return false;
            }

            // deletes apply before edits, so the edit is from an earlier call, which would lose it without failing
            if (HasEditOverride(parameter))
            {
                error = $"Cannot delete csv [{csvName}] identifier/guid [{identifierOrGuid}]: it's edited by overrides.";
                return false;
            }

            _deletedParameters ??= new HashSet<IMutableParameter>();
            _deletedParameters.Add(parameter);
            error = null;
            return true;
        }

        private void RemoveDeleteOverride(IMutableParameter parameter) => _deletedParameters.Remove(parameter);

        #endregion

        #region Add overrides

        /// <inheritdoc cref="IMutableParameterManager.ParameterFactory"/>
        public IParameterFactory ParameterFactory { get; set; }

        /// <summary>
        /// Whether ApplyOverrides added the parameter.  Added rows keep their values as overrides, so they can't be
        /// edited.
        /// </summary>
        protected bool IsAddedByOverride(IMutableParameter parameter) =>
            _addedParameters != null && _addedParameters.Contains(parameter);

        // a null key finds nothing, since a dictionary throws on one
        private static bool TryGetAdded(Dictionary<string, Dictionary<string, IMutableParameter>> mappings, string type,
            string key, out IMutableParameter parameter)
        {
            if (key != null && mappings != null && mappings.TryGetValue(type, out Dictionary<string, IMutableParameter> parameters))
                return parameters.TryGetValue(key, out parameter);
            parameter = null;
            return false;
        }

        private bool ApplyAddOverride(string csvName, string identifierOrKeyPath, JObject values, out ICreatedParameter created, out string error)
        {
            created = ParameterFactory.Create(this, LocalCSV.CSVUtil.CSVToInterfaceFileName(csvName), identifierOrKeyPath);
            if (created == null)
            {
                error = $"Cannot find parameter type for csv [{csvName}].";
                return false;
            }

            /*
             * An info is registered by its identifier: rows added by overrides are only referenced by identifier, so it
             * needs no guid.  A struct is registered by its key path and by the guid of its key path, since it can be
             * referenced by either.
             */
            var key = created.IdentifierOrKeyPath;
            var guid = created.IsInfo ? null : _guidConverter.ToGuid(key);
            var interfaceNames = created.InterfaceNames;
            for (int i = 0; i < interfaceNames.Count; i++)
            {
                var interfaceName = interfaceNames[i];
                // a loaded row clashes even when it's deleted: a row can't be deleted and added, so edit it instead
                var loaded = created.IsInfo ? GetLoaded(interfaceName, key) : GetLoadedWithGUID(interfaceName, guid);
                if (loaded != null && IsDeletedByOverride(loaded))
                {
                    error = $"Cannot add parameter for csv [{csvName}] and identifier/key path [{identifierOrKeyPath}]: it's deleted. Edit the row instead of deleting and adding it.";
                    return false;
                }

                // so does a row added by overrides, in this call or an earlier one
                bool added = created.IsInfo
                    ? TryGetAdded(_addedIdentifierMappings, interfaceName, key, out _)
                    : TryGetAdded(_addedGuidMappings, interfaceName, guid, out _);
                if (loaded != null || added)
                {
                    error = $"Cannot add parameter for csv [{csvName}] and identifier/key path [{identifierOrKeyPath}]: it already exists.";
                    return false;
                }
            }

            /*
             * Set the values before registering the row, so a row that fails isn't registered.  The names are unique and
             * EditProperty fails on one that isn't a column, so with the right count every column gets set.  A
             * reference isn't looked up when it's set, so a row can point to a row added later in this call.
             */
            var columns = created.Columns;
            if (values.Count != columns.Count)
            {
                error = $"Error adding csv [{csvName}] identifier/key path [{identifierOrKeyPath}]: Expected {columns.Count} columns ({string.Join(", ", columns)}), got {values.Count}.";
                return false;
            }

            // in ordinal order, as ApplyOverrides reads the json, so the column an error names doesn't depend on it
            foreach (var property in values.Properties().OrderBy(column => column.Name, StringComparer.Ordinal))
            {
                if (!created.Parameter.EditProperty(this, property.Name, (string)property.Value, out error))
                {
                    error = $"Error adding csv [{csvName}] identifier/key path [{identifierOrKeyPath}]: {property.Name}: {error}";
                    return false;
                }
            }

            _addedIdentifierMappings ??= new Dictionary<string, Dictionary<string, IMutableParameter>>();
            _addedGuidMappings ??= new Dictionary<string, Dictionary<string, IMutableParameter>>();
            _addedParameters ??= new HashSet<IMutableParameter>();
            var mappings = created.IsInfo ? _addedIdentifierMappings : _addedGuidMappings;
            for (int i = 0; i < interfaceNames.Count; i++)
            {
                if (!mappings.TryGetValue(interfaceNames[i], out Dictionary<string, IMutableParameter> parameters))
                {
                    parameters = new Dictionary<string, IMutableParameter>();
                    mappings[interfaceNames[i]] = parameters;
                }

                parameters[key] = created.Parameter;
                if (!created.IsInfo)
                    parameters[guid] = created.Parameter;
            }
            _addedParameters.Add(created.Parameter);

            error = null;
            return true;
        }

        private void RemoveAddOverride(ICreatedParameter created)
        {
            var identifierOrKeyPath = created.IdentifierOrKeyPath;
            var guid = created.IsInfo ? null : _guidConverter.ToGuid(identifierOrKeyPath);
            var mappings = created.IsInfo ? _addedIdentifierMappings : _addedGuidMappings;
            var interfaceNames = created.InterfaceNames;
            for (int i = 0; i < interfaceNames.Count; i++)
            {
                mappings[interfaceNames[i]].Remove(identifierOrKeyPath);
                if (!created.IsInfo)
                    mappings[interfaceNames[i]].Remove(guid);
            }
            _addedParameters.Remove(created.Parameter);
        }

        #endregion
    }
}
