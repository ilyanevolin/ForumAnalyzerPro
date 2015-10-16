using ForumAnalyzerPro.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ForumAnalyzerPro.Algorithms.submodules
{
    internal class PostParser
    {
        private int MAX_REQUESTS, AMOUNT_SIGNATURES;
        private Http http;
        private readonly Object lockMe = new Object(); //mutual exclusion
        private int MAX_THREADS = 1;

        string[] patterns_singlePageUnkown = {
                    "\\<div class=\"signature\" ((.|\\t|\\n)+?)</div>",
                    "<!-- sig -->((.|\\t|\\n)+?)<!-- / sig -->",
                    "blockquote class=\"signature restore\"><div class=\"signaturecontainer\">((.|\\t|\\n)+?)</div></blockquote>",
                    "<td class=\"alt2\" valign=\"bottom\" height=\"100%\"style=\"border-right: 1px solid #d0d0d0\">((.|\\t|\\n)+?)</td>",
                    "(\\<.+?Guests cannot see links in posts.+?\\</)"
        };

        string[] error404 = { "No Thread specified" };


        public PostParser(Http http, int MAX_REQUESTS, int AMOUNT_SIGNATURES, int MAX_THREADS)
        {
            this.http = http;
            this.MAX_REQUESTS = MAX_REQUESTS;
            this.AMOUNT_SIGNATURES = AMOUNT_SIGNATURES;
        }



        //make request to scraped page url and parse signatures
        public IList<string> GetSignaturesFromPage(IList<Uri> urls, string originalUrl)
        {
            if (urls == null) return null;
            IList<string> list = new List<string>();
            int made_requests = 0;
            int failed_filters = 0;

            IList<Task> tasks = new List<Task>();
            foreach (Uri url in urls)
            {
                try
                {
                    if (http.Aborted) break; 
                    Task t = new Task(() =>
                    {
                        ProcessOnThread(originalUrl, ref made_requests, url, ref list, ref failed_filters);
                    });
                    tasks.Add(t); t.Start();

                    if (tasks.Count >= MAX_THREADS)
                    {
                        Task.WaitAll(tasks.ToArray());
                        if (list.Count >= AMOUNT_SIGNATURES || made_requests > MAX_REQUESTS) break;
                        tasks.Clear();
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error PP65." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                    Logging.Log("user", "Algo", msg);
                }
            }
            return list;
        }

        private void ProcessOnThread(string originalUrl, ref int made_requests, Uri url, ref IList<string> list, ref int failed_filters)
        {
            try
            {
                string _url = validateUrl(url.AbsoluteUri, originalUrl);
                if (_url == null) return;
                string rs = http.GET(_url, "", Http.DefaultHeaders, null); ++made_requests;
                if (rs == null || error404.Any(rs.Contains)) return;

                foreach (string s in patterns_singlePageUnkown)
                    ParseSinglePage(ref list, rs, s, ref failed_filters);
            }
            catch (Exception ex)
            {
                string msg = "Error PP78." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "Algo", msg);
            }
        }

        private string validateUrl(string url, string originalUrl)
        {
            string _url = "";
            try
            {
                _url = url.Replace("&amp;", "&");
                if (_url.Contains("http") && !_url.Replace("www.", "").Contains(originalUrl.Replace("www.", "")))
                    _url = null;//if not relative & external url
            }
            catch (Exception ex)
            {
                string msg = "Error PP86." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "Algo", msg);
            }
            return _url;
        }

        protected void ParseSinglePage(ref IList<string> list, string rs, string pattern, ref int failed_filters)
        {
            try
            {
                var matches = Regex.Matches(rs, pattern);
                foreach (Match match in matches)
                    foreach (Capture capture in match.Captures)
                        lock (lockMe)
                        {
                            if (!list.Contains(capture.Value))
                                list.Add(capture.Value);
                            else
                                ++failed_filters;
                        }
            }
            catch (Exception ex)
            {
                string msg = "Error PP102." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "Algo", msg);
            }
        }
    }
}
