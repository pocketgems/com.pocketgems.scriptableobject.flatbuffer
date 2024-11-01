using System;
using System.Collections.Generic;
using System.IO;
#if !UNITY_EDITOR
using System.Reflection;
#endif
using PocketGems.Parameters.Common.Editor;
using Scriban;
using Scriban.Runtime;

namespace PocketGems.Parameters.Common.Util.Editor
{
    /// <summary>
    /// Helper class to generate code with Scriban
    /// </summary>
    internal static class ScribanHelper
    {
        public static void GenerateClass(string templateFilename, string outputFilePath, IDictionary<string, object> args = null)
        {
            var path = Path.Combine(EditorParameterConstants.Template.RootDirPath, templateFilename);
            var contents = GenerateCode(path, args);
            File.WriteAllText(outputFilePath, contents);
        }

        private static string GenerateCode(string templateFilePath, IDictionary<string, object> args = null)
        {
            string templateText = null;
            if (File.Exists(templateFilePath))
            {
                templateText = File.ReadAllText(templateFilePath);
            }
#if !UNITY_EDITOR
            else
            {
                // support for external code generation
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyNamespace = assembly.GetName().Name;
                var folderKeyPath = "Common.Templates";
                var templateFilename = Path.GetFileName(templateFilePath);
                using (Stream stream = assembly.GetManifestResourceStream($"{assemblyNamespace}.{folderKeyPath}.{templateFilename}"))
                    if (stream != null)
                    {
                        using StreamReader reader = new StreamReader(stream);
                        templateText = reader.ReadToEnd();
                    }
            }
#endif

            if (templateText == null)
            {
                throw new FileNotFoundException($"{templateFilePath} not found");
            }

            var template = Template.Parse(templateText, templateFilePath);
            if (template.HasErrors)
                throw new FormatException($"{templateFilePath} template has issues: {template.Messages}");

            var scriptObject = new ScriptObject();
            if (args != null)
            {
                foreach (var arg in args)
                    scriptObject.Add(arg.Key, arg.Value);
            }

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            return template.Render(context);
        }
    }
}
