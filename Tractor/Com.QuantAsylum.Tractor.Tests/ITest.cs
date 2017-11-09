using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// Interface class for each test
    /// </summary>
    interface ITest
    {
        /// <summary>
        /// Returns a description of the test to be run. This will be shown 
        /// to the user when they are selecting the which test to run. This 
        /// is hardcoded into the test class and cannot be modified by the user
        /// </summary>
        /// <returns></returns>
        string TestDescription();

        /// <summary>
        /// Returns the name of the test. This will be shown 
        /// to the user when they are selecting the which test to run. This 
        /// is hardcoded into the test class and cannot be modified by the user
        /// </summary>
        /// <returns></returns>
        string TestName();

        /// <summary>
        /// Synchronously execute the test
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pass"></param>
        void DoTest(out float[] value, out bool pass);
    }
}
