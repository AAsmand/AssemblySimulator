using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simulator.Models
{
    public class SimModel
    {
        public bool IsStarted { get; set; }
        public string code { get; set; }
        public string line { get; set; }
        public string pc { get; set; }
        public string ac { get; set; }
        public string ir { get; set; }
        public string decode { get; set; }
        public string dr { get; set; }
        public string ar { get; set; }
        public string e { get; set; }
        public string i { get; set; }
        public string ien { get; set; }
        public string fgo { get; set; }
        public string fgi { get; set; }
        public string sc { get; set; }
        public string outr { get; set; }
        public string inpr { get; set; }
        public string opcode { get; set; }

        public string message { get; set; }
    }
}
