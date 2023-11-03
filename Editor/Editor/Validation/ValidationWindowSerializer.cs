using System;
using System.Collections.Generic;
using PocketGems.Parameters.Util;
using PocketGems.Parameters.Validation;
using UnityEditor;
using UnityEngine;

namespace PocketGems.Parameters.Editor.Validation
{
    internal static class ValidationWindowSerializer
    {
        [Serializable]
        public class ValidationErrorCollection
        {
            // key is unique per project
            public static string EditorPrefKey => $"parameter_errors_{HashUtil.MD5Hash(Application.dataPath)}";

            public List<ValidationError> Errors;
            public ValidationErrorCollection(IReadOnlyList<ValidationError> errors)
            {
                Errors = new List<ValidationError>(errors);
            }
        }

        public static IReadOnlyList<ValidationError> DeserializeFromStorage()
        {
            if (!EditorPrefs.HasKey(ValidationErrorCollection.EditorPrefKey))
                return null;

            var errorJson = EditorPrefs.GetString(ValidationErrorCollection.EditorPrefKey);
            var collection = JsonUtility.FromJson<ValidationErrorCollection>(errorJson);
            return collection.Errors;
        }

        public static void SerializeToStorage(IReadOnlyList<ValidationError> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                EditorPrefs.DeleteKey(ValidationErrorCollection.EditorPrefKey);
                return;
            }

            var collection = new ValidationErrorCollection(errors);
            string errorJson = JsonUtility.ToJson(collection);
            EditorPrefs.SetString(ValidationErrorCollection.EditorPrefKey, errorJson);
        }
    }
}
