using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    /// <summary>
    /// This interface handles the basics for all instruments
    /// </summary>
    interface IInstrument
    {
        void SetToDefaults();
        bool IsRunning();
        void LaunchApplication();
        bool ConnectToDevice(out string result);
        bool IsConnected();
        void CloseConnection();
    }
}
