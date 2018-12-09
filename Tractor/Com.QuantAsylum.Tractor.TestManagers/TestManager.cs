using Com.QuantAsylum.Tractor.Tests;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
    /// Test managers are primarily focused on ensuring all the instruments required for a set of
    /// tests are present and working, as well as implementing the various abstract methods that make
    /// sense for that particular set of equipment. 
    /// </summary>
    class TestManager
    {
        public object TestClass = (object)new QA401_QA450();

        /// <summary>
        /// A list of tests we will run
        /// </summary>
        public List<TestBase> TestList = new List<TestBase>();

        public delegate void StartEditing();
        public delegate void DoneEditing();
        public delegate void CancelEditing();

        public TestManager()
        {
        }

        public void SetTestList(List<TestBase> testList)
        {
            TestList = testList;
        }

        public void SetCallbacks(StartEditing startEditing, DoneEditing doneEditing, CancelEditing cancelEditing)
        {
            TestBase.StartEditingCallback = startEditing;
            TestBase.DoneEditingCallback = doneEditing;
            TestBase.CancelEditingCallback = cancelEditing;
        }

        /// <summary>
        /// Finds a unique name in the TestList given a root. For example, if
        /// the root is "THD" and the list is empty, then "THD-1" will be returned, 
        /// and then "THD-2" will be returned, etc. 
        /// </summary>
        /// <param name="nameRoot"></param>
        /// <returns></returns>
        virtual public string FindUniqueName(string nameRoot)
        {
            nameRoot += "-";
            string newName = "";

            // Bounded search for unique names
            for (int i = 0; i < 10000; i++)
            {
                newName = nameRoot + i.ToString();

                if (TestList.Count == 0)
                    return newName;

                try
                {
                    if (TestList.First(o => o.Name == newName) != null)
                    {
                        // This name is already being used. Keep going
                    }
                }
                catch
                {
                    // Nothing matched. 
                    break;
                }
            }

            // Here, we've found a unique name
            return newName;
        }
    }
}
