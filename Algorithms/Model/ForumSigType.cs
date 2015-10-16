using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumAnalyzerPro.Algorithms
{
    public class ForumSigType
    {
        public string Url { get; set; }
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
            UNKOWN,
            ERROR
        }

        public string Col_TypeForum { get {
            string s = "";
            if (Type.ContainsKey(TYPE.HREF_DOFOLLOW) && Type[TYPE.HREF_DOFOLLOW] > 0)
                s = "Allows signatures + URL + DoFollow";
            else if (Type.ContainsKey(TYPE.HREF) && Type[TYPE.HREF] > 0)
                s = "Allows signatures + URL (NoFollow)";
            else if (Type.ContainsKey(TYPE.PLAIN_TEXT_LINK) && Type[TYPE.PLAIN_TEXT_LINK] > 0)
                s = "Allows text-only signatures (non-clickable URLs)";
            else if (Type.ContainsKey(TYPE.TEXT_ONLY) && Type[TYPE.TEXT_ONLY] > 0)
                s = "Allows text-only signatures (URLs not allowed)";
            else if (Type.ContainsKey(TYPE.UNKOWN))
                s = "No signatures found";
            else if (Type.ContainsKey(TYPE.ERROR))
                s = "Error occured.";

            return s;
        } }
    }
}
