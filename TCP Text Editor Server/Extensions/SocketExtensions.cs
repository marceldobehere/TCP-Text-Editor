using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.Extensions
{
    public static class SocketExtensions
    {
        public static bool IsAlive(this Socket socket)
        {
            return !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
        }
    }
}
