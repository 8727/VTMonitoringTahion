using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;


namespace VTMonitoringTahion
{
    internal class Web
    {
        static HttpListener serverWeb;
        public static Thread WEBServer = new Thread(ThreadWEBServer);

        static void ThreadWEBServer()
        {
            serverWeb = new HttpListener();
            serverWeb.Prefixes.Add(@"http://+:8020/");
            serverWeb.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            serverWeb.Start();
            while (Service.statusWeb)
            {
                ProcessRequest();
            }
        }

        static void ProcessRequest()
        {
            var result = serverWeb.BeginGetContext(ListenerCallback, serverWeb);
            var startNew = Stopwatch.StartNew();
            result.AsyncWaitHandle.WaitOne();
            startNew.Stop();
        }

        static void ListenerCallback(IAsyncResult result)
        {
            var HttpResponse = serverWeb.EndGetContext(result);
            //string key = HttpResponse.Request.QueryString["key"];
            string json = "{\n\t\"dateTime\":\"" + DateTime.Now.ToString() + "\"";

            json += ",\n\t\"upTime\":\"" + Service.StatusJson["UpTime"] + "\"";

            json += ",\n\t\"networkNetspeed\":\"" + Service.StatusJson["NetworkNetspeed"] + "\"";
            json += ",\n\t\"networkSent\":\"" + Service.StatusJson["NetworkSent"] + "\"";
            json += ",\n\t\"networkReceived\":\"" + Service.StatusJson["NetworkReceived"] + "\"";

            json += ",\n\t\"viewCamera\":[\n\t";
            int c = 0;
            foreach (DictionaryEntry ViewCameraKey in Service.ViewCamera)
            {
                c++;
                json += "\t{\n\t\t\"ip\":\"" + ViewCameraKey.Key + "\",\n\t\t\"status\":\"" + ViewCameraKey.Value + "\"\n\t\t}";
                if (c < Service.ViewCamera.Count)
                {
                    json += ",";
                }
            }
            json += "\n\t]";

            json += "\n}";

            HttpResponse.Response.Headers.Add("Content-Type", "application/json");
            HttpResponse.Response.StatusCode = 200;
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            HttpResponse.Response.ContentLength64 = buffer.Length;
            HttpResponse.Response.OutputStream.Write(buffer, 0, buffer.Length);
            HttpResponse.Response.Close();
        }
    }
}
