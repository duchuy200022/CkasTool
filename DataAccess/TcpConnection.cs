using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CkasTool_MVVM.DataAccess
{
    public class TcpConnection
    {
        private static Socket _instance;
        public static Socket Instance { 
            get
            {
                if( _instance == null )
                {
                    _instance = new Socket(SocketType.Stream, ProtocolType.Tcp);
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
