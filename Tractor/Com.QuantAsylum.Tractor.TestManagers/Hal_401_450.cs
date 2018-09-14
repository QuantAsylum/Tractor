using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Hardware;

namespace Tractor.Com.QuantAsylum.Tractor.TestManagers dsb
{

    class Hal_401_450 : iHal
    {
        /// <summary>
        /// Returns the friendly name of this hardware profile. This will convey to the user
        /// that the measurement devices are a QA401 or a QA401 + QA450, or any other collection
        /// of hardware.
        /// </summary>
        /// <returns></returns>
        public string GetProfileName()
        {
            return "QA401 + QA450";
        }

        // DUT power 
        public void DutSetDefault()
        {

        }

        public void DutSetPowerState(bool powerEnable)
        {
            QA450.SetDutPower(powerEnable);
        }

        public bool DutGetPowerState()
        {
            return QA450.GetDutPower();
        }


        public float DutGetCurrent()
        {
            return QA450.GetCurrent();
        }

        [MethodNotSupported]
        public void DutSetVoltage(float voltage_V)
        {
            throw new NotImplementedException();
        }

        [MethodNotSupported]
        public float DutGetVoltage()
        {
            throw new NotImplementedException();
        }

        [MethodNotSupported]
        public float DutGetTemperature()
        {
            throw new NotImplementedException();
        }

        // Programmable Load 
        public void LoadSetDefault()
        {

        }

        [MethodImpedanceSupported_Ohms(new int[] {8, 4, 0})]
        public void LoadSetImpedance(int impedance)
        {
            QA450.SetImpedance(impedance);
        }

        public int LoadGetImpedance()
        {
            return QA450.GetImpedance();
        }

        public void LoadSetLoadState(bool loadEnable)
        {

        }

        public bool LoadGetLoadState()
        {
            throw new NotImplementedException();
        }

        [MethodNotSupported]
        public float LoadGetTemperature()
        {
            throw new NotImplementedException();
        }


        // Audio Generators

        public void AudioGenSetDefault()
        {

        }

        public void AudioGenSetGen1(float freq_Hz, float ampLevel_dBV)
        {

        }

        public void AudioGenSetGen1Enable(bool genEnable)
        {

        }
        public void AudioGenSetGen2(float freq_Hz, float ampLevel_dBV)
        {

        }

        public void AudioGenSetGen2Enable(bool genEnable)
        {

        }

        // Audio Input Attenuators

        [MethodAttenSupported_dB(new int[] { 0, 20 })]
        public void SetAtten(float attenLevel_dB)
        {

        }

        // Audio measurement control
        public void RunSingle()
        {

        }
        public Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }
        public void GetRMS(out float rmsLeft_dBV, out float rmsRight_dBV)
        {
            throw new NotImplementedException();
        }
    }
}
