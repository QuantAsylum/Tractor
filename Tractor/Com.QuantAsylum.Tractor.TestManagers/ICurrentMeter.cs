using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    interface ICurrentMeter
    {

        /// <summary>
        /// Returns the DUT current.
        /// </summary>
        /// <param name="averages"></param>
        /// <returns></returns>
        float GetDutCurrent(int averages = 1);
    }
}
