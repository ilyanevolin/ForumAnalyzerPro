using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ForumAnalyzerPro.Common
{

    public class Http
    {
        public static string ACCEPT_JSON = "application/json, text/javascript, */*; q=0.01";
        public static string ACCEPT_HTML = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";


        HttpWebRequest req;
        public void Abort()
        {
            if (req != null)
                req.Abort();
        }

        public string POST(string url, string referer, string contentType, List<byte[]> data, List<string> headers, CookieContainer c, Proxy proxy, int timeOut = 100000, string accept = "*/*")
        {
            System.Diagnostics.Debug.WriteLine(url);
            //Console.WriteLine(url);
            try
            {

                req = (HttpWebRequest)WebRequest.Create(url);

                if (proxy != null)
                {
                    req.Proxy = proxy.WebProxy;
                    req.PreAuthenticate = true;
                    req.Timeout = timeOut;
                }

                //req.Timeout = 15000;
                req.ContentType = contentType;//"multipart/form-data; boundary=" + boundary;
                req.Method = "POST";
                req.KeepAlive = true;
                req.Referer = referer;
                req.AllowAutoRedirect = true;
                req.UserAgent = ("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:30.0) Gecko/20100101 Firefox/30.0");
                req.Accept = accept;
                if (headers != null)
                {
                    foreach (string s in headers)
                    {
                        req.Headers.Add(s);
                    }
                }
                req.CookieContainer = c == null ? new CookieContainer() : c;
                req.ServicePoint.Expect100Continue = false;
                ServicePointManager.Expect100Continue = false;
                req.AutomaticDecompression = DecompressionMethods.GZip;

                Stream stream = req.GetRequestStream();
                foreach (byte[] b in data)
                {
                    stream.Write(b, 0, b.Length);
                }
                stream.Close();

                Task<WebResponse> t = req.GetResponseAsync();
                t.Wait();
                HttpWebResponse response = (HttpWebResponse)t.Result;

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string RS = reader.ReadToEnd();

                reader.Close();
                req.Abort();
                stream.Close();

                return RS;
            }
            catch (WebException ex)
            {
                HttpWebResponse objResponse = ex.Response as HttpWebResponse;
                string RS = "[[[ERROR:POST]]]\n\n";
                RS += url + Environment.NewLine + Environment.NewLine;
                RS += ex.Message + Environment.NewLine + Environment.NewLine;
                if (objResponse != null)
                {
                    StreamReader reader = new StreamReader(objResponse.GetResponseStream());                   
                    RS += reader.ReadToEnd() + "\n\n" + objResponse.ResponseUri + "\n";
                    for (int i = 0; i < objResponse.Headers.Count; ++i)
                        RS += ("\n" + objResponse.Headers.Keys[i] + ": " + objResponse.Headers[i]);
                }

                Logging.Log("user or auto", "http", RS);
                return RS;
            }
            catch (Exception ex)
            {
                string RS = "[[[ERROR]]]\n\n";
                RS += url + Environment.NewLine + Environment.NewLine;
                RS += ex.Message + Environment.NewLine + Environment.NewLine;
                Logging.Log("user or auto", "http", RS);
                return RS;
            }
        }
        public string GET(string url, string referer, CookieContainer c, List<string> headers, Proxy proxy, int timeOut = 60000, string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
        {

            System.Diagnostics.Debug.WriteLine(url);
            try
            {

                req = (HttpWebRequest)WebRequest.Create(url);

                if (proxy != null)
                {
                    req.Proxy = proxy.WebProxy;
                    req.PreAuthenticate = true;
                    req.Timeout = timeOut;
                }

                //req.Timeout = 15000;
                req.CookieContainer = c == null ? new CookieContainer() : c;
                req.Referer = referer;
                req.Accept = accept;
                req.UserAgent = ("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:30.0) Gecko/20100101 Firefox/30.0");
                if (headers != null)
                {
                    foreach (string s in headers)
                        req.Headers.Add(s);
                }
                req.AllowAutoRedirect = true;
                req.KeepAlive = true;
                req.AutomaticDecompression = DecompressionMethods.GZip;
                ServicePointManager.Expect100Continue = false;
                req.ServicePoint.Expect100Continue = false;
                Task<WebResponse> t = req.GetResponseAsync();
                t.Wait();
                HttpWebResponse response = (HttpWebResponse)t.Result;
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string RS = reader.ReadToEnd();
                response.Close();
                req.Abort();
                stream.Close();
                return RS;
            }
            catch (WebException ex)
            {
                HttpWebResponse objResponse = ex.Response as HttpWebResponse;
                string RS = "[[[ERROR:GET]]]\n\n";
                RS += url + Environment.NewLine + Environment.NewLine;
                RS += ex.Message + Environment.NewLine + Environment.NewLine;
                if (objResponse != null)                
                {
                    StreamReader reader = new StreamReader(objResponse.GetResponseStream());
                    RS += reader.ReadToEnd() + "\n\n" + objResponse.ResponseUri + "\n";
                    for (int i = 0; i < objResponse.Headers.Count; ++i)
                        RS += ("\n" + objResponse.Headers.Keys[i] + ": " + objResponse.Headers[i]);
                }

                Logging.Log("user or auto", "http", RS);
                return RS;
            }
            catch (Exception ex)
            {
                string RS = "[[[ERROR]]]\n\n";
                RS += url + Environment.NewLine + Environment.NewLine;
                RS += ex.Message + Environment.NewLine + Environment.NewLine;
                Logging.Log("user or auto", "http", RS);
                return RS;
            }
        }
        public bool TestProxy(Proxy proxy)
        {
            try
            {
                string rs = GET("https://www.pinterest.com", "", new CookieContainer(), null, proxy, 5000);

                if (!rs.Contains("title=\"Pinterest\""))
                {
                    rs = GET("https://www.pinterest.com", "", new CookieContainer(), null, proxy, 10000);
                    if (!rs.Contains("title=\"Pinterest\""))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                string msg = "Error testing proxy." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "account action: proxy test", msg);
                
                return false;
            }
        }

        public static string getBoundary()
        {
            return "----------------------------" + System.DateTime.Now.Ticks.ToString("x");
        }
        public static void DownloadFile(string url, string file)
        {
            using (WebClient wc = new WebClient())
                wc.DownloadFile(url, file);
        }
        public static string DownloadFile(string url)
        {
            string ext = Path.GetExtension(url);
            string file = Path.GetTempFileName() + ext;
            string fullpath = Path.Combine(Path.GetTempPath(), file);
            DownloadFile(url, file);
            return fullpath;
        }

        public static string HttpEncode(string s)
        { return HttpUtility.UrlEncode(s); }

        public static bool ValidUrl(string u)
        {
            if (u == null || u.Length <= 1) return false;
            string p = @"(http://|https://)(www\.)?([a-zA-Z0-9\-]+?)(\.)([a-zA-Z]+?){1,6}(([a-z0-9A-Z\?\&\%\=_\-\./])+?)$";
            return Regex.IsMatch(u, p);
        }
        public static bool ValidUri(string s)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(s, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }
    }
}
