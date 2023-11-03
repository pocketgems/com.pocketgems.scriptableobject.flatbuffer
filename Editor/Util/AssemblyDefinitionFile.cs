using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = System.Object;

namespace PocketGems.Parameters.Util
{
    [Serializable]
    public class AssemblyDefinitionFile
    {
        // https://docs.unity3d.com/2020.3/Documentation/Manual/AssemblyDefinitionFileFormat.html
        public string name;

        // default values if creating a new one in Unity
        public string[] references;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool autoReferenced;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public string[] defineConstraints;
        public string[] optionalUnityReferences;

        public AssemblyDefinitionFile(string name)
        {
            this.name = name;
            references = new string[] { };
            includePlatforms = new string[] { };
            excludePlatforms = new string[] { };
            allowUnsafeCode = false;
            autoReferenced = true;
            overrideReferences = false;
            precompiledReferences = new string[] { };
            defineConstraints = new string[] { };
            optionalUnityReferences = new string[] { };
        }

        public static AssemblyDefinitionFile LoadFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<AssemblyDefinitionFile>(json);
        }

        public void WriteFile(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            var fileName = $"{name}.asmdef";
            string json = JsonUtility.ToJson(this);
            File.WriteAllText(Path.Combine(directoryPath, fileName), json);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            AssemblyDefinitionFile a = (AssemblyDefinitionFile)obj;
            return name.Equals(a.name) &&
                   references.SequenceEqual(a.references) &&
                   includePlatforms.SequenceEqual(a.includePlatforms) &&
                   excludePlatforms.SequenceEqual(a.excludePlatforms) &&
                   allowUnsafeCode == a.allowUnsafeCode &&
                   autoReferenced == a.autoReferenced &&
                   overrideReferences == a.overrideReferences &&
                   precompiledReferences.SequenceEqual(a.precompiledReferences) &&
                   defineConstraints.SequenceEqual(a.defineConstraints) &&
                   optionalUnityReferences.SequenceEqual(a.optionalUnityReferences);
        }
    }
}
