using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ShowImageApp.Models
{
    public class ConnectClientInfo
    {
       public Socket s { get; set; }
       public bool ReqFlag { get; set; }
    }
}
