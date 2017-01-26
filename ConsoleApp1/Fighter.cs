using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Fighter
    {
        public static int fighters =0 ;
        public int id;
        public Socket socket;
        public string pokemon="";
        public bool ready=false;
    }
}
