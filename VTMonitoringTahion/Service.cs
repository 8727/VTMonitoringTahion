using Microsoft.Win32;
using System;
using System.Collections;
using System.Configuration;
using System.ServiceProcess;


namespace VTMonitoringTahion
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        public static TimeSpan localZone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

        public static Hashtable StatusJson = new Hashtable();
        public static Hashtable ViewCamera = new Hashtable();

        public static int storageDays = 35;
        public static bool statusWeb = true;
        public static string networkMonitoring = "vEthernet (LAN)";
        public static int dataUpdateInterval = 5;

        public static string sqlSource = @"(LOCAL)\SQLEXPRESS";
        public static string sqlUser = "sa";
        public static string sqlPassword = "1";

        void LoadConfig()
        {
            Logs.WriteLine("------------------------- Monitoring Service Settings -------------------------");

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\VTMonitoringTahion", true))
            {
                if (key.GetValue("FailureActions") == null)
                {
                    key.SetValue("FailureActions", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0xea, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0xea, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0xea, 0x00, 0x00 });
                }
            }

            if (ConfigurationManager.AppSettings.Count != 0)
            {
                networkMonitoring = ConfigurationManager.AppSettings["NetworkMonitoring"];
                dataUpdateInterval = Convert.ToInt32(ConfigurationManager.AppSettings["DataUpdateIntervalMinutes"]);

                sqlSource = ConfigurationManager.AppSettings["SQLDataSource"];
                sqlUser = ConfigurationManager.AppSettings["SQLUser"];
                sqlPassword = ConfigurationManager.AppSettings["SQLPassword"];
            }


            var hostStatusTimer = new System.Timers.Timer(dataUpdateInterval * 60000);
            hostStatusTimer.Elapsed += Timer.OnHostStatusTimer;
            hostStatusTimer.AutoReset = true;
            hostStatusTimer.Enabled = true;

            Logs.WriteLine($">>>>> Monitoring host parameters at {dataUpdateInterval} minute intervals.");
            Logs.WriteLine("-------------------------------------------------------------------------------");
        }

        void CreatedStatusJson()
        {
            StatusJson.Add("UpTime", Request.GetUpTime().ToString());

            string[] network = Request.GetNetwork();
            StatusJson.Add("NetworkNetspeed", network[0]);
            StatusJson.Add("NetworkReceived", network[1]);
            StatusJson.Add("NetworkSent", network[2]);
        }


        protected override void OnStart(string[] args)
        {
            Logs.WriteLine("*******************************************************************************");
            Logs.WriteLine("************************** Service Monitoring START ***************************");
            Logs.WriteLine("*******************************************************************************");
            LoadConfig();
            CreatedStatusJson();
            Web.WEBServer.Start();
        }

        protected override void OnStop()
        {
            statusWeb = false;
            Web.WEBServer.Interrupt();
            Logs.WriteLine("*******************************************************************************");
            Logs.WriteLine("*************************** Service Monitoring STOP ***************************");
            Logs.WriteLine("*******************************************************************************");
        }
    }
}
