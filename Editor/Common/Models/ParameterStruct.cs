using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.PropertyTypes.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public class ParameterStruct : ParameterInterface, IParameterStruct
    {
        public ParameterStruct(Type type) : base(type)
        {
        }

        protected override Regex NamingPattern() => EditorParameterConstants.Interface.StructNameRegex;
        protected override Type RequiredBaseType() => typeof(IBaseStruct);

        public string StructName(bool includeExtension) =>
            NamingUtil.StructNameFromBaseName(BaseName, includeExtension);

        public override bool Validate(out IReadOnlyList<string> outErrors)
        {
            var baseReturn = base.Validate(out outErrors);
            List<string> errors = new List<string>(outErrors);
            outErrors = errors;

            // find circular references between struct interfaces which would result in a compilation error after code generation
            var error = CheckStructCircularReferences(Type, new List<Type>());
            if (error != null)
                errors.Add(error);

            return baseReturn && errors.Count == 0;
        }

        private string CheckStructCircularReferences(Type structInterfaceType, List<Type> path)
        {
            var error = path.Contains(structInterfaceType);
            string errorString = null;
            path.Add(structInterfaceType);
            if (error)
            {
                // build error string
                StringBuilder errorStringBuilder = new StringBuilder("Circular Struct Reference: ");
                for (int i = 0; i < path.Count; i++)
                {
                    if (i > 0)
                        errorStringBuilder.Append(" -> ");
                    errorStringBuilder.Append(path[i]);
                }
                errorString = errorStringBuilder.ToString();
            }
            else
            {
                // grab all interfaces
                var allInterfaces = new List<Type>();
                void DFS(Type t)
                {
                    var baseInterfaces = t.GetInterfaces().OrderBy(t => t.Name).ToList();
                    foreach (var baseInterface in baseInterfaces)
                    {
                        if (!allInterfaces.Contains(baseInterface))
                            DFS(baseInterface);
                    }
                    allInterfaces.Add(t);
                }
                DFS(structInterfaceType);

                // iterate through all properties to find struct references
                for (int i = 0; i < allInterfaces.Count; i++)
                {
                    var propertyInfos = allInterfaces[i].GetProperties();
                    for (int j = 0; j < propertyInfos.Length; j++)
                    {
                        var propertyInfo = propertyInfos[j];

                        // C# allows circular references in structs if a list is used (not direct field)
                        const bool includeLists = false;
                        var structType = AttemptToGetStructReference(propertyInfo, includeLists);
                        if (structType == null)
                            continue;
                        errorString = CheckStructCircularReferences(structType, path);
                        if (errorString != null)
                            break;
                    }
                    if (errorString != null)
                        break;
                }
            }

            path.RemoveAt(path.Count - 1);
            return errorString;
        }

        private Type AttemptToGetStructReference(PropertyInfo propertyInfo, bool includeLists)
        {
            if (ParameterStructReferencePropertyType.IsReferenceType(propertyInfo, out Type genericType))
                return genericType;
            if (includeLists && ParameterStructReferenceListPropertyType.IsListReferenceType(propertyInfo, out genericType))
                return genericType;
            return null;
        }
    }

}
