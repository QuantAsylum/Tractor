using Com.QuantAsylum.Tractor.Dialogs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will prompt the user to enter a serial number or other identifier
    /// </summary>
    [Serializable]
    public class ShellA00 : UiTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Process Name:", MaxLength = 128)]
        public string ProcessName = "";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Arguments:", MaxLength = 128)]
        public string Arguments = "";

        [ObjectEditorAttribute(Index = 220, DisplayText = "Timeout (sec)", MinValue = 1, MaxValue = 100)]
        public int TimeoutSec = 20;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Run Minimized")]
        public bool Minimized = false;

        public ShellA00() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            tr = new TestResult(2);

            Process proc = new Process();
            proc.StartInfo.FileName = ProcessName;
            proc.StartInfo.Arguments = Arguments;
            proc.StartInfo.WindowStyle = Minimized ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal;
            proc.Start();
            proc.WaitForExit(TimeoutSec * 1000);

            if (proc.HasExited == false)
            {
                throw new Exception("User process in ShellA00 did not complete. ");
            }

            tr.Pass = true;
        }

        public override string GetTestDescription()
        {
            return "Executes a shell command with specified arguments. Waits for the specified time for the shell command " +
                "to complete. If the shell command completes in that time, the test is passed. If not, the test is terminated.";
        }
    }
}
