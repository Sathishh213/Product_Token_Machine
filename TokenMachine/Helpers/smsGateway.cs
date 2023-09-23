using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine.Helpers
{
    public static class smsGateway
    {

        public static BackgroundWorker bw = new BackgroundWorker();
        static bool IsInit = true;
        static List<string> urls = new List<string>();


        public static void sendsmsAsyn(string to, string message)
        {
            if (IsInit)
            {
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                IsInit = false;
            }

            string baseurl = Properties.Settings.Default.smsurl + "&mobiles=" + to + "&message=" + message;
            urls.Add(baseurl);

            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
        }

        public static void SendsmsAsyn(string[] to, string message)
        {
            if (IsInit)
            {
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                IsInit = false;
            }

            foreach (var toadd in to)
            {
                string baseurl = Properties.Settings.Default.smsurl + "&mobiles=" + toadd + "&message=" + message;
                urls.Add(baseurl);
            }
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
        }

        private static void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            try
            {
                WebClient client = new WebClient();
                foreach (string baseurl in urls.ToList())
                {
                    string s = "";
                    Stream data = client.OpenRead(baseurl);
                    StreamReader reader = new StreamReader(data);
                    s = reader.ReadToEnd();
                    reader.Close();
                    data.Close();
                    writeSMSlog("Message : " + baseurl.Replace(Properties.Settings.Default.smsurl, "") + " Status : " + s);
                    urls.Remove(baseurl);
                    //if (s.IndexOf("Successfully") > -1)
                    //{
                    //    urls.Remove(baseurl);
                    //}
                }
            }
            catch (Exception ex)
            {
                writeSMSlog(ex.Message + " \t\t" + ex.StackTrace);
            }

        }

        private static void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }


        public static bool sendsmsSync(string to, string message)
        {
            bool sent = false;
            try
            {
                string s = "";
                string baseurl = Properties.Settings.Default.smsurl + "&TO=" + to + "&MESSAGE=" + message;
                WebClient client = new WebClient();
                Stream data = client.OpenRead(baseurl);
                using (StreamReader reader = new StreamReader(data))
                {
                    s = reader.ReadToEnd();
                }
                data.Close();
                if (s.IndexOf("Successfully") > -1)
                {
                    sent = true;
                }

                writeSMSlog("Message : " + message + " Status : " + s);
            }
            catch (Exception ex)
            {
                writeSMSlog(ex.Message + " \t\t" + ex.StackTrace);
            }

            return sent;
        }

        public static void writeSMSlog(string msg)
        {
            try
            {
                string fileName = "log\\sms\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                if (!Directory.Exists("log"))
                {
                    Directory.CreateDirectory("log");
                }
                if (!Directory.Exists("log\\sms"))
                {
                    Directory.CreateDirectory("log\\sms");
                }
                using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    msg = "\n" + DateTime.Now.ToString("h:mm:ss tt") + " : " + msg;
                    sw.WriteLine(msg);
                }
            }
            catch
            {

            }

        }

    }

}
