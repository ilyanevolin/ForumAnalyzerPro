using ForumAnalyzerPro.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ForumAnalyzerPro.Algorithms
{
    public class ForumScrape
    {
        protected Http http = new Http();

        //make request to forum (provided) url
        public ISet<string> GetSignaturesFromForum(Uri url)
        {
            var a = GetInternalPages(url);
            var b = GetSignaturesFromPage(a);
            return b;
        }

        //parse internal URLs
        protected IList<Uri> GetInternalPages(Uri url)
        {
            string rs = http.GET(url.AbsoluteUri, "", null, null, null);

            /*if (DetermineForum(rs))
                return ParseHomepage(rs);
            else*/
            return AllLinksFromHomePage(rs);

        }
        /*
        protected bool DetermineForum(string rs)
        {
            string pattern = "<meta name=\"generator\" content=\"vBulletin 4\\.\\d\\.\\d\" />";
            if (Regex.IsMatch(rs, pattern))
            {
                strParseHomePage = "<h4 class=\"threadtitle\">\\n\\t*<a href=\"(.+?)\"";
                strParseSinglePage = "<blockquote class=\"signature restore\"><div class=\"signaturecontainer\">(.+?)</div></blockquote>";
                return true;
            }
            //else if ...

            return false;
        }*/

        string[] blacklisted_words = { "members", "users", "member", "user" };
        //unkown forum type
        protected IList<Uri> AllLinksFromHomePage(string rs)
        {
            var list = new List<Uri>();
            string pattern = "<a\\s+(?:[^>]*?\\s+)?href=\"([^\"]*)\"";
            MatchCollection mc = Regex.Matches(rs, pattern);
            foreach (Match m in mc)
            {
                string s = m.Groups[1].ToString();
                if (blacklisted_words.Any(s.Contains)) continue;
                Match mm = Regex.Match(s, "((\\d){5,})");
                if (mm.Captures.Count > 0)
                {
                    if (Http.ValidUri(s))
                    {
                        list.Add(new Uri(s));
                        AddRandomUri(ref list, s, mm.Groups[1].ToString());
                    }
                }
            }

            return list;
        }
        private void AddRandomUri(ref List<Uri> list, string url, string number)
        {
            //extra links by increm. thread_id
            for (int i = 0; i < 20; i++)
            {
                int n;
                if (int.TryParse(number, out n))
                {
                    string s = url.Replace(n.ToString(), (n - i - 1).ToString());
                    var u = new Uri(s);
                    if (Http.ValidUri(s) && !list.Contains(u))
                        list.Add(u);
                }
            }
        }

        //known forum type
        protected string strParseHomePage { get; set; }
        protected IList<Uri> ParseHomepage(string rs)
        {
            IList<Uri> list = new List<Uri>();
            var matches = Regex.Matches(rs, strParseHomePage);
            foreach (Match match in matches)
                list.Add(new Uri(match.Groups[1].Value));

            return list;
        }

        string[] patterns_singlePageUnkown = {
                    "\\<div class=\"signature\" ((.|\\t|\\n)+?)</div>",
                    "<!-- sig -->((.|\\t|\\n)+?)<!-- / sig -->",
                    "blockquote class=\"signature restore\"><div class=\"signaturecontainer\">(.+?)</div></blockquote>"
        };

        //make request to scraped page url and parse signatures
        protected ISet<string> GetSignaturesFromPage(IList<Uri> urls)
        {
            int made_requests = 0;
            int failed_filters = 0;
            ISet<string> list = new HashSet<string>();
            string rs = "";
            foreach (Uri url in urls)
            {
                rs = http.GET(url.AbsoluteUri, "", null, null, null); ++made_requests;
                if (rs == null) continue;
                IList<string> a = new List<string>();
                if (strParseSinglePage != null)
                    ParseSinglePage(ref a, rs, strParseSinglePage);
                else
                    foreach (string s in patterns_singlePageUnkown)
                        ParseSinglePage(ref a, rs, s);

                foreach (var s in a)
                    if (!list.Contains(s))
                    {
                        if (filter(s)) list.Add(s); else ++failed_filters;
                    }
                if (list.Count >= 20 || made_requests > 40 || failed_filters > 10) break; //10 is more than enough
            }
            return list;
        }

        private bool filter(string s)
        {
            if (s.Contains("href") && !s.Contains("nofollow"))
                return true;
            return false;
        }

        protected string strParseSinglePage { get; set; }
        protected void ParseSinglePage(ref IList<string> list, string rs, string pattern)
        {
            try
            {
                var matches = Regex.Matches(rs, pattern);
                foreach (Match match in matches)
                    foreach (Capture capture in match.Captures)
                        list.Add(capture.Value);
            }
            catch { }
        }

    }
    //check dofollow
}
