using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.TestManagers
{
    interface IVoltMeter
    {
        float GetVoltage(int channel);
    }
}
