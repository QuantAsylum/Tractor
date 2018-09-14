using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.TestManagers
{

    // Various attribute classes are below. These exist so that the HAL can convey
    // what functions are supported to the higher levels of the design. For example,
    // 

    [AttributeUsage(AttributeTargets.Method)]
    class MethodNotSupported : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    class MethodImpedanceSupported_Ohms : Attribute
    {
        int[] Impedance;
        public MethodImpedanceSupported_Ohms(int[] impedance)
        {
            Impedance = impedance;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    class MethodAttenSupported_dB : Attribute
    {
        int[] Impedance;
        public MethodAttenSupported_dB(int[] impedance)
        {
            Impedance = impedance;
        }
    }


    /// <summary>
    /// Hardware Abstration Layer. A HAL needs to exist for a particular configuration of 
    /// measurement devices. For example, a config that consists of just a QA401 might
    /// be named "QA401 Config". A config that consists of a QA401 and QA50 might be 
    /// named "QA401 + QA450 Config".
    /// 
    /// A configuration won't be cable of supporting all API calls. For example, if you
    /// have just a QA401, then it's not possible to be make current measurements. And 
    /// if you have a QA450, you can make current measurements, but you can't make a 
    /// voltage measurement
    /// </summary>
    interface iHal
    {
        /// <summary>
        /// Returns the friendly name of this hardware profile. This will convey to the user
        /// that the measurement devices are a QA401 or a QA401 + QA450, or any other collection
        /// of hardware.
        /// </summary>
        /// <returns></returns>
        string GetProfileName();

        // DUT power. This section controls the power provided to the DUT.

        /// <summary>
        /// Sets default options for the instrument
        /// </summary>
        void    DutSetDefault();

        void    DutSetPowerState(bool powerEnable);
        bool    DutGetPowerState();
        float   DutGetCurrent();
        void    DutSetVoltage(float voltage_V);
        float   DutGetVoltage();
        float   DutGetTemperature();

        // Programmable Load 
        void    LoadSetDefault();
        void    LoadSetImpedance(int impedance);
        int     LoadGetImpedance();
        float   LoadGetTemperature();

        // Audio Generators
        void AudioGenSetDefault();
        void AudioGenSetGen1(float freq_Hz, float ampLevel_dBV);
        void AudioGenSetGen1Enable(bool genEnable);
        void AudioGenSetGen2(float freq_Hz, float ampLevel_dBV);
        void AudioGenSetGen2Enable(bool genEnable);

        // Audio Input Attenuators
        void SetAtten(float attenLevel_dB);

        // Audio measurement control
        void RunSingle();
        Bitmap GetBitmap();
        void GetRMS(out float rmsLeft_dBV, out float rmsRight_dBV);
    }
}
