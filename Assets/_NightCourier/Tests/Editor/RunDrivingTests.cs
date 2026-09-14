using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NightCourier.Tests
{
    public static class RunDrivingTests
    {
        private static TestRunnerApi activeApi;
        private static bool running;

        [MenuItem("NightCourier/Run Sport Regression")]
        public static void RunSport()
        {
            Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                groupNames = new[] { "NightCourier.Tests.SportPowertrainTests", "NightCourier.Tests.DrivingPrototypeTests.SportRunsTwoGearsWithIsolatedAudioAndBody" }
            }));
        }

        [MenuItem("NightCourier/Run Driving Regression Tests %#t")]
        public static void Run()
        {
            Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[] { "NightCourier.PlayModeTests" }
            }));
        }

        [MenuItem("NightCourier/Run Pavement Regression")]
        public static void RunPavement()
        {
            Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                testNames = new[] { "NightCourier.Tests.DrivingPrototypeTests.ParkedPickupRestartsAndSteersOnPavement" }
            }));
        }

        [MenuItem("NightCourier/Run Battery Regression")]
        public static void RunBattery()
        {
            Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                testNames = new[] { "NightCourier.Tests.DrivingPrototypeTests.BatteryStartsHalfFullDrainsAndChargesWhileParked" }
            }));
        }

        private static void Execute(ExecutionSettings settings)
        {
            if (running)
            {
                // A Test Framework abort (for example, Play Mode interrupted by a
                // script reload) can skip RunFinished and leave this static latch set.
                // A fresh command in Edit Mode is an explicit recovery request.
                if (!EditorApplication.isPlaying) ReleaseRunner();
                else
                {
                    Debug.LogWarning("[Driving regression] A test run is already active.");
                    return;
                }
            }
            running = true;
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(new Results());
            activeApi.Execute(settings);
        }

        private static void ReleaseRunner()
        {
            running = false;
            if (activeApi == null) return;
            Object.DestroyImmediate(activeApi);
            activeApi = null;
        }

        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.Test.IsSuite)
                    Debug.Log($"[Driving regression] {result.Test.Name}: {result.ResultState} {result.Message}");
            }
            public void RunFinished(ITestResultAdaptor result)
            {
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/DrivingRegression.txt", $"{result.ResultState}: {result.PassCount} passed, {result.FailCount} failed\n{result.Message}");
                Debug.Log($"[Driving regression] Complete: {result.PassCount} passed, {result.FailCount} failed");
                ReleaseRunner();
            }
        }
    }
}
