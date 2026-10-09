using UnityEditor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Util.Editor
{
    public delegate void EditorApplicationHandler();

    /// <summary>
    /// Convenience class to observe and receive callbacks for Unity project launch.
    /// </summary>
    [ExcludeFromCoverage]
    public static class EditorApplicationObserver
    {
        public static event EditorApplicationHandler ApplicationLaunched;

        /// <summary>
        /// OnProjectChangedInEditor is called multiple times on project launched and recompiled.
        /// This class does additional book keeping to know when a the app has launched.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnLoadCallback()
        {
            if (EditorAnalyticsSessionInfo.id == 0)
            {
                // wait for unity to initialize with a session id
                EditorApplication.update += CheckForSessionId;
                return;
            }
            OnApplicationChangedInEditor();
        }

        /// <summary>
        /// Checks if the watcher detected a first launch already for this session.
        /// </summary>
        /// <returns>true if the a session id exists and handler events have been invoked</returns>
        public static bool ApplicationHasFinishedInitializingSession() =>
            EditorAnalyticsSessionInfo.id != 0 && SessionState.GetBool(LaunchedKey, false);

        // SessionState is kept across domain reloads and cleared when the editor quits.  The analytics session id
        // isn't stored because it can change while the editor stays open, which would look like a new launch.
        private const string LaunchedKey = "ParametersLaunched";

        /// <summary>
        /// Checks for session id to check for an initialized session.
        /// </summary>
        private static void CheckForSessionId()
        {
            if (EditorAnalyticsSessionInfo.id != 0)
            {
                EditorApplication.update -= CheckForSessionId;
                OnApplicationChangedInEditor();
            }
        }

        /// <summary>
        /// Invoke the launch event once per editor session.
        /// </summary>
        private static void OnApplicationChangedInEditor()
        {
            if (SessionState.GetBool(LaunchedKey, false))
                return;
            SessionState.SetBool(LaunchedKey, true);
            OnApplicationLaunched();
        }

        private static void OnApplicationLaunched() => ApplicationLaunched?.Invoke();
    }
}
