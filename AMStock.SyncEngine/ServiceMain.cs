using System;
using System.ServiceProcess;
using System.Timers;

namespace AMStock.SyncEngine
{
    partial class ServiceMain : ServiceBase
    {
        private const int ObserverTimerDelay = 1000; //1 second;
        public ServiceMain()
        {
            InitializeComponent();
            ObserverTimer = new System.Timers.Timer(ObserverTimerDelay);
        }

        public void DebugStart()
        {
            ObserverTimer_Elapsed(null, null);
        }

        protected override void OnStart(string[] args)
        {
            ObserverTimer.Enabled = true;
        }

        protected override void OnStop()
        {
            ObserverTimer.Enabled = false;
        }

        private void ObserverTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ObserverTimer.Enabled = false;
            try
            {
                var sync = new SyncTask();
                sync.Start();
            }
            catch (Exception ex)
            {
                Stop();
                ServiceLog.Log(ex.Message);
            }
        }

        public void DoEventLogging()
        {
            var toLog = "";
            var appLog = new System.Diagnostics.EventLog { Source = "_MATS_AutoService" };
            appLog.WriteEntry("Service Error: \r\n" + toLog);
        }
    }
}
