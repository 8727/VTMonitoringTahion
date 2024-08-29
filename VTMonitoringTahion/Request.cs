using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VTMonitoringTahion
{
    internal class Request
    {
        public static UInt32 GetUpTime()
        {
            TimeSpan upTime = TimeSpan.FromMilliseconds(Environment.TickCount);
            return Convert.ToUInt32(upTime.TotalSeconds);
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
            string[] req = { speed.ToString(), ((lastReceived - oldReceived) / 131072.0).ToString(), ((lastSent - oldSent) / 131072.0).ToString() };
            return req;
        }
    }
}
