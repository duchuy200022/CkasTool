using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CkasTool_MVVM.DataAccess
{
    public class SerialConnection
    {
        private static SerialPort  _instance;
        public static SerialPort Instance
        {
            get
            { 
                if(_instance == null)
                {
                    _instance = new SerialPort();
                    _instance.BaudRate = 250000;
                    _instance.Parity = Parity.None;
                    _instance.DataBits = 8;
                    _instance.StopBits = StopBits.One;
                    _instance.Handshake = Handshake.None;
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }
}
