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

        /// <summary>
        /// This allows different tests to share data.
        /// </summary>
        public Dictionary<string, string> LocalStash = new Dictionary<string, string>();

        public void SetToDefaults()
        {
            ((IInstrument)TestClass).SetToDefaults();

            //if (TestClass is IInstrument)
            //{
            //    ((IInstrument)TestClass).SetToDefaults();
            //}
            //else if (TestClass is IComposite)
            //{
            //    ((IComposite)TestClass).SetToDefaults();
            //}
        }
    }
}
