using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    interface IComposite
    {
        void SetToDefaults();
        bool IsConnected();
        bool ConnectToDevices();
    }
}
