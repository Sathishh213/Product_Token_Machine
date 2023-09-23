using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace VendingMachine.Helpers
{
    public static class Hardware
    {
        //public static string[] duo = SerialPort.GetPortNames();
        public static string vending_port = "COM7";// replace Com port no
        public static int vending_BaudRate = 9600;
        public static int vending_DataBits = 8;
        public static Parity vending_Parity = Parity.None;
        public static StopBits vending_StopBits = StopBits.Two;

    }
}
