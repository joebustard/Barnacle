// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Barnacle
{
    public delegate void RXMessage(object param);

    public delegate Task RXMessageTask(object param);

    public static class NotificationManager
    {
        private static bool idleMode = false;

        private static int idleTimeSeconds = 5 * 60;

        private static List<ObserverDef> observers = new List<ObserverDef>();

        // if no notifications come through for a specified time
        // then an IdleTimer notification is sent to anyone
        // who has subscribed to it
        private static DispatcherTimer timer = null;

        public static bool IdleMode
        {
            get
            {
                return idleMode;
            }
            set
            {
                if (value != idleMode)
                {
                    StopIdleTimer();
                    idleMode = value;
                    StartIdleTimer();
                }
            }
        }

        public static int IdleTimeSeconds
        {
            get { return idleTimeSeconds; }
            set
            {
                if (idleTimeSeconds != value)
                {
                    StopIdleTimer();
                    idleTimeSeconds = value;
                    StartIdleTimer();
                }
            }
        }

        public static void Notify(string name, object param)
        {
            StopIdleTimer();
            SendNotification(name, param);
            StartIdleTimer();
        }

        public static async Task NotifyTask(string name, object param)
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
                    if (df.observerTask != null)
                    {
                        try
                        {
                            await df.observerTask(param);
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
            Subscribe("", name, fnc);
        }

        public static void Subscribe(string subscriberName, string eventName, RXMessage fnc)
        {
            ObserverDef df = new ObserverDef();
            df.subscriberName = subscriberName;
            df.messageName = eventName;
            df.observer = fnc;
            df.alive = true;
            observers.Add(df);
        }

        public static void SubscribeTask(string subscriberName, string eventName, RXMessageTask fnc)
        {
            ObserverDef df = new ObserverDef();
            df.subscriberName = subscriberName;
            df.messageName = eventName;
            df.observerTask = fnc;
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

        private static void SendNotification(string name, object param)
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

        private static void StartIdleTimer()
        {
            if (IdleMode)
            {
                if (timer == null)
                {
                    timer = new DispatcherTimer();
                    timer.Tick += Timer_Tick;
                }
                TimeSpan ts = new TimeSpan(0, 0, 0, IdleTimeSeconds);
                timer.Interval = ts;
                timer.Start();
            }
        }

        private static void StopIdleTimer()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            SendNotification("IdleTimer", null);
        }

        private struct ObserverDef
        {
            public bool alive;
            public string messageName;
            public RXMessage observer;
            public RXMessageTask observerTask;
            public string subscriberName;
        }
    }
}