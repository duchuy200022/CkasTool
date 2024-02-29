using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CkasTool_MVVM.Mediator
{
    public class Event
    {
        public static string TcpError = "Tcp Error";
        public static string SerialError = "Serial Error";
        public static string SendDataSuccess = "SendDataSuccess";
        public static string SendDataError = "SendDataError";
    }
}
