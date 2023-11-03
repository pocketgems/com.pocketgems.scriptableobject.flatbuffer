using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner;
using UnityEngine.TestTools;

[assembly: TestRunCallback(typeof(UnitTestListener))]

[ExcludeFromCoverage]
public class UnitTestListener : ITestRunCallback
{
    public static bool AreUnitTestsRunning { get; private set; }

    public void RunStarted(ITest testsToRun) => AreUnitTestsRunning = true;
    public void TestStarted(ITest test) { }
    public void TestFinished(ITestResult result) { }
    public void RunFinished(ITestResult testResults) { AreUnitTestsRunning = false; }
}
