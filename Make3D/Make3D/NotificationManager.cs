using System;
using System.Collections.Generic;
using System.Windows;

namespace Make3D
{
    public delegate void RXMessage(object param);

    public static class NotificationManager
    {
        private static List<ObserverDef> observers = new List<ObserverDef>();

        public static void Notify(string name, object param)
        {
            bool adjust = false;
            List<ObserverDef> tmp = new List<ObserverDef>();
            foreach (ObserverDef df in observers)
            {
                tmp.Add(df);
            }

            for (int i = 0; i < tmp.Count; i++)
            {
                ObserverDef df = tmp[i];
                if (df.messageName == name)
                {
                    if (df.observer != null)
                    {
                        try
                        {
                            df.observer(param);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            df.alive = false;
                            adjust = true;
                        }
                    }
                }
            }

            if (adjust)
            {
                observers.Clear();
                for (int i = 0; i < tmp.Count; i++)
                {
                    ObserverDef df = tmp[i];
                    if (df.alive)
                    {
                        observers.Add(df);
                    }
                }
            }
        }

        public static void Subscribe(string name, RXMessage fnc)
        {
            ObserverDef df = new ObserverDef();
            df.subscriberName = "";
            df.messageName = name;
            df.observer = fnc;
            df.alive = true;
            observers.Add(df);
        }

        public static void Subscribe(string subscriberName,string eventName, RXMessage fnc)
        {
            ObserverDef df = new ObserverDef();
            df.subscriberName = subscriberName;
            df.messageName = eventName;
            df.observer = fnc;
            df.alive = true;
            observers.Add(df);
        }
        public static void Unsubscribe(String s)
        {
            List<ObserverDef> obs = new List<ObserverDef>();

            foreach (ObserverDef df in observers)
            {
                if (df.messageName.ToLower() != s.ToLower())
                {
                    obs.Add(df);
                }
            }
            observers = obs;
        }

        public static void ViewUnsubscribe(String s)
        {
            List<ObserverDef> obs = new List<ObserverDef>();

            foreach (ObserverDef df in observers)
            {
                if (df.subscriberName.ToLower() != s.ToLower())
                {
                    obs.Add(df);
                }
            }
            observers = obs;
        }
        private struct ObserverDef
        {
            public bool alive;
            public string subscriberName;
            public string messageName;
            public RXMessage observer;
        }
    }
}