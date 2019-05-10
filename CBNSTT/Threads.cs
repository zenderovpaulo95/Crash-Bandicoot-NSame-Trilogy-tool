using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBNSTT
{
    public delegate void SendMessage(string message);
    public delegate void SendProgress(int progress);

    public class Threads
    {
        public event SendMessage SendMes;
        public event SendProgress SendProg;
    }
}
