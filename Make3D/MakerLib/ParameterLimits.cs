using System;
using System.Collections.Generic;

namespace MakerLib
{
    public class ParamLimit
    {
        public string Low;
        public string High;
    }

    public class ParamLimits
    {
        private Dictionary<String, ParamLimit> Entries;

        public ParamLimits()
        {
            Entries = new Dictionary<string, ParamLimit>();
        }

        public void AddLimit(string n, string l, string h)
        {
            ParamLimit pl = new ParamLimit();
            pl.Low = l;
            pl.High = h;
            Entries[n] = pl;
        }

        public void AddLimit(string n, double l, double h)
        {
            ParamLimit pl = new ParamLimit();
            pl.Low = l.ToString();
            pl.High = h.ToString();
            Entries[n.ToLower()] = pl;
        }

        public void AddLimit(string n, int l, int h)
        {
            ParamLimit pl = new ParamLimit();
            pl.Low = l.ToString();
            pl.High = h.ToString();
            Entries[n.ToLower()] = pl;
        }

        public bool Check(String n, double v)
        {
            bool res = false;
            if (Entries.ContainsKey(n.ToLower()))
            {
                ParamLimit pl = Entries[n.ToLower()];
                if (v >= Convert.ToDouble(pl.Low))
                {
                    if (v <= Convert.ToDouble(pl.High))
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        public bool Check(String n, int v)
        {
            bool res = false;
            if (Entries.ContainsKey(n))
            {
                ParamLimit pl = Entries[n];
                if (v >= Convert.ToInt32(pl.Low))
                {
                    if (v <= Convert.ToInt32(pl.High))
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        public void GetLimits(string n, out string l, out string h)
        {
            l = "";
            h = "";
            if (Entries.ContainsKey(n))
            {
                ParamLimit pl = Entries[n];
                l = pl.Low;
                h = pl.High;
            }
        }

        internal ParamLimit GetLimits(string n)
        {
            ParamLimit res = null;
            if (Entries.ContainsKey(n))
            {
                res = Entries[n];
            }
            return res;
        }
    }
}