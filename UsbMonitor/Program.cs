using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace UsbMonitor {
    public static class Program {
        public static void Main() {
            UsbMonitor um = new UsbMonitor();
            //um.TestStartAndStop(new string[0]);
            ServiceBase.Run(um);
        }
    }
}
