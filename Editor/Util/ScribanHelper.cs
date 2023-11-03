using System;
using System.Collections.Generic;
using System.IO;
using Scriban;
using Scriban.Runtime;

namespace PocketGems.Parameters.Util
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
            var template = Template.Parse(File.ReadAllText(templateFilePath), templateFilePath);
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
