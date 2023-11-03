using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Util
{
    public class ParameterDebugTest
    {
        private string _logMessage;
        private bool _editorVerboseLogsSetting;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _logMessage = "the log message";
            _editorVerboseLogsSetting = ParameterPrefs.VerboseLogs;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ParameterPrefs.VerboseLogs = _editorVerboseLogsSetting;
        }

        [Test]
        public void Log()
        {
            ParameterDebug.Log(_logMessage);
            LogAssert.Expect(LogType.Log, _logMessage);
        }

        [Test]
        public void LogError()
        {
            ParameterDebug.LogError(_logMessage);
            LogAssert.Expect(LogType.Error, _logMessage);
        }

        [Test]
        public void LogVerbose()
        {
            ParameterPrefs.VerboseLogs = false;
            ParameterDebug.LogVerbose(_logMessage);
            LogAssert.NoUnexpectedReceived();

            ParameterPrefs.VerboseLogs = true;
            ParameterDebug.LogVerbose(_logMessage);
            LogAssert.Expect(LogType.Log, _logMessage);
        }
    }
}
