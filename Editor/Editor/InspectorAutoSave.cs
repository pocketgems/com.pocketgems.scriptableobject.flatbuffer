using System;
using System.Diagnostics;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;
using UnityEditor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor.Editor
{
    [ExcludeFromCoverage]
    [InitializeOnLoad]
    static class InspectorAutoSave
    {
        private const int SaveDelayMillis = 5000;

        private static UnityEngine.Object s_target;
        private static string s_path;
        private static int s_dirtyCount;
        private static readonly Stopwatch s_dirtyStopWatch;
        private static readonly Stopwatch s_dispatchStopWatch;

        static InspectorAutoSave()
        {
            s_dirtyStopWatch = new Stopwatch();
            s_dispatchStopWatch = new Stopwatch();
            Reset();
            UnityEditor.Editor.finishedDefaultHeaderGUI += AutoSave;
            EditorApplication.playModeStateChanged += PlayModeState;
            EditorApplication.update += Update;
        }

        private static void PlayModeState(PlayModeStateChange state)
        {
            // must utilize the ExitingEditMode flag instead of EnteredPlayMode flag.
            // EnteredPlayMode is set very late after the app has started running.
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            if (s_dispatchStopWatch.IsRunning || s_dirtyCount > 0)
            {
                ParameterDebug.Log($"Auto Saving on Play.");
                // SaveAssets will result in GenerateData to be dispatched on the next update loop which will be
                // too late since this environment will be taken down.
                AssetDatabase.SaveAssets();
                // invoke any dispatched actions from the SaveAssets call above
                EditorParameterDataManager.InvokeEditorUpdateActions();
            }
        }

        public static void DispatchDelayedSave()
        {
            if (s_dispatchStopWatch.IsRunning)
                s_dispatchStopWatch.Restart();
            else
                s_dispatchStopWatch.Start();
        }

        private static void Update()
        {
            if (s_dispatchStopWatch.IsRunning && s_dispatchStopWatch.ElapsedMilliseconds >= SaveDelayMillis)
            {
                AssetDatabase.SaveAssets();
                s_dispatchStopWatch.Reset();
                ParameterDebug.Log($"Auto Saved.");
            }

            if (s_target != null)
            {
                if (s_dirtyStopWatch.IsRunning && s_dirtyStopWatch.ElapsedMilliseconds >= SaveDelayMillis)
                {
                    SaveAsset();
                }
            }
        }

        private static void Reset()
        {
            s_target = null;
            s_path = null;

            s_dirtyStopWatch.Reset();
            s_dirtyCount = 0;
        }

        private static void SaveAsset()
        {
            if (s_target != null)
            {
                AssetDatabase.SaveAssetIfDirty(s_target);
                ParameterDebug.Log($"Auto Saved: {s_path}");
            }
            Reset();
        }

        private static void AutoSave(UnityEditor.Editor editor)
        {
            if (s_target != null && (s_target != editor.target || editor.targets.Length > 1))
            {
                // switched inspector - save previous changes
                SaveAsset();
            }

            if (!EditorUtility.IsPersistent(editor.target))
            {
                Reset();
                return;
            }

            if (editor.targets.Length > 1)
            {
                // multiple objects selected
                Reset();
                return;
            }

            if (!EditorUtility.IsDirty(editor.target))
            {
                Reset();
                return;
            }

            if (s_target != editor.target)
            {
                // this function returns only forward slash on mac & windows
                var path = AssetDatabase.GetAssetPath(editor.target);
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (!typeof(ParameterScriptableObject).IsAssignableFrom(assetType))
                {
                    Reset();
                    return;
                }

                s_target = editor.target;
                s_path = path;
            }

            int prevDirtyCount = s_dirtyCount;
            s_dirtyCount = EditorUtility.GetDirtyCount(editor.target);

            if (ParameterPrefs.EnableInspectorAutoSave)
            {
                if (!s_dirtyStopWatch.IsRunning)
                    s_dirtyStopWatch.Start();
                else if (s_dirtyCount != prevDirtyCount)
                    s_dirtyStopWatch.Restart();
            }
        }
    }
}
