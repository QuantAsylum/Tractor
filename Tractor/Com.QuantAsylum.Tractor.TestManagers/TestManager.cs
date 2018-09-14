using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.TestManagers
{
    public enum ChannelEnum { Left, Right};

    public class PointD
    {
        public double X;
        public double Y;
    }

    /// <summary>
    /// Test managers are primarily focused on ensuring all the instruments required for a set of
    /// tests are present and working
    /// </summary>
    abstract public class TestManager
    {
        /// <summary>
        /// A list of tests we will run
        /// </summary>
        public List<TestBase> TestList { get; set; }

        public TestManager()
        {
            TestList = new List<TestBase>();
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

            for (int i = 0; i < 10000; i++)
            {
                newName = nameRoot + i.ToString();

                bool nameIsUnique = true;

                foreach (TestBase Test in TestList)
                {
                    if (Test.Name == newName)
                    {
                        nameIsUnique = false;
                        break;
                    }
                }

                if (nameIsUnique)
                    break;
            }

            // Here, we've found a unique name
            return newName;
        }

        /// <summary>
        /// Must be implemented by derived classes
        /// </summary>
        abstract public void ConnectToDevices();

        /// <summary>
        /// Must be implemented by derived classes
        /// </summary>
        /// <returns></returns>
        abstract public bool AllConnected();

        /// <summary>
        /// Returns the friendly name of this hardware profile. This will convey to the user
        /// that the measurement devices are a QA401 or a QA401 + QA450, or any other collection
        /// of hardware.
        /// </summary>
        /// <returns></returns>
        abstract public string GetProfileName();

        // DUT power. This section controls the power provided to the DUT.

        /// <summary>
        /// Sets default options for the instrument
        /// </summary>
        abstract public void DutSetDefault();

        abstract public void DutSetPowerState(bool powerEnable);
        abstract public bool DutGetPowerState();
        abstract public float DutGetCurrent();
        abstract public void DutSetVoltage(float voltage_V);
        abstract public float DutGetVoltage();
        abstract public float DutGetTemperature();

        // Programmable Load 
        abstract public void LoadSetDefault();
        /// <summary>
        /// Returns a list of impedances supported by the programmable load. The value '0' is 
        /// reserved to indicate the load is disconnected
        /// </summary>
        /// <returns></returns>
        abstract public int[] GetImpedances();
        abstract public void LoadSetImpedance(int impedance);
        abstract public int LoadGetImpedance();
        abstract public float LoadGetTemperature();

        abstract public void AudioAnalyzerSetDefaults();

        abstract public void AudioAnalyzerSetFftLength(uint length);

        // Audio Generators
        abstract public void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz);
        abstract public void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz);

        // Audio Input Attenuators
        /// <summary>
        /// Returns a list of supported max intput values
        /// </summary>
        /// <returns></returns>
        abstract public int[] GetInputRanges();
        abstract public void SetInputRange(int inputRange_dB);

        // Audio measurement control
        abstract public void RunSingle();
        abstract public bool AnalyzerIsBusy();

        abstract public PointD[] GetData(ChannelEnum channel);


        abstract public double ComputeRms(PointD[] data, float startFreq, float stopFreq);
        abstract public double ComputeThdPct(PointD[] data, float freq, float stopFreq);

        abstract public Bitmap GetBitmap();
    }
}
