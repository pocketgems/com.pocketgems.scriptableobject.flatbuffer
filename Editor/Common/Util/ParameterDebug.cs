using UnityEngine;

namespace PocketGems.Parameters.Common.Util.Editor
{
    internal static class ParameterDebug
    {
        /// <summary>
        ///  Logs to the console only if verbose logs are enabled.
        /// </summary>
        /// <param name="log">String to log</param>
        public static void LogVerbose(string log)
        {
            if (ParameterPrefs.VerboseLogs)
            {
                Debug.Log(log);
            }
        }

        /// <summary>
        ///  Error logs to the console.
        /// </summary>
        /// <param name="log">String to log</param>
        public static void LogError(string log) => Debug.LogError(log);

        /// <summary>
        ///  Warning logs to the console.
        /// </summary>
        /// <param name="log">String to log</param>
        public static void LogWarning(string log) => Debug.LogWarning(log);

        /// <summary>
        ///  Logs to the console.
        /// </summary>
        /// <param name="log">String to log</param>
        public static void Log(string log) => Debug.Log(log);
    }
}
