using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    interface IProgrammableLoad
    {
        /// <summary>
        /// Returns a list of supported impedances. 0 ohms = open
        /// </summary>
        /// <returns></returns>
        int[] GetSupportedImpedances();

        /// <summary>
        /// Sets the desired impedance. The value must be in the list of supported impedances
        /// </summary>
        /// <param name="impedance"></param>
        void SetImpedance(int impedance);
        
        /// <summary>
        /// Returns the currently active impedance
        /// </summary>
        /// <returns></returns>
        int GetImpedance();

        /// <summary>
        /// Returns the load temperature
        /// </summary>
        /// <returns></returns>
        float GetLoadTemperature();

    }
}
