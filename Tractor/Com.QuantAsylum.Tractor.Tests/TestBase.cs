using Com.QuantAsylum.Tractor.Tests.GainTests;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Com.QuantAsylum.Tractor.Tests.IMDTests;
using Com.QuantAsylum.Tractor.Tests.NoiseFloors;
using Tractor.Com.QuantAsylum.Tractor.Tests.THDs;
using Com.QuantAsylum.Tractor.Tests.Other;
using Com.QuantAsylum.Tractor.TestManagers;
using static Com.QuantAsylum.Tractor.TestManagers.TestManager;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests
{
    public class TestResult
    {
       
        /// <summary>
        /// An array of doubles that represent the test results. Index 0 = Left, Index 1 = Right
        /// </summary>
        public double[] Value;

        /// <summary>
        /// Results formatted for display. This allows each test to show whatever number of 
        /// significant digits makes sense OR to display an error code
        /// </summary>
        public string[] StringValue;

        /// <summary>
        /// Indicates if the test overall passed
        /// </summary>
        public bool Pass;

        /// <summary>
        /// A test-specific string that will be passed to the operator 
        /// </summary>
        public string OperatorMessage = "";

        public TestResult(int count)
        {
            Pass = false;
            Value = new double[count];
            StringValue = new string[count];


            for (int i=0; i<count; i++)
            {
                Value[i] = 0;
                StringValue[i] = "---";
            }
        }
    }

    /// <summary>
    /// Base class for handling the display and update of data used by each test. 
    /// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(GainA01))]
    [System.Xml.Serialization.XmlInclude(typeof(GainA03))]
    [System.Xml.Serialization.XmlInclude(typeof(ImdA01))]
    [System.Xml.Serialization.XmlInclude(typeof(ImdA03))]
    [System.Xml.Serialization.XmlInclude(typeof(RmsLevelA01))]
    [System.Xml.Serialization.XmlInclude(typeof(RmsLevelA03))]
    [System.Xml.Serialization.XmlInclude(typeof(IdInputA00))]
    [System.Xml.Serialization.XmlInclude(typeof(ThdA01))]
    [System.Xml.Serialization.XmlInclude(typeof(ThdA03))]
    [System.Xml.Serialization.XmlInclude(typeof(ThdB03))]
    [System.Xml.Serialization.XmlInclude(typeof(PromptA00))]
    [System.Xml.Serialization.XmlInclude(typeof(ImpedanceA03))]
    [System.Xml.Serialization.XmlInclude(typeof(PowerA14))]
    [System.Xml.Serialization.XmlInclude(typeof(EfficiencyA07))]
    [System.Xml.Serialization.XmlInclude(typeof(AuditionA01))]
    [System.Xml.Serialization.XmlInclude(typeof(ShellA00))]
    [System.Xml.Serialization.XmlInclude(typeof(GainSorted3A01N))]
    [System.Xml.Serialization.XmlInclude(typeof(GainSorted5A01N))]
    [System.Xml.Serialization.XmlInclude(typeof(FreqResponseA01))]
    [System.Xml.Serialization.XmlInclude(typeof(FreqResponseA03))]
    [System.Xml.Serialization.XmlInclude(typeof(ThdNA01))]
    [System.Xml.Serialization.XmlInclude(typeof(MicCompareA01))]
    //
    // Naming Convention for classes:
    // Class names are as follows, and each must be unique
    // GainSorted5A01N
    // Name of Test is GainSorted5
    // A is the variant of this particular test. The next varient would be B, etc
    // 01 is a bitmask indicating the hardware needed:
    //  0x1 (lsb) indicates the IAudioAnalyzer interface is needed
    //  0x2 indicates the IProgrammableLoad interface is needed
    //  0x4 indicates the Current Meter is needed
    //  0x8 indicates the Power Supply is needed
    // N indicates the operator will be notified with information beyond the pass/fail (that is, an actionable message)
    //
    public class TestBase
    {
        [Flags]
        internal enum HardwareTypes { AudioAnalyzer = 0x01, ProgrammableLoad = 0x02, CurrentMeter = 0x04, PowerSupply = 0x10 }

        /// <summary>
        /// Returns a value indicating the hardware required to run this test
        /// </summary>
        internal virtual int HardwareMask
        {
            get
            {
                return 0;
            }
 
        }

        internal bool RunTest { get; set; } = true;

        /// <summary>
        /// Returns the user-assigned name for the test. This name must be unique among
        /// all the tests
        /// </summary>
        [ObjectEditorAttribute(Index = 3, DisplayText = "Test Name")]
        public string Name = "Placeholder";

        /// <summary>
        /// This is to permit grouping of tests by type. The type should be 
        /// one of the following:
        /// User Input
        /// Level
        /// Frequency Response
        /// Phase
        /// Crosstalk 
        /// SNR
        /// Distortion
        /// 
        /// Level measures the absolute level of a signal
        /// Frequency Response measures amplitude relative to a reference amplitude
        /// Phase measures the phase relationship between two signals
        /// Crosstalk meausures the leakage between channels
        /// SNR measures the ratio between the signal and the noise
        /// Distortion measures THD or THD + N
        /// </summary>
        internal enum TestTypeEnum { Unspecified, Operator, LevelGain, Distortion, Other };
        internal TestTypeEnum _TestType = TestTypeEnum.Unspecified;

        internal Bitmap TestResultBitmap { get; set; }

        internal TestManager Tm;

        internal void SetTestManager(TestManager tm)
        {
            Tm = tm;
        }

        public object ShallowCopy()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Each derived class can override and check the values as needed. This would be
        /// checking beyond what the ObjectEditorAttributes already check. Examples might
        /// be verifying load impedances are legit since the attributes have not way of 
        /// specifying that check (eg only 4 or 8 ohms)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public virtual bool CheckValues(out string s)
        {
            s = "";
            return true;
        }

        public bool IsParamsValid(out string s)
        {
            s = "";
            return true;
        }

        public virtual string GetTestLimits()
        {
            return "";
        }

        public virtual string GetTestDescription()
        {
            throw new NotImplementedException();
        }

        public virtual string GetTestName()
        {
            return this.GetType().Name;
        }

        internal virtual TestTypeEnum TestType
        {
            get
            {
                return _TestType;
            }
        }

        public virtual void DoTest(string title, out TestResult testResult)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Allows test to determine if all the required pieces are present
        /// </summary>
        /// <returns></returns>
        public virtual bool IsRunnable()
        {
            bool success = true;

            if ((HardwareMask & (int)HardwareTypes.AudioAnalyzer) > 0 )
            {
                if (Tm.TestClass is IAudioAnalyzer == false)
                    success = false;
            }
            if ((HardwareMask & (int)HardwareTypes.CurrentMeter) > 0)
            {
                if (Tm.TestClass is ICurrentMeter == false)
                    success = false;
            }
            if ((HardwareMask & (int)HardwareTypes.PowerSupply) > 0)
            {
                if (Tm.TestClass is IPowerSupply == false)
                    success = false;
            }
            if ((HardwareMask & (int)HardwareTypes.ProgrammableLoad) > 0)
            {
                if (Tm.TestClass is IProgrammableLoad == false)
                    success = false;
            }

            return success;
        }

        /// <summary>
        /// Because the remoting interface can only pass common data types that know how to 
        /// serialize themselves, images are passed as byte arrays. This helper will take
        /// a byte array and save it to the property
        /// </summary>
        /// <param name="imgArray"></param>
        public Bitmap CaptureBitmap(byte[] imgArray)
        {
            MemoryStream ms = new MemoryStream(imgArray);
            return (Bitmap)Image.FromStream(ms);
        }      
    }

    public class UiTestBase : TestBase
    {

    }

    public class AudioTestBase : TestBase
    {
        [ObjectEditorAttribute(Index = 100, DisplayText = "FFT Size", MustBePowerOfTwo = true, MinValue = 2048, MaxValue = 65536)]
        public uint FftSize = 8192;

        [ObjectEditorAttribute(Index = 102, DisplayText = "Retry Count")]
        public int RetryCount = 2;

        [ObjectEditorAttribute(Index = 104, DisplayText = "Measure Left Channel")]
        public bool LeftChannel = true;

        [ObjectEditorAttribute(Index = 106, DisplayText = "Measure Right Channel")]
        public bool RightChannel = true;
    }
}
