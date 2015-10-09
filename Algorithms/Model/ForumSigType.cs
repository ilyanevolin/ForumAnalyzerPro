using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumAnalyzerPro.Algorithms
{
    public class ForumSigType
    {
        public IList<string> Sigs { get; set; }
        public IDictionary<TYPE, int> Type { get; set; }

        public ForumSigType()
        {
            Type = new Dictionary<TYPE, int>();
        }

        public enum TYPE
        {
            HREF_DOFOLLOW,
            HREF,
            PLAIN_TEXT_LINK,
            TEXT_ONLY,
            UNKOWN
        }
    }
}
