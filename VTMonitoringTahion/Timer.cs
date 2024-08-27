

using System.Timers;
using System;

namespace VTMonitoringTahion
{
    internal class Timer
    {
        public static void OnHostStatusTimer(Object source, ElapsedEventArgs e)
        {
            Service.StatusJson["UpTime"] = Request.GetUpTime().ToString();
            TimeSpan uptime = TimeSpan.FromSeconds(Convert.ToDouble(Service.StatusJson["UpTime"]));
            Logs.WriteLine($"Host uptime {uptime}.");
            //-------------------------------------------------------------------------------------------------

            string[] network = Request.GetNetwork();
            Service.StatusJson["NetworkNetspeed"] = network[0];
            Service.StatusJson["NetworkReceived"] = network[1];
            Service.StatusJson["NetworkSent"] = network[2];
            Logs.WriteLine($"Interface speed {Service.StatusJson["NetworkNetspeed"]}, incoming load {Service.StatusJson["NetworkReceived"]}, outgoing load {Service.StatusJson["NetworkSent"]}.");
            //-------------------------------------------------------------------------------------------------
        }
    }
}
