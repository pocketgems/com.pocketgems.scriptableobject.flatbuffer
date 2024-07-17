using System;
using System.Collections.Generic;
using System.Text;

namespace PocketGems.Parameters.Common.Util.Editor
{
    internal class SchemaBuilder
    {
        private readonly string _rootClassName;
        private readonly Dictionary<string, List<Tuple<string, string>>> _tableNameToProperties;

        public SchemaBuilder(string rootClassName)
        {
            _rootClassName = rootClassName;
            _tableNameToProperties = new Dictionary<string, List<Tuple<string, string>>>();
        }

        public List<string> TableNames
        {
            get => new List<string>(_tableNameToProperties.Keys);
        }

        public void DefineArrayField(string tableName, string fieldName, FlatBufferFieldType fieldType)
        {
            DefineArrayField(tableName, fieldName, FieldTypeString(fieldType));
        }

        public void DefineArrayField(string tableName, string fieldName, string fieldType)
        {
            DefineField(tableName, fieldName, $"[{fieldType}]");
        }

        public void DefineField(string tableName, string fieldName, FlatBufferFieldType fieldType)
        {
            DefineField(tableName, fieldName, FieldTypeString(fieldType));
        }

        public void DefineField(string tableName, string fieldName, string fieldType)
        {
            var property = new Tuple<string, string>(fieldName, fieldType);
            if (_tableNameToProperties.TryGetValue(tableName, out List<Tuple<string, string>> properties))
            {
                properties.Add(property);
            }
            else
            {
                _tableNameToProperties[tableName] = new List<Tuple<string, string>> { property };
            }
        }

        /// <summary>
        /// These properties supposed to be lower case in the FlatBuffer schema file.  We cannot use
        /// the lower case versions in the Enum because they're reserved system terms.
        /// </summary>
        /// <param name="type">Enum</param>
        /// <returns>String to be used in schema file.</returns>
        private string FieldTypeString(FlatBufferFieldType type) => type.ToString().ToLower();

        public string GenerateSchemaContent()
        {
            StringBuilder schemaString = new StringBuilder();
            var classNames = new List<string>(_tableNameToProperties.Keys);

            // build schema for each csv
            for (int i = 0; i < classNames.Count; i++)
            {
                var className = classNames[i];
                schemaString.AppendFormat("table {0} {{\n", className);
                var properties = _tableNameToProperties[className];
                for (int j = 0; j < properties.Count; j++)
                {
                    var pair = properties[j];
                    var fieldName = pair.Item1.ToSnakeCase();
                    var fieldType = pair.Item2;
                    schemaString.AppendFormat("    {0}:{1};\n", fieldName, fieldType);
                }

                schemaString.Append("}\n\n");
            }

            schemaString.AppendFormat("root_type {0};", _rootClassName);

            return schemaString.ToString();
        }
    }
}
