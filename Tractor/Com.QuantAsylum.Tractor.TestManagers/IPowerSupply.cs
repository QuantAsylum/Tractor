using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    interface IPowerSupply
    {
        /// <summary>
        /// Gets the power state
        /// </summary>
        /// <returns></returns>
        bool GetSupplyState();
        /// <summary>
        /// Sets the power state. If 'powerEnable' is true, then the power to the DUT is enabled
        /// </summary>
        /// <param name="powerEnable"></param>
        void SetSupplyState(bool powerEnable);

        void SetSupplyVoltage(float volts);

        float GetSupplyVoltage();
    }
}
