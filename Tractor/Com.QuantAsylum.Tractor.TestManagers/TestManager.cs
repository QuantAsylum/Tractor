using Com.QuantAsylum.Tractor.Tests;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tractor;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    public enum ChannelEnum { Left, Right};

    /// <summary>
    /// Data returned from some equipment is an array of Points (for example, amplitude and frequency)
    /// </summary>
    public class PointD
    {
        public double X;
        public double Y;
    }

    /// <summary>
    /// TestManager manages the TestClass
    /// </summary>
    class TestManager
    {
        public object TestClass;

        public delegate void StartEditing();
        public delegate void DoneEditing();
        public delegate void CancelEditing();

        public Dictionary<string, string> LocalStash = new Dictionary<string, string>();

        public void SetCallbacks(StartEditing startEditing, DoneEditing doneEditing, CancelEditing cancelEditing)
        {
            TestBase.StartEditingCallback = startEditing;
            TestBase.DoneEditingCallback = doneEditing;
            TestBase.CancelEditingCallback = cancelEditing;
        }
    }
}
