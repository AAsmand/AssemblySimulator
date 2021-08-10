using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simulator
{
    public class Computer
    {
        public Computer()
        {
            LABEL = new Dictionary<string, string>();
            VALUE = new Dictionary<string, string>();
            ADDRESS = new Dictionary<string, string>();
        }
        public char[] AC { get; set; }
        public  char[] DR { get; set; }
        public  char[] IR { get; set; }
        public  char[] TR { get; set; }
        public  char[] AR { get; set; }
        public  char[] PC { get; set; }
        public  char[] INPR { get; set; }
        public  char[] OUTR { get; set; }
        public  char[] E { get; set; }
        public  char[] I { get; set; }
        public  char[] HEX { get; set; }
        public  char[] OPCode { get; set; }
        public  char[] OPdecode { get; set; }
        public  int SC { get; set; }
        public  int FGI { get; set; }
        public  int FGO { get; set; }
        public  int IEN { get; set; }
        public  int step_flag { get; set; }
        public  int hlt_flag { get; set; }
        public  int run_flag { get; set; }
        public  int org_int { get; set; }
        public  int counter { get; set; }
        public  string line { get; set; }
        public  Dictionary<string, string> LABEL { get; set; }
        public  Dictionary<string, string> VALUE { get; set; }
        public  Dictionary<string, string> ADDRESS { get; set; }
        public String[] strlist { get; set; }
        public  String[] strlist_2 { get; set; }
}



}
