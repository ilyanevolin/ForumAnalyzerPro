using ForumAnalyzerPro.Common;
using System;
using System.Collections.Generic;
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
        public PostParser(Http http, int MAX_REQUESTS, int AMOUNT_SIGNATURES)
        {
            this.http = http;
            this.MAX_REQUESTS = MAX_REQUESTS;
            this.AMOUNT_SIGNATURES = AMOUNT_SIGNATURES;
        }

        string[] patterns_singlePageUnkown = {
                    "\\<div class=\"signature\" ((.|\\t|\\n)+?)</div>",
                    "<!-- sig -->((.|\\t|\\n)+?)<!-- / sig -->",
                    "blockquote class=\"signature restore\"><div class=\"signaturecontainer\">((.|\\t|\\n)+?)</div></blockquote>",
                    "<td class=\"alt2\" valign=\"bottom\" height=\"100%\"style=\"border-right: 1px solid #d0d0d0\">((.|\\t|\\n)+?)</td>"
        };

        string[] error404 = { "No Thread specified" };

        //make request to scraped page url and parse signatures
        public IList<string> GetSignaturesFromPage(IList<Uri> urls, string originalUrl)
        {
            IList<string> list = new List<string>();
            int made_requests = 0;
            int failed_filters = 0;
            string rs = "";
            foreach (Uri url in urls)
            {
                string _url = validateUrl(url.AbsoluteUri, originalUrl);
                if (_url == null) continue;
                rs = http.GET(_url, "", null, null, null); ++made_requests;
                if (rs == null || error404.Any(rs.Contains)) continue;

                foreach (string s in patterns_singlePageUnkown)
                    ParseSinglePage(ref list, rs, s, ref failed_filters);

                if (list.Count >= AMOUNT_SIGNATURES || made_requests > MAX_REQUESTS) break;
            }
            return list;
        }
        private string validateUrl(string url, string originalUrl)
        {
            string _url = "";
            _url = url.Replace("&amp;", "&");
            if (_url.Contains("http") && !_url.Replace("www.", "").Contains(originalUrl.Replace("www.", "")))
                _url = null;//if not relative & external url
            return _url;
        }

        protected void ParseSinglePage(ref IList<string> list, string rs, string pattern, ref int failed_filters)
        {
            try
            {
                var matches = Regex.Matches(rs, pattern);
                foreach (Match match in matches)
                    foreach (Capture capture in match.Captures)
                        if (!list.Contains(capture.Value))
                            list.Add(capture.Value);
                        else
                            ++failed_filters;
            }
            catch { }
        }
    }
}
