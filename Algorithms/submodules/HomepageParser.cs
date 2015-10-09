using ForumAnalyzerPro.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ForumAnalyzerPro.Algorithms.submodules
{
    internal class HomepageParser
    {
        Http http = new Http();

        //parse internal URLs
        public IList<Uri> GetInternalPages(Uri url)
        {
            string rs = http.GET(url.AbsoluteUri, "", null, null, null);
            return AllLinksFromHomePage(rs, url.AbsoluteUri);
        }

        string[] blacklisted_words = { "members", "users", "member", "user", "ratings_guide", "consumer" };
        //unkown forum type
        protected IList<Uri> AllLinksFromHomePage(string rs, string rootpath)
        {
            var list = new List<Uri>();
            string pattern = "<a\\s+(?:[^>]*?\\s+)?href=\"([^\"]*)\"";
            MatchCollection mc = Regex.Matches(rs, pattern);
            foreach (Match m in mc)
            {
                string s = m.Groups[1].ToString();
                if (blacklisted_words.Any(s.Contains)) continue;
                Match mm = Regex.Match(s, "[\\=\\/\\-]((\\d){5,})(\\-|\\&){0,}");
                if (mm.Captures.Count > 0)
                {
                    if (!s.Contains("http")) s = rootpath + s;
                    if (Http.ValidUri(s))
                    {
                        list.Add(new Uri(s));
                        AddRandomUri(ref list, s, mm.Groups[1].ToString());
                    }
                }
            }

            var rnd = new Random();
            IList<Uri> b = list.OrderBy(i => rnd.Next()).Cast<Uri>().ToList(); ;
            return b;
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


    }
}
