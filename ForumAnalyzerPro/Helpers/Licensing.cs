using ForumAnalyzerPro.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ForumAnalyzerPro.Helpers
{
    public static class Licensing
    {
        public static string HID()
        {

            string hardwareID = GenHID();

            try
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ForumAnalyzerProHID", true);
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ForumAnalyzerProHID");
                    key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ForumAnalyzerProHID", true);
                    key.SetValue("ForumAnalyzerProHID", hardwareID);
                }
                else
                {
                    hardwareID = key.GetValue("ForumAnalyzerProHID").ToString();
                    key.Close();
                }
            }
            catch { }

            return hardwareID;
        }
        private static string GenHID()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string hardwareID = "";
            string sMacAddress = "";
            string userID = Environment.UserName;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            hardwareID = ("" + sMacAddress + "" + userID).GetHashCode().ToString();
            Console.WriteLine(hardwareID);

            return hardwareID;
        }

        public struct LicenseResponse
        {
            public string Message;
            public Color color;
            public bool IsValid;
        };
        public struct TrialResponse
        {
            public string Message;
            public bool Success;
        };

        public static LicenseResponse CheckLicense(Http request, string TID)
        {
            LicenseResponse resp = new LicenseResponse();

            string HID = Licensing.HID();

            string rs = null;
            string url = "https://healzer.com/forumanalyzer/check.php?tid=" + TID + "&hid=" + HID;
            try
            {
                List<string> headers = new List<string>
                {   
                    "Accept-Language: en-gb,en;q=0.8,de;q=0.5,fr;q=0.3",
                    "Accept-Encoding: gzip, deflate"
                };

                rs = request.GET(
                        url,
                        "",
                        Http.DefaultHeaders,
                        null
                        );


            }
            catch (Exception ex)
            {
                string msg = "Error LIC77." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "account action: Licensing", msg);


                resp.Message = "Unable to connect to server.";
                resp.color = Colors.Red;
                resp.IsValid = false;
                return resp;
            }

            if (rs == null || rs.Length == 0)
            {
                resp.Message = "Unable to connect to server.";
                resp.color = Colors.Red;
                resp.IsValid = false;
                return resp;
            }
            else if (rs.Contains("License already in use"))
            {
                resp.Message = "License is already in use.";
                resp.color = Colors.Red;
                resp.IsValid = false;
                return resp;
            }
            else if (rs.Contains("Transaction ID not found"))
            {
                resp.Message = "Invalid License ID.";
                resp.color = Colors.Red;
                resp.IsValid = false;
                return resp;
            }

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] raw_input = Encoding.UTF32.GetBytes(HID);
            byte[] raw_output = md5.ComputeHash(raw_input);
            string md5_hid = "";
            foreach (byte myByte in raw_output)
                md5_hid += myByte.ToString("X2");

            if (rs.Equals(md5_hid, StringComparison.InvariantCultureIgnoreCase))
            {
                UserProperties.SetProperty("TID", TID);

                try
                {
                    Microsoft.Win32.RegistryKey key;
                    key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ForumAnalyzerProLicense");
                    key.SetValue("tid", TID);
                    key.Close();
                }
                catch { }

                resp.Message = "Success.";
                resp.color = Colors.Green;
                resp.IsValid = true;
                return resp;
            }
            else
            {
                UserProperties.SetProperty("TID", "");

                try
                {
                    Microsoft.Win32.RegistryKey key;
                    key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ForumAnalyzerProLicense", true);
                    if (key == null)
                    {
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ForumAnalyzerProLicense");
                        key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ForumAnalyzerProLicense", true);
                    }
                    if (key.GetValue("tid") != null)
                    {
                        key.DeleteValue("tid");
                        key.Close();
                    }
                }
                catch { }

                resp.Message = "Invalid License ID.";
                resp.color = Colors.Red;
                resp.IsValid = false;
                return resp;
            }
        }

        public static bool LoadCheckLicense(Http request, ref string lblStatus, ref LicenseResponse resp, ref bool IsValid)
        {
            try
            {
                UserProperties.OnLoad(); //call only once !!!

                string LocalTID = (string)UserProperties.GetProperty("TID");
                try
                {
                    Microsoft.Win32.RegistryKey key;
                    key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ForumAnalyzerProLicense", true);
                    if (key == null)
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ForumAnalyzerProLicense");
                    else
                        LocalTID = key.GetValue("tid") == null ? "" : key.GetValue("tid").ToString();
                    key.Close();
                }
                catch { }

                if (LocalTID.Length > 0)
                {
                    lblStatus = "";
                    Task.Factory.StartNew(() => { Thread.Sleep(100); });
                    lblStatus = "Verifying license...";

                    resp = Licensing.CheckLicense(request, LocalTID);
                }

                IsValid = resp.IsValid;
            }
            catch (Exception ex)
            {
                string msg = "Error SF100." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "account action: SplashForm", msg);

                lblStatus = "Error SF100. Please contact support.";
                return false;
            }
            return true;
        }
        public static bool LoadCheckUpdater(Http request, ref string lblStatus)
        {
            try
            {
                int attempts = 0;
                try
                {
                    while (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Updater.exe"))
                    {
                        if (attempts >= 5)
                            throw new Exception();
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile("https://healzer.com/forumanalyzer/Updater.exe", AppDomain.CurrentDomain.BaseDirectory + "Updater.exe");
                        }
                        attempts++;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error MW74." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                    Logging.Log("user", "downloading new updater version", msg);

                    return false;
                }

                attempts = 0;
                string pv = FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "Updater.exe").ProductVersion;
                string cpv = request.GET("https://healzer.com/forumanalyzer/updater.txt", "", null, null);

                if (cpv.Contains("[[[ERROR"))
                {
                    lblStatus = "Error while connecting to server. Please contact support.";
                    return false;
                }


                if (int.Parse(cpv.Replace(".", "")) > int.Parse(pv.Replace(".", "")))
                {
                    lblStatus = "Downloading update...";
                    while (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Updater.exe"))
                    {
                        try
                        {
                            foreach (var process in Process.GetProcessesByName("Updater"))
                            {
                                process.Kill();
                            }
                            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "Updater.exe");
                            ++attempts;
                        }
                        catch (Exception ex)
                        {
                            string msg = "Error MW108." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                            Logging.Log("user", "kill process new updater version", msg);

                        }
                        if (attempts > 5) throw new Exception();
                    }
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile("https://healzer.com/forumanalyzer/Updater.exe", AppDomain.CurrentDomain.BaseDirectory + "Updater.exe");
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error MW122." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "new updater version", msg);
                lblStatus = "Error MW122. Please contact support.";
                return false;
            }
            return true;
        }
        public static bool LoadCheckUpdate(Http request, ref string lblStatus, bool lite = false)
        {
            //lite for timer
            try
            {
                string pv = FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "ForumAnalyzerPro.exe").ProductVersion;
                string cpv = request.GET("https://healzer.com/forumanalyzer/update.txt", "", null, null);
                if (cpv.Contains("[[[ERROR"))
                {
                    if (!lite)
                        lblStatus = "Error while connecting to server. Please contact support.";
                    return false;
                }

                if (int.Parse(cpv.Replace(".", "")) > int.Parse(pv.Replace(".", "")) && !lite)
                {
                    Process.Start(System.IO.Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
                    return false;
                }
                else if (lite && int.Parse(cpv.Replace(".", "")) > int.Parse(pv.Replace(".", "")))
                {
                    return true;
                }
                else if (lite)
                    return false;
            }
            catch (Exception ex)
            {
                string msg = "Error MW154." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "MW", msg);
                if (!lite)
                    lblStatus = "Error MW154. Please contact support.";
                return false;
            }
            return true;
        }

    }
}
