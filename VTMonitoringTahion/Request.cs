﻿using System;
using System.Linq;
using System.Threading;
using System.ServiceProcess;
using System.Net.NetworkInformation;


namespace VTMonitoringTahion
{
    internal class Request
    {
        public static void StatusNTPService()
        {
            ServiceController service = new ServiceController("Network Time Protocol Daemon");
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                Logs.WriteLine($">>>> Service {"Network Time Protocol Daemon"} status >>>> {service.Status} <<<<");
                service.Start();
                Logs.WriteLine($">>>> Service {"Network Time Protocol Daemon"} status >>>> {service.Status} <<<<");
            }
        }

        public static UInt32 GetUpTime()
        {
            try
            {
                TimeSpan upTime = TimeSpan.FromMilliseconds(Environment.TickCount);
                return Convert.ToUInt32(upTime.TotalSeconds);
            }
            catch
            {
                return Convert.ToUInt32(Service.StatusJson["UpTime"]);
            }
        }

        public static string[] GetNetwork()
        {
            long oldReceived = 0;
            long oldSent = 0;
            long lastReceived = 0;
            long lastSent = 0;
            UInt16 speed = 0;

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters.Where(a => a.Name == Service.networkMonitoring))
            {
                var ipv4Info = adapter.GetIPv4Statistics();
                oldReceived = ipv4Info.BytesReceived;
                oldSent = ipv4Info.BytesSent;
            }
            Thread.Sleep(1000);
            foreach (NetworkInterface adapter in adapters.Where(a => a.Name == Service.networkMonitoring))
            {
                var ipv4Info = adapter.GetIPv4Statistics();
                lastReceived = ipv4Info.BytesReceived;
                lastSent = ipv4Info.BytesSent;
                speed = Convert.ToUInt16(adapter.Speed / 1000000);
            }
            string[] req = { speed.ToString(), ((lastReceived - oldReceived) / 131072.0).ToString().Replace(",", "."), ((lastSent - oldSent) / 131072.0).ToString().Replace(",", ".")};
            return req;
        }
    }
}
