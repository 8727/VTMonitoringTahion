using Microsoft.Win32;
using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Xml;


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
        public static Hashtable ViewCameraStatus = new Hashtable();

        public static int storageDays = 35;
        public static bool statusWeb = true;
        public static string networkMonitoring = "Ethernet";
        public static int dataUpdateInterval = 5;

        public static string diskMonitoring = "C:\\Program Files\\VOCORD\\VOCORD Tahion NetScaleIP";

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

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\VTNetScaleIPService"))
            {
                if (key != null)
                {
                    if (key.GetValue("ImagePath") != null)
                    {
                        string install = key.GetValue("ImagePath").ToString().Replace("\"", "");
                        diskMonitoring = install.Remove(install.LastIndexOf("\\")) + "\\VTNetScaleIPService.exe.srv.config";
                    }
                }
            }

            if (File.Exists(diskMonitoring))
            {
                XmlDocument dataXmlFile = new XmlDocument();
                dataXmlFile.Load(diskMonitoring);
                XmlNodeList nodeList = dataXmlFile.SelectNodes($"//Camera");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        var pattern = @"(?<key>\w+)=(?<value>.*?)(?=(\s\w+=|$))";
                        var matches = Regex.Matches(node.OuterXml, pattern);

                        string cam = null;
                        string ip = null;

                        foreach (Match match in matches)
                        {
                            if ((match.Groups["key"]).ToString() == "CamId")
                            {
                                cam = match.Groups["value"].ToString().Replace("\"", "");
                            }

                            if ((match.Groups["key"]).ToString() == "Ip")
                            {
                                ip = match.Groups["value"].ToString().Replace("\"", "");
                            }
                        }

                        ViewCamera.Add(ip, SQL.ViewCameraSources("4950000000000000" + cam + "0000000000000001"));
                        ViewCameraStatus.Add(ip, SQL.ViewCameraStatus((ViewCamera[ip]).ToString()));
                        Logs.WriteLine($">>>>> Recording from overview camera {ip} added to status monitoring.");
                    }
                }
            }

            var pingTimer = new System.Timers.Timer(5 * 60000);
            pingTimer.Elapsed += Timer.OnViewCameraTimer;
            pingTimer.AutoReset = true;
            pingTimer.Enabled = true;

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
