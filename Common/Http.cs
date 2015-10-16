using System;
using System.Collections;
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
        public static readonly string UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0";
        public static readonly List<string> DefaultHeaders = new List<string> {
                "Accept-Language: en-GB,en;q=0.8,de;q=0.5,fr;q=0.3",
                "Accept-Encoding: gzip, deflate"
            };

        public bool Aborted { get; private set; }
        HttpWebRequest req;
        CookieContainer CookieJar = new CookieContainer();

        public void Abort()
        {
            if (req != null)
                req.Abort();
            Aborted = true;

        }

        public string POST(string url, string referer, string contentType, List<byte[]> data, List<string> headers, Proxy proxy, int timeOut = 100000, string accept = "*/*")
        {
            if (Aborted) return null;
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
                req.UserAgent = UserAgent;
                req.Accept = accept;
                if (headers != null)
                {
                    foreach (string s in headers)
                    {
                        req.Headers.Add(s);
                    }
                }
                req.CookieContainer = CookieJar;
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
        public string GET(string url, string referer, List<string> headers, Proxy proxy, int timeOut = 60000, string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
        {
            if (Aborted) return null;
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
                req.CookieContainer = CookieJar;
                req.Referer = referer;
                req.Accept = accept;
                req.UserAgent = UserAgent;
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
            catch (AggregateException ex)
            {
                HttpWebResponse objResponse = ((WebException)ex.InnerExceptions[0]).Response as HttpWebResponse;
                string val = objResponse.GetResponseHeader("Refresh");
                if (val != null && val.Length > 0)
                {
                    string link = Regex.Match(val, "(.+?)\\=\\/(cdn-cgi\\/.+)").Groups[2].Value;
                    //string rs = GET(url + link, url, c, headers, proxy, timeOut, accept);
                    string rs = "";
                    try
                    {
                        WebClient client = null;
                        while (client == null)
                        {
                            Console.WriteLine("Trying..");
                            client = CloudflareEvader.CreateBypassedWebClient(url);
                        }

                        CookieJar = ((WebClientEx)client).CookieContainer;
                        rs = client.DownloadString(url);
                    }
                    catch (Exception ex2) { }
                    return rs;
                    //SmsWebClient client = new SmsWebClient(c);
                    //string html = client.DownloadString(link);
                }
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
                string rs = GET("https://www.pinterest.com", "", null, proxy, 5000);

                if (!rs.Contains("title=\"Pinterest\""))
                {
                    rs = GET("https://www.pinterest.com", "", null, proxy, 10000);
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

    public class CloudflareEvader
    {
        /// <summary>
        /// Tries to return a webclient with the neccessary cookies installed to do requests for a cloudflare protected website.
        /// </summary>
        /// <param name="url">The page which is behind cloudflare's anti-dDoS protection</param>
        /// <returns>A WebClient object or null on failure</returns>
        public static WebClient CreateBypassedWebClient(string url)
        {
            var JSEngine = new Jint.Engine(); //Use this JavaScript engine to compute the result.

            //Download the original page
            var uri = new Uri(url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";
            //Try to make the usual request first. If this fails with a 503, the page is behind cloudflare.
            try
            {
                var res = req.GetResponse();
                string html = "";
                using (var reader = new StreamReader(res.GetResponseStream()))
                    html = reader.ReadToEnd();
                return new WebClient();
            }
            catch (WebException ex) //We usually get this because of a 503 service not available.
            {
            string html = "";
            //using (var reader = new StreamReader(ex.Response.GetResponseStream()))
            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                html = reader.ReadToEnd();
            //If we get on the landing page, Cloudflare gives us a User-ID token with the cookie. We need to save that and use it in the next request.
            var cookie_container = new CookieContainer();
            //using a custom function because ex.Response.Cookies returns an empty set ALTHOUGH cookies were sent back.
            var initial_cookies = GetAllCookiesFromHeader(ex.Response.Headers["Set-Cookie"], uri.Host);
            foreach (Cookie init_cookie in initial_cookies)
                cookie_container.Add(init_cookie);

            /* solve the actual challenge with a bunch of RegEx's. Copy-Pasted from the python scrapper version.*/
            var challenge = Regex.Match(html, "name=\"jschl_vc\" value=\"(\\w+)\"").Groups[1].Value;
            var challenge_pass = Regex.Match(html, "name=\"pass\" value=\"(.+?)\"").Groups[1].Value;

            var builder = Regex.Match(html, @"setTimeout\(function\(\){\s+(var t,r,a,f.+?\r?\n[\s\S]+?a\.value =.+?)\r?\n").Groups[1].Value;
            builder = Regex.Replace(builder, @"a\.value =(.+?) \+ .+?;", "$1");
            builder = Regex.Replace(builder, @"\s{3,}[a-z](?: = |\.).+", "");

            //Format the javascript..
            builder = Regex.Replace(builder, @"[\n\\']", "");

            //Execute it. 
            long solved = long.Parse(JSEngine.Execute(builder).GetCompletionValue().ToObject().ToString());
            solved += uri.Host.Length; //add the length of the domain to it.

            Console.WriteLine("***** SOLVED CHALLENGE ******: " + solved);
            Thread.Sleep(3000); //This sleeping IS requiered or cloudflare will not give you the token!!

            //Retreive the cookies. Prepare the URL for cookie exfiltration.
            string cookie_url = string.Format("{0}://{1}/cdn-cgi/l/chk_jschl", uri.Scheme, uri.Host);
            var uri_builder = new UriBuilder(cookie_url);
            var query = HttpUtility.ParseQueryString(uri_builder.Query);
            //Add our answers to the GET query
            query["jschl_vc"] = challenge;
            query["jschl_answer"] = solved.ToString();
            query["pass"] = challenge_pass;
            uri_builder.Query = query.ToString();

            //Create the actual request to get the security clearance cookie
            HttpWebRequest cookie_req = (HttpWebRequest)WebRequest.Create(uri_builder.Uri);
            cookie_req.AllowAutoRedirect = false;
            cookie_req.CookieContainer = cookie_container;
            cookie_req.Referer = url;
            cookie_req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";
            //We assume that this request goes through well, so no try-catch
            var cookie_resp = (HttpWebResponse)cookie_req.GetResponse();
            //The response *should* contain the security clearance cookie!
            if (cookie_resp.Cookies.Count != 0) //first check if the HttpWebResponse has picked up the cookie.
                foreach (Cookie cookie in cookie_resp.Cookies)
                    cookie_container.Add(cookie);
            else //otherwise, use the custom function again
            {
                //the cookie we *hopefully* received here is the cloudflare security clearance token.
                if (cookie_resp.Headers["Set-Cookie"] != null)
                {
                    var cookies_parsed = GetAllCookiesFromHeader(cookie_resp.Headers["Set-Cookie"], uri.Host);
                    foreach (Cookie cookie in cookies_parsed)
                        cookie_container.Add(cookie);
                }
                else
                {
                    //No security clearence? something went wrong.. return null.
                    //Console.WriteLine("MASSIVE ERROR: COULDN'T GET CLOUDFLARE CLEARANCE!");
                    return null;
                }
            }
            //Create a custom webclient with the two cookies we already acquired.
            WebClient modedWebClient = new WebClientEx(cookie_container);
            modedWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");
            modedWebClient.Headers.Add("Referer", url);
            return modedWebClient;
            }
        }

        /* Credit goes to https://stackoverflow.com/questions/15103513/httpwebresponse-cookies-empty-despite-set-cookie-header-no-redirect 
           (user https://stackoverflow.com/users/541404/cameron-tinker) for these functions 
        */
        public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
        {
            ArrayList al = new ArrayList();
            CookieCollection cc = new CookieCollection();
            if (strHeader != string.Empty)
            {
                al = ConvertCookieHeaderToArrayList(strHeader);
                cc = ConvertCookieArraysToCookieCollection(al, strHost);
            }
            return cc;
        }

        private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            string[] strCookTemp = strCookHeader.Split(',');
            ArrayList al = new ArrayList();
            int i = 0;
            int n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                    al.Add(strCookTemp[i]);
                i = i + 1;
            }
            return al;
        }

        private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            CookieCollection cc = new CookieCollection();

            int alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (int i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");
                            string firstName = strCNameAndCValue.Substring(0, firstEqual);
                            string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName;
                            cookTemp.Value = allValue;
                        }
                        continue;
                    }
                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');
                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Path = NameValuePairTemp[1];
                            else
                                cookTemp.Path = "/";
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');

                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Domain = NameValuePairTemp[1];
                            else
                                cookTemp.Domain = strHost;
                        }
                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                    cookTemp.Path = "/";
                if (cookTemp.Domain == string.Empty)
                    cookTemp.Domain = strHost;
                cc.Add(cookTemp);
            }
            return cc;
        }
    }

    /*Credit goes to  https://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
 (user https://stackoverflow.com/users/129124/pavel-savara) */
    public class WebClientEx : WebClient
    {
        public WebClientEx(CookieContainer container)
        {
            this.container = container;
        }

        public CookieContainer CookieContainer
        {
            get { return container; }
            set { container = value; }
        }

        public CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }

}
