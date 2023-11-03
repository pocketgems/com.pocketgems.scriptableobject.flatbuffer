using System.IO;

namespace PocketGems.Parameters.LocalCSV
{
    internal static class CSVUtil
    {
        public static string CSVToInterfaceFileName(string filePath) => $"I{Path.GetFileNameWithoutExtension(filePath)}";
    }
}
