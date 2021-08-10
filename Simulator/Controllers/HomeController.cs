using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simulator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simulator.Controllers
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            string temp = JsonSerializer.Serialize(value);
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
    public class HomeController : Controller
    {
        public SimModel model { get; set; }
        public Computer Comp { get; set; }
        public void FillSessionModels()
        {
            if (HttpContext.Session.Get<SimModel>("SimModel") == default)
            {
                HttpContext.Session.Set<SimModel>("SimModel", new SimModel());
            }
            if (HttpContext.Session.Get<Computer>("Comp") == default)
            {
                HttpContext.Session.Set<Computer>("Comp", new Computer());
            }
            model = HttpContext.Session.Get<SimModel>("SimModel");
            Comp = HttpContext.Session.Get<Computer>("Comp");
        }
        public void SetSessions()
        {
            HttpContext.Session.Set<SimModel>("SimModel", model);
            HttpContext.Session.Set<Computer>("Comp", Comp);
        }
        public IActionResult Index()
        {
            FillSessionModels();
            ViewBag.hlt = Comp.hlt_flag;
            ViewBag.Run = model.IsStarted;
            if (!string.IsNullOrEmpty(model.message))
            {
                ViewBag.message = model.message;
                model.message = "";
            }
            Comp.hlt_flag = 0;

            return View(model);
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Number Methods
        public string Dec_to_hex(string str)
        {
            if (str[0] != '-')
            {
                int decval = Convert.ToInt32(str);
                string hexval = (decval).ToString("X");
                return hexval;
            }
            else
            {
                short decval = Convert.ToInt16(str);
                Int16 d = decval;
                string s = Convert.ToString(d, 2);
                s = Bin_to_hex(s);
                return s;
            }

        }
        public string Hex_to_dec(string str)
        {
            int num;
            num = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            string decval = num.ToString();
            return decval;
        }
        public string Hex_to_bin(string hexvalue)
        {
            string binaryval = "";
            binaryval = Convert.ToString(Convert.ToInt32(hexvalue, 16), 2);
            return binaryval;
        }
        public string Bin_to_hex(string str)
        {
            string hexval = "";
            hexval = Convert.ToUInt16(str, 2).ToString("X4");
            return hexval;
        }
        #endregion

        #region String Methods
        string make3(string s)
        {
            if (s.Length == 0)
                s = "000" + s;
            if (s.Length == 1)
                s = "00" + s;
            if (s.Length == 2)
                s = "0" + s;
            return s;
        }
        string make4(string s)
        {
            if (s.Length == 0)
                s = "0000" + s;
            if (s.Length == 1)
                s = "000" + s;
            if (s.Length == 2)
                s = "00" + s;
            if (s.Length == 3)
                s = "0" + s;
            return s;
        }
        string make_string(string s)
        {
            if (s.Length == 0)
                s = "000" + s;
            if (s.Length == 1)
                s = "00" + s;
            if (s.Length == 2)
                s = "0" + s;
            return s;
        }
        string make16(string a)
        {
            int l = a.Length;
            string s = "";
            for (int i = 0; i < 16 - l; i++)
                s += "0";
            a = s + a;
            return a;
        }
        #endregion

        #region Tasks
        void Prepare()
        {
            Comp.E = "0".ToCharArray();
            String s = model.code;
            String str = model.code;

            String[] spearator = { " ", "\n" };
            Int32 count = 100000000;
            Comp.strlist = str.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries);//word by word

            String[] spearator_2 = { "\n" };
            Int32 count_2 = 100000000;
            Comp.strlist_2 = str.Split(spearator_2, count_2, StringSplitOptions.RemoveEmptyEntries);//line by line

            //start
            Comp.org_int = 0;
            Comp.PC = "0".ToCharArray();
            Comp.counter = 0;
        }
        void Exec_Command()
        {
            string strstr;
            int w;
            w = Comp.counter;
            try
            {
                while ((w < Comp.strlist_2.Length) && Comp.step_flag == 1)
                {
                    if (Comp.step_flag == 1)
                    {
                        int inst = 0, name = 0;
                        String[] spearator_3 = { " " };
                        Int32 count_3 = 100000000;
                        String[] ins = Comp.strlist_2[w].Split(spearator_3, count_3, StringSplitOptions.RemoveEmptyEntries);//check
                        if (ins[0] == "ORG")
                        {
                            Comp.org_int = Int32.Parse(Hex_to_dec(ins[1]));
                            Comp.PC = ins[1].ToCharArray();
                            Comp.counter += 1;
                            Comp.line = Comp.strlist_2[w];
                            Fill_Model();
                        }
                        else if (Comp.strlist_2[w].IndexOf(',') > -1)
                        {
                            inst = 1;
                            name = 2;
                        }
                        else if (Comp.strlist_2[w].IndexOf(',') == -1)
                        {
                            inst = 0;
                            name = 1;
                        }
                        //register reference instructions
                        if (ins[inst] == "CLA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            strstr = "0";
                            Comp.OPCode = "111".ToCharArray();
                            strstr = make4(strstr);
                            Comp.AC = strstr.ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "7800".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB11".ToCharArray();
                            Comp.AR = "800".ToCharArray();
                            Comp.IR = "7800".ToCharArray();
                            Fill_Model();
                        }
                        else if (ins[inst] == "CLE")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.OPCode = "111".ToCharArray();
                            strstr = "0";
                            Comp.E = strstr.ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "7400".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB10".ToCharArray();
                            Comp.AR = "400".ToCharArray();
                            Comp.IR = "7400".ToCharArray();
                            Fill_Model();
                        }
                        else if (ins[inst] == "CMA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            string s2 = new string(Comp.AC);
                            string SD = new string(Comp.AC);
                            string bin = Hex_to_bin(SD);
                            int k;
                            bin = make16(bin);
                            char[] temp = bin.ToCharArray();
                            for (k = 0; k < bin.Length; k++)
                                if (bin[k] == '0')
                                    temp[k] = '1';
                                else if (bin[k] == '1')
                                    temp[k] = '0';

                            string st = new string(temp);
                            string temp2 = Bin_to_hex(st);
                            temp2 = make4(temp2);
                            Comp.AC = temp2.ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "7200".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB9".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "200".ToCharArray();
                            Comp.IR = "7200".ToCharArray();
                            Fill_Model();
                        }
                        else if (ins[inst] == "CME")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            string s2 = new string(Comp.E);
                            int c = Int32.Parse(s2);
                            if (s2 == "0")
                                Comp.E = "1".ToCharArray();
                            if (s2 == "1")
                                Comp.E = "0".ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "7100".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB8".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "100".ToCharArray();
                            Comp.IR = "7100".ToCharArray();
                            Fill_Model();
                        }
                        else if (ins[inst] == "CIR")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            string s = new string(Comp.AC);
                            string bf = Hex_to_bin(s);
                            bf = make16(bf);
                            string eee = new string(Comp.E);
                            bf = eee + bf;


                            int ite;
                            char temporary;
                            char[] arr = bf.ToCharArray();
                            for (ite = 0; ite < 17 - 1; ite++)
                            {
                                temporary = arr[ite + 1];
                                arr[ite + 1] = arr[0];
                                arr[0] = temporary;
                            }
                            char[] chstr = new char[16];
                            for (int k = 1; k < 17; k++)
                                chstr[k - 1] = arr[k];
                            Comp.E = arr[0].ToString().ToCharArray();
                            string gk = new string(chstr);
                            Comp.AC = Bin_to_hex(gk).ToCharArray();

                            Comp.SC = 3;
                            Comp.HEX = "7080".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB7".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "080".ToCharArray();
                            Comp.IR = "7080".ToCharArray();
                            Fill_Model();
                        }
                        else if (ins[inst] == "CIL")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            string s = new string(Comp.AC);
                            string bf = Hex_to_bin(s);
                            bf = make16(bf);
                            string eee = new string(Comp.E);
                            bf = eee + bf;

                            int ite;
                            char temporary;
                            char[] arr = bf.ToCharArray();
                            for (int ii = 0; ii < 16; ii++)
                            {
                                for (ite = 0; ite < 17 - 1; ite++)
                                {
                                    temporary = arr[ite + 1];
                                    arr[ite + 1] = arr[0];
                                    arr[0] = temporary;
                                }
                            }

                            char[] chstr = new char[16];
                            for (int k = 1; k < 17; k++)
                                chstr[k - 1] = arr[k];
                            Comp.E = arr[0].ToString().ToCharArray();
                            string gk = new string(chstr);
                            Comp.AC = Bin_to_hex(gk).ToCharArray();


                            Comp.SC = 3;
                            Comp.HEX = "7040".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB6".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "040".ToCharArray();
                            Comp.IR = "7040".ToCharArray();
                            Fill_Model();
                        }
                        else if (ins[inst] == "INC")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            string tmp = new string(Comp.AC);
                            if (tmp == "ffff" || tmp == "FFFF")
                            {
                                Comp.AC = "1".ToCharArray();
                                Comp.E = "1".ToCharArray();
                            }
                            else
                            {
                                string s2 = new string(Comp.AC);
                                string s3 = Hex_to_dec(s2);
                                int c = Int32.Parse(s3);
                                int d = c + 1;
                                string hex = Dec_to_hex(d.ToString());
                                Comp.AC = hex.ToCharArray();
                            }
                            Comp.SC = 3;
                            Comp.HEX = "7020".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB5".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "020".ToCharArray();
                            Comp.IR = "7020".ToCharArray();
                            Fill_Model();
                        }
                        else if (ins[inst] == "SPA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.HEX = "7010".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB4".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "010".ToCharArray();
                            Comp.IR = "7010".ToCharArray();
                            string sq = new string(Comp.AC);
                            sq = Hex_to_bin(sq);
                            sq = make16(sq);
                            if (sq[0] == '0')
                            {
                                string s2 = new string(Comp.PC);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);
                                int d = c + 1;
                                s2 = d.ToString();
                                s2 = Dec_to_hex(s2);
                                Comp.PC = s2.ToCharArray();
                                Comp.counter += 1;

                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "SNA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.SC = 3;
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.HEX = "7008".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB3".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "008".ToCharArray();
                            Comp.IR = "7008".ToCharArray();
                            string sq = new string(Comp.AC);
                            sq = Hex_to_bin(sq);
                            sq = make16(sq);
                            if (sq[0] == '1')
                            {
                                string s2 = new string(Comp.PC);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);
                                int d = c + 1;
                                s2 = d.ToString();
                                s2 = Dec_to_hex(s2);
                                Comp.PC = s2.ToCharArray();
                                Comp.counter += 1;
                            }

                            Fill_Model();
                        }
                        else if (ins[inst] == "SZA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.SC = 3;
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.HEX = "7004".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB2".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "004".ToCharArray();
                            Comp.IR = "7004".ToCharArray();
                            string sq = new string(Comp.AC);
                            sq = Hex_to_bin(sq);
                            sq = make16(sq);
                            if (sq == "0000000000000000")
                            {
                                string s2 = new string(Comp.PC);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);
                                int d = c + 1;
                                s2 = d.ToString();
                                s2 = Dec_to_hex(s2);
                                Comp.PC = s2.ToCharArray();
                                Comp.counter += 1;
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "SZE")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.SC = 3;
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.HEX = "7002".ToCharArray();
                            Comp.I = "0".ToCharArray();
                            Comp.OPdecode = "rB1".ToCharArray();
                            Comp.OPCode = "111".ToCharArray();
                            Comp.AR = "002".ToCharArray();
                            Comp.IR = "7002".ToCharArray();
                            if (Comp.E[0] == '0')
                            {
                                string s2 = new string(Comp.PC);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);
                                int d = c + 1;
                                s2 = d.ToString();
                                s2 = Dec_to_hex(s2);
                                Comp.PC = s2.ToCharArray();
                                Comp.counter += 1;
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "HLT")
                        {
                            Comp.hlt_flag = 1;
                            string strhlt = "";

                            #region Reset Texts Requierd
                            model.line = String.Format("HLT");
                            model.pc = String.Format(strhlt);
                            model.ac = String.Format(strhlt);
                            model.ir = String.Format(strhlt);
                            model.decode = String.Format(strhlt);
                            model.dr = String.Format(strhlt);
                            model.ar = String.Format(strhlt);
                            model.e = String.Format(strhlt);
                            model.i = String.Format(strhlt);
                            model.ien = String.Format(strhlt);
                            model.fgo = String.Format(strhlt);
                            model.fgi = String.Format(strhlt);
                            model.sc = String.Format(strhlt);
                            model.outr = String.Format(strhlt);
                            model.opcode = String.Format(strhlt);
                            model.inpr = String.Format(strhlt);
                            #endregion
                            model.IsStarted = false;

                            Comp.AC = "".ToCharArray();
                            Comp.DR = "".ToCharArray();
                            Comp.IR = "".ToCharArray();
                            Comp.TR = "".ToCharArray();
                            Comp.AR = "".ToCharArray();
                            Comp.PC = "".ToCharArray();
                            Comp.E = "".ToCharArray();
                            Comp.I = "".ToCharArray();
                            Comp.HEX = "".ToCharArray();
                            Comp.OPCode = "".ToCharArray();
                            Comp.OPdecode = "".ToCharArray();
                            Comp.INPR = "".ToCharArray();
                            Comp.OUTR = "".ToCharArray();
                            Comp.counter = 0;
                            Comp.org_int = 0;
                            Comp.line = "";
                            Comp.SC = 0;
                            Comp.FGI = 0;
                            Comp.FGO = 0;
                            Comp.IEN = 0;
                            Comp.step_flag = 0;
                            Comp.run_flag = 0;

                            Comp.LABEL.Clear();
                            Comp.VALUE.Clear();
                            Comp.ADDRESS.Clear();

                            Array.Clear(Comp.strlist, 0, Comp.strlist.Length);
                            Array.Clear(Comp.strlist_2, 0, Comp.strlist_2.Length);


                            break;
                        }
                        //memory reference instructions
                        else if (ins[inst] == "AND")
                        {
                            Comp.line = Comp.strlist_2[w];
                            if ((ins.Length == 3 && ins[2] != "I") || (ins.Length == 2)) //directed
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 5;
                                Comp.OPCode = "000".ToCharArray();
                                Comp.OPdecode = "D0".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);
                                string str1 = "0" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();
                                Comp.DR = Comp.VALUE[ins[name]].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "0".ToCharArray();

                                char[] str_tmp = new char[16];
                                int k;
                                string ss1 = new string(Comp.DR);
                                string ss2 = new string(Comp.AC);

                                string str_dr = Hex_to_bin(ss1);
                                string str_ac = Hex_to_bin(ss2);
                                str_dr = make16(str_dr);
                                str_ac = make16(str_ac);
                                for (k = 0; k < 16; k++)
                                {
                                    if (str_dr[k] == '0' || str_ac[k] == '0')
                                        str_tmp[k] = '0';
                                    else if (str_dr[k] == '1' && str_ac[k] == '1')
                                        str_tmp[k] = '1';
                                }
                                string h = new string(str_tmp);
                                h = Bin_to_hex(h);
                                Comp.AC = h.ToCharArray();
                            }
                            else if ((ins.Length == 4) || (ins.Length == 3 && ins[2] == "I")) //indirected
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 5;
                                Comp.OPCode = "000".ToCharArray();
                                Comp.OPdecode = "D0".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "8" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.VALUE[ins[name]].ToCharArray();
                                string xs = new string(Comp.AR);
                                Comp.DR = Comp.ADDRESS[xs].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "1".ToCharArray();

                                char[] str_tmp = new char[16];
                                int k;
                                string ss1 = new string(Comp.DR);
                                string ss2 = new string(Comp.AC);

                                string str_dr = Hex_to_bin(ss1);
                                string str_ac = Hex_to_bin(ss2);
                                str_dr = make16(str_dr);
                                str_ac = make16(str_ac);
                                for (k = 0; k < 16; k++)
                                {
                                    if (str_dr[k] == '0' || str_ac[k] == '0')
                                        str_tmp[k] = '0';
                                    else if (str_dr[k] == '1' && str_ac[k] == '1')
                                        str_tmp[k] = '1';
                                }
                                string h = new string(str_tmp);
                                h = Bin_to_hex(h);
                                Comp.AC = h.ToCharArray();
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "ADD")
                        {
                            Comp.line = Comp.strlist_2[w];
                            if ((ins.Length == 3 && ins[2] != "I") || (ins.Length == 2))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 5;
                                Comp.OPCode = "001".ToCharArray();
                                Comp.OPdecode = "D1".ToCharArray();
                                string str1 = "1" + Comp.LABEL[ins[name]];
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();
                                Comp.DR = Comp.VALUE[ins[name]].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "0".ToCharArray();

                                string s2 = new string(Comp.AC);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);
                                int ac = c;
                                string s3 = new string(Comp.DR);
                                s3 = Hex_to_dec(s3);
                                int d = Int32.Parse(s3);
                                int dr = d;

                                ac = ac + dr;
                                int e = 0;
                                if (ac > 65535)
                                {
                                    e = 1;
                                    ac -= 65535;
                                    Comp.E = e.ToString().ToCharArray();
                                }
                                Comp.AC = Dec_to_hex(ac.ToString()).ToCharArray();
                                //Comp.E = e.ToString().ToCharArray();
                            }
                            if ((ins.Length == 4) || (ins.Length == 3 && ins[2] == "I"))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 5;
                                Comp.OPCode = "001".ToCharArray();
                                Comp.OPdecode = "D1".ToCharArray();
                                string str1 = "9" + Comp.LABEL[ins[name]];
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.VALUE[ins[name]].ToCharArray();
                                string fq = new string(Comp.AR);

                                Comp.DR = Comp.ADDRESS[fq].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "1".ToCharArray();

                                string s2 = new string(Comp.AC);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);
                                int ac = c;
                                string s3 = new string(Comp.DR);
                                s3 = Hex_to_dec(s3);
                                int d = Int32.Parse(s3);
                                int dr = d;

                                ac = ac + dr;
                                int e = 0;
                                if (ac > 65535)
                                {
                                    e = 1;
                                    ac -= 65535;
                                    Comp.E = e.ToString().ToCharArray();
                                }
                                Comp.AC = Dec_to_hex(ac.ToString()).ToCharArray();
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "LDA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            if ((ins.Length == 3 && ins[2] != "I") || (ins.Length == 2))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 5;
                                Comp.OPCode = "010".ToCharArray();
                                Comp.OPdecode = "D2".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "2" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();
                                Comp.DR = Comp.VALUE[ins[name]].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "0".ToCharArray();
                                Comp.AC = Comp.DR;
                            }
                            if ((ins.Length == 4) || (ins.Length == 3 && ins[2] == "I"))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 5;
                                Comp.OPCode = "010".ToCharArray();
                                Comp.OPdecode = "D2".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "A" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.VALUE[ins[name]].ToCharArray();
                                string fq = new string(Comp.AR);
                                Comp.DR = Comp.ADDRESS[fq].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "1".ToCharArray();
                                Comp.AC = Comp.DR;
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "STA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            if ((ins.Length == 3 && ins[2] != "I") || (ins.Length == 2))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 4;
                                Comp.OPCode = "011".ToCharArray();
                                Comp.OPdecode = "D3".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "B" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();
                                string acc = new string(Comp.AC);
                                Comp.VALUE[ins[name]] = acc;//////
                                Comp.I = "0".ToCharArray();
                            }
                            else if ((ins.Length == 4) || (ins.Length == 3 && ins[2] == "I"))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 4;
                                Comp.OPCode = "011".ToCharArray();
                                Comp.OPdecode = "D3".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "C" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.AR = Comp.VALUE[ins[name]].ToCharArray();
                                string fq = new string(Comp.AR);
                                string h = new string(Comp.AC);
                                Comp.ADDRESS[fq] = h;
                                Comp.I = "1".ToCharArray();
                                string myKey = Comp.LABEL.FirstOrDefault(x => x.Value == fq).Key;
                                Comp.VALUE[myKey] = h;
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "BUN")
                        {
                            Comp.line = Comp.strlist_2[w];
                            if ((ins.Length == 3 && ins[2] != "I") || (ins.Length == 2))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();

                                Comp.counter = Comp.counter + 1;

                                Comp.SC = 4;
                                Comp.OPCode = "100".ToCharArray();
                                Comp.OPdecode = "D4".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "4" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();
                                //Comp.DR = Comp.VALUE[ins[name]].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "0".ToCharArray();
                                if (Comp.org_int > 0)
                                {
                                    string sss = new string(Comp.AR);
                                    sss = Hex_to_dec(sss);
                                    int c = Int32.Parse(sss);

                                    w = c - Comp.org_int + 1;
                                    Comp.counter = w;
                                    Comp.PC = (Comp.AR);
                                }
                                else
                                {
                                    string sss = new string(Comp.AR);
                                    sss = Hex_to_dec(sss);
                                    int c = Int32.Parse(sss);
                                    w = c;
                                    Comp.counter = w;
                                    Comp.PC = Comp.AR;
                                }
                            }
                            else if ((ins.Length == 4) || (ins.Length == 3 && ins[2] == "I"))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 4;
                                Comp.OPCode = "100".ToCharArray();
                                Comp.OPdecode = "D4".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "C" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.VALUE[ins[name]].ToCharArray();
                                //Comp.DR = Comp.ADDRESS[Comp.AR.ToString()].ToCharArray();
                                Comp.IR = str1.ToCharArray();
                                Comp.I = "1".ToCharArray();
                                if (Comp.org_int > 0)
                                {
                                    string sss = new string(Comp.AR);
                                    sss = Hex_to_dec(sss);
                                    int c = Int32.Parse(sss);

                                    w = c - Comp.org_int + 1;
                                    Comp.counter = w;
                                    int zx = c + 1;//Comp.org_int;
                                    sss = Dec_to_hex(sss);
                                    Comp.PC = sss.ToCharArray();
                                }
                                else
                                {
                                    string sss = new string(Comp.AR);
                                    sss = Hex_to_dec(sss);
                                    int c = Int32.Parse(sss);

                                    //w = c - Comp.org_int + 1;
                                    w = c;
                                    Comp.counter = w;
                                    Comp.PC = (Comp.AR);
                                }
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "BSA")
                        {
                            Comp.line = Comp.strlist_2[w];
                            if ((ins.Length == 3 && ins[2] != "I") || (ins.Length == 2))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 4;
                                Comp.OPCode = "101".ToCharArray();
                                Comp.OPdecode = "D5".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "5" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.IR = str1.ToCharArray();

                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();

                                string s2 = new string(Comp.AR);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);

                                int ar = c + 1;
                                Comp.AR = ar.ToString().ToCharArray();
                                string s33 = new string(Comp.PC);

                                Comp.VALUE[ins[name]] = s33;
                                Comp.PC = Comp.AR;
                                if (Comp.org_int > 0)
                                {
                                    w = ar - Comp.org_int + 1;
                                    Comp.counter = w;
                                }
                                else
                                {
                                    w = ar;
                                    Comp.counter = w;
                                }
                                Comp.DR = Comp.VALUE[ins[name]].ToCharArray();
                                Comp.I = "0".ToCharArray();
                            }
                            else if ((ins.Length == 4) || (ins.Length == 3 && ins[2] == "I"))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 4;
                                Comp.OPCode = "101".ToCharArray();
                                Comp.OPdecode = "D5".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "5" + fs;
                                Comp.HEX = str1.ToCharArray();
                                Comp.IR = str1.ToCharArray();

                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();

                                string s2 = new string(Comp.AR);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);

                                int ar = c + 1;
                                Comp.AR = ar.ToString().ToCharArray();
                                string s33 = new string(Comp.PC);

                                Comp.VALUE[ins[name]] = s33;
                                Comp.PC = Comp.AR;
                                if (Comp.org_int > 0)
                                {
                                    w = ar - Comp.org_int + 1;
                                    Comp.counter = w;
                                }
                                else
                                {
                                    w = ar;
                                    Comp.counter = w;
                                }
                                Comp.DR = Comp.VALUE[ins[name]].ToCharArray();
                                Comp.I = "0".ToCharArray();
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "ISZ")
                        {
                            Comp.line = Comp.strlist_2[w];
                            if ((ins.Length == 3 && ins[2] != "I") || (ins.Length == 2))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 4;
                                Comp.OPCode = "110".ToCharArray();
                                Comp.OPdecode = "D6".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "6" + fs;
                                Comp.IR = str1.ToCharArray();
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.LABEL[ins[name]].ToCharArray();
                                Comp.DR = Comp.VALUE[ins[name]].ToCharArray();

                                string s2 = new string(Comp.DR);
                                s2 = Hex_to_dec(s2);
                                int c = Int32.Parse(s2);

                                int dr = c;
                                dr += 1;
                                string s4 = dr.ToString();
                                s4 = Dec_to_hex(s4);
                                if (s4 == "10000")
                                    s4 = "0000";
                                Comp.DR = s4.ToCharArray();
                                Comp.VALUE[ins[name]] = s4.ToString();
                                Comp.ADDRESS[Comp.LABEL[ins[name]]] = s4.ToString();
                                if (dr == 65536)
                                {
                                    string s3 = new string(Comp.PC);
                                    s3 = Hex_to_dec(s3);
                                    int bbb = Int32.Parse(s3);

                                    int aa = bbb + 1;
                                    s3 = aa.ToString();
                                    s3 = Dec_to_hex(s3);
                                    Comp.PC = s3.ToCharArray();
                                    w++;
                                    w++;
                                    Comp.counter = w;
                                }
                                Comp.I = "0".ToCharArray();
                            }
                            if ((ins.Length == 4) || (ins.Length == 3 && ins[2] == "I"))
                            {
                                string s1 = new string(Comp.PC);
                                s1 = Hex_to_dec(s1);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                s1 = a.ToString();
                                s1 = Dec_to_hex(s1);
                                Comp.PC = s1.ToCharArray();
                                Comp.counter = Comp.counter + 1;
                                Comp.SC = 4;
                                Comp.OPCode = "110".ToCharArray();
                                Comp.OPdecode = "D6".ToCharArray();
                                string fs = make_string(Comp.LABEL[ins[name]]);

                                string str1 = "E" + fs;
                                Comp.IR = str1.ToCharArray();
                                Comp.HEX = str1.ToCharArray();
                                Comp.AR = Comp.VALUE[ins[name]].ToCharArray();
                                string ssss = new string(Comp.AR);
                                Comp.DR = Comp.ADDRESS[ssss].ToCharArray();

                                string ss = new string(Comp.DR);
                                ss = Hex_to_dec(ss);
                                int c = Int32.Parse(ss);

                                int dr = c;
                                dr += 1;
                                ss = dr.ToString();
                                ss = Dec_to_hex(ss);
                                if (ss == "10000")
                                    ss = "0000";
                                Comp.DR = ss.ToCharArray();
                                string sar = new string(Comp.AR);
                                Comp.ADDRESS[sar] = ss;
                                if (dr == 65536)
                                {
                                    string s3 = new string(Comp.PC);
                                    s3 = Hex_to_dec(s3);
                                    int d = Int32.Parse(s3);

                                    int aa = d + 1;
                                    s3 = aa.ToString();
                                    s3 = Dec_to_hex(s3);
                                    Comp.PC = s3.ToCharArray();
                                    w++;
                                    w++;
                                    Comp.counter = w;
                                }
                                Comp.I = "1".ToCharArray();
                            }
                            Fill_Model();
                        }
                        //IO reference instruction
                        else if (ins[inst] == "INP")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.OPCode = "111".ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "F800".ToCharArray();
                            Comp.I = "1".ToCharArray();
                            Comp.OPdecode = "pB11".ToCharArray();
                            Comp.AR = "800".ToCharArray();
                            Comp.IR = "F800".ToCharArray();
                            for (int q = 0; q < 8; q++)
                            {
                                Comp.AC[q] = Comp.INPR[q];
                            }
                            Comp.FGI = 0;
                            Fill_Model();
                        }
                        else if (ins[inst] == "OUT")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.OPCode = "111".ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "F400".ToCharArray();
                            Comp.I = "1".ToCharArray();
                            Comp.OPdecode = "pB10".ToCharArray();
                            Comp.AR = "400".ToCharArray();
                            Comp.IR = "F400".ToCharArray();
                            for (int q = 0; q < 8; q++)
                            {
                                Comp.OUTR[q] = Comp.AC[q];
                            }
                            Comp.FGO = 0;
                            Fill_Model();
                        }
                        else if (ins[inst] == "SKI")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.OPCode = "111".ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "F200".ToCharArray();
                            Comp.I = "1".ToCharArray();
                            Comp.OPdecode = "pB9".ToCharArray();
                            Comp.AR = "200".ToCharArray();
                            Comp.IR = "F200".ToCharArray();
                            if (Comp.FGI == 1)
                            {
                                w++;
                                string ss = new string(Comp.PC);
                                int c = Int32.Parse(ss);
                                int aa = c + 1;
                                Comp.PC = aa.ToString().ToCharArray();
                                Comp.counter = w;
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "SKO")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.OPCode = "111".ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "F100".ToCharArray();
                            Comp.I = "1".ToCharArray();
                            Comp.OPdecode = "pB8".ToCharArray();
                            Comp.AR = "100".ToCharArray();
                            Comp.IR = "F100".ToCharArray();
                            if (Comp.FGO == 1)
                            {
                                w++;
                                string s2 = new string(Comp.PC);
                                int c = Int32.Parse(s2);
                                int aa = c + 1;
                                Comp.PC = aa.ToString().ToCharArray();
                                Comp.counter = w;
                            }
                            Fill_Model();
                        }
                        else if (ins[inst] == "ION")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.OPCode = "111".ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "F080".ToCharArray();
                            Comp.I = "1".ToCharArray();
                            Comp.OPdecode = "pB7".ToCharArray();
                            Comp.AR = "80".ToCharArray();
                            Comp.IR = "F080".ToCharArray();
                            Comp.IEN = 1;
                            Fill_Model();
                        }
                        else if (ins[inst] == "IOF")
                        {
                            Comp.line = Comp.strlist_2[w];
                            string s1 = new string(Comp.PC);
                            s1 = Hex_to_dec(s1);
                            int b = Int32.Parse(s1);
                            int a = b + 1;
                            s1 = a.ToString();
                            s1 = Dec_to_hex(s1);
                            Comp.PC = s1.ToCharArray();
                            Comp.counter = Comp.counter + 1;
                            Comp.OPCode = "111".ToCharArray();
                            Comp.SC = 3;
                            Comp.HEX = "F040".ToCharArray();
                            Comp.I = "1".ToCharArray();
                            Comp.OPdecode = "pB6".ToCharArray();
                            Comp.AR = "40".ToCharArray();
                            Comp.IR = "F040".ToCharArray();
                            Comp.IEN = 0;
                            Fill_Model();
                        }
                        else if (Comp.hlt_flag == 0)
                        {
                            Comp.line = Comp.strlist_2[w];
                            String[] spearator_4 = { " ", "," };
                            Int32 count_4 = 100000000;
                            String[] lbl = Comp.strlist_2[w].Split(spearator_4, count_4, StringSplitOptions.RemoveEmptyEntries);//check
                            if (Comp.LABEL.ContainsKey(lbl[0]))
                            {
                                string s1 = new string(Comp.PC);
                                int b = Int32.Parse(s1);
                                int a = b + 1;
                                Comp.PC = a.ToString().ToCharArray();
                                Comp.counter += 1;
                                Fill_Model();
                            }
                            else if(ins[0] != "ORG")
                            {
                                model.message = "فرمت کد ها صحیح نمیباشد پس از اصلاح کد ها مجددا تلاش نمایید";
                            }
                        }
                        Comp.step_flag = 0;
                    }
                }
            }
            catch (Exception)
            {
                model.message = "فرمت کد ها صحیح نمیباشد پس از اصلاح کد ها مجددا تلاش نمایید  !";
            }
        }
        void Exec_All()
        {
            while (Comp.hlt_flag == 0 && Comp.run_flag == 1)
            {
                Comp.step_flag = 1;
                Exec_Command();
            }
        }
        void Fill_Model()
        {
            string pcs = new string(Comp.PC);
            pcs = make3(pcs);
            string acs = new string(Comp.AC);
            acs = make4(acs);
            string irs = new string(Comp.IR);
            irs = make4(irs);
            string opdecodes = new string(Comp.OPdecode);
            string drs = new string(Comp.DR);
            drs = make4(drs);
            string ars = new string(Comp.AR);
            ars = make3(ars);
            string es = new string(Comp.E);
            string iss = new string(Comp.I);
            string inprs = new string(Comp.INPR);
            string outrs = new string(Comp.OUTR);
            string opcodes = new string(Comp.OPCode);

            #region Fill Text boxs Requierd
            model.line = String.Format(Comp.line);
            model.pc = String.Format(pcs);
            model.ac = String.Format(acs);
            model.ir = String.Format(irs);
            model.decode = String.Format(opdecodes);
            model.dr = String.Format(drs);
            model.ar = String.Format(ars);
            model.e = String.Format(es);
            model.i = String.Format(iss);
            model.ien = String.Format(Comp.IEN.ToString());
            model.fgo = String.Format(Comp.FGO.ToString());
            model.fgi = String.Format(Comp.FGI.ToString());
            model.sc = String.Format(Comp.SC.ToString());
            model.outr = String.Format(outrs);
            model.opcode = String.Format(opcodes);
            model.inpr = String.Format(inprs);
            #endregion

        }
        #endregion

        [HttpPost]
        public IActionResult Compile(string m)
        {
            Comp = new Computer();
            model = new SimModel();
            ///////////////////////////////////////////////////////////////////////
            if (string.IsNullOrEmpty(m))
            {
                model.message = "ابتدا کد اسمبلی خود را نوشته و سپس بر روی دکمه کامپایل کلیک نمایید ";
                SetSessions();
                return RedirectToAction("Index");
            }
            model.code = m;
            model.code = model.code.Replace("\r", "");
            String s = model.code;
            String str = model.code;
            String[] spearator = { " ", "\n" };
            Int32 count = 100000000;
            String[] strlist = str.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries);//word by word

            String[] spearator_2 = { "\n" };
            Int32 count_2 = 100000000;
            string org;
            int org_int = 0;
            if (strlist[0] == "ORG")
            {
                org = strlist[1];
                org_int = int.Parse(org, System.Globalization.NumberStyles.HexNumber);
            }
            String[] strlist_2 = str.Split(spearator_2, count_2, StringSplitOptions.RemoveEmptyEntries);//line by line
            int flag = 0, emp = 0;
            //prepare dictionarys
            for (int i = 0; i < strlist_2.Length; i++)
            {
                String[] spearator_g = { " " };
                Int32 count_g = 100000000;
                String[] strlist_g = strlist_2[i].Split(spearator_g, count_g, StringSplitOptions.RemoveEmptyEntries);

                if (strlist_g[0] == "ORG")
                {
                    if (flag == 0)
                        flag = 1;
                    else
                        emp = i;
                    org = strlist_g[1];
                    org_int = int.Parse(org, System.Globalization.NumberStyles.HexNumber);
                }
                if (strlist_2[i].IndexOf(',') > -1)
                {
                    String[] spearator_l = { "," };
                    int count_l = 100000000;
                    String[] strlist_l = strlist_2[i].Split(spearator_l, count_l, StringSplitOptions.RemoveEmptyEntries);
                    if (org_int > 0)
                        Comp.LABEL.Add(strlist_l[0], (org_int + i - 1).ToString("X")); //label name , label address
                    else
                        Comp.LABEL.Add(strlist_l[0], (i).ToString("X")); //label name , label address

                    if (strlist_g.Length > 2)
                    {
                        string hexval = "0";
                        if (strlist_g[1] == "HEX")
                        {
                            hexval = strlist_g[2];
                            Comp.VALUE.Add(strlist_l[0], hexval);  //label name , decimal value
                        }
                        if (strlist_g[1] == "DEC")
                        {
                            hexval = Dec_to_hex(strlist_g[2]);
                            Comp.VALUE.Add(strlist_l[0], hexval);  //label name , decimal value
                        }
                        if (org_int > 0)
                            Comp.ADDRESS.Add((org_int + i - 1).ToString("X"), hexval);   //address   value
                        else
                            Comp.ADDRESS.Add((i).ToString("X"), hexval);   //address   value
                    }

                }
                if (strlist_g[0] == "DEC")
                {
                    string hexadecimal = Dec_to_hex(strlist_g[1]);
                    if (org_int > 0 && flag == 0)
                        Comp.ADDRESS.Add((org_int + i - 1).ToString("X"), hexadecimal);   //address   value
                    else if (org_int == 0 && flag == 0)
                        Comp.ADDRESS.Add((i).ToString("X"), hexadecimal);   //address   value
                    else if (org_int > 0 && flag == 1)
                        Comp.ADDRESS.Add((org_int + i - 1 - emp).ToString("X"), hexadecimal);   //address   value

                }
                if (strlist_g[0] == "HEX")
                {
                    if (org_int > 0)
                        Comp.ADDRESS.Add((org_int + i - 1).ToString("X"), strlist_g[1]);   //address   value
                    else
                        Comp.ADDRESS.Add((i).ToString("X"), strlist_g[1]);
                }
            }


            Comp.hlt_flag = 0;
            Comp.run_flag = 0;
            Prepare();
            Comp.step_flag = 1;
            model.IsStarted = true;
            Exec_Command();
            SetSessions();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Stop()
        {
            FillSessionModels();
            string strhlt = "";
            model.code = String.Format(strhlt);
            model.line = String.Format(strhlt);
            model.pc = String.Format(strhlt);
            model.ac = String.Format(strhlt);
            model.ir = String.Format(strhlt);
            model.decode = String.Format(strhlt);
            model.dr = String.Format(strhlt);
            model.ar = String.Format(strhlt);
            model.e = String.Format(strhlt);
            model.i = String.Format(strhlt);
            model.ien = String.Format(strhlt);
            model.fgo = String.Format(strhlt);
            model.fgi = String.Format(strhlt);
            model.sc = String.Format(strhlt);
            model.outr = String.Format(strhlt);
            model.opcode = String.Format(strhlt);
            model.inpr = String.Format(strhlt);
            model.IsStarted = false;

            Comp.AC = "".ToCharArray();
            Comp.DR = "".ToCharArray();
            Comp.IR = "".ToCharArray();
            Comp.TR = "".ToCharArray();
            Comp.AR = "".ToCharArray();
            Comp.PC = "".ToCharArray();
            Comp.E = "".ToCharArray();
            Comp.I = "".ToCharArray();
            Comp.HEX = "".ToCharArray();
            Comp.OPCode = "".ToCharArray();
            Comp.OPdecode = "".ToCharArray();
            Comp.INPR = "".ToCharArray();
            Comp.OUTR = "".ToCharArray();
            Comp.counter = 0;
            Comp.org_int = 0;
            Comp.line = "";
            Comp.SC = 0;
            Comp.FGI = 0;
            Comp.FGO = 0;
            Comp.IEN = 0;
            Comp.step_flag = 0;
            Comp.hlt_flag = 0;
            Comp.run_flag = 0;

            Comp.LABEL.Clear();
            Comp.VALUE.Clear();
            Comp.ADDRESS.Clear();

            Array.Clear(Comp.strlist, 0, Comp.strlist.Length);
            Array.Clear(Comp.strlist_2, 0, Comp.strlist_2.Length);
            SetSessions();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Next()
        {
            FillSessionModels();
            Comp.step_flag = 1;
            Exec_Command();
            SetSessions();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult All()
        {
            FillSessionModels();
            Comp.run_flag = 1;
            Exec_All();
            SetSessions();
            return RedirectToAction("Index");
        }
    }
}
