using ForumAnalyzerPro.Algorithms.submodules;
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
        protected ForumSigType ThisType = new ForumSigType();
        protected int MAX_REQUESTS, AMOUNT_SIGNATURES;
        protected int is_DoFollow, is_Href, is_TextLink, is_TextOnly, is_Unkown;
        protected Http http = new Http();

        //make request to forum (provided) url
        public ForumSigType GetSignaturesFromForum(Uri url, int MAX_REQUESTS, int AMOUNT_SIGNATURES)
        {
            this.AMOUNT_SIGNATURES = AMOUNT_SIGNATURES;
            this.MAX_REQUESTS = MAX_REQUESTS;

            HomepageParser hp = new HomepageParser();            
            var a = hp.GetInternalPages(url);

            PostParser ps = new PostParser(http, MAX_REQUESTS,AMOUNT_SIGNATURES);
            ThisType.Sigs = ps.GetSignaturesFromPage(a, url.AbsoluteUri);

            DetermineType();
            return ThisType;
        }

        private void DetermineType()
        {
            foreach (var s in ThisType.Sigs) filters(s);
            AssignType();
        }

        string[] signature_messages = { "To view links or images in signatures" };
        private void filters(string s)
        {
            //explicit href
            if (signature_messages.Any(s.Contains))
                ++is_Unkown;
            else if (s.Contains("href"))
            {
                ++is_Href;
                if (!s.Contains("nofollow"))
                    ++is_DoFollow;
            }
            else if (s.Contains("http://") || s.Contains("www.") || s.Contains("https://"))
                ++is_TextLink;
            else
                ++is_TextOnly;
        }
        private void AssignType()
        {
            if (is_DoFollow > 0)
                ThisType.Type.Add(ForumSigType.TYPE.HREF_DOFOLLOW, is_DoFollow);

            if (is_Href > 0)
                ThisType.Type.Add(ForumSigType.TYPE.HREF, is_Href);

            if (is_TextLink > 0)
                ThisType.Type.Add(ForumSigType.TYPE.PLAIN_TEXT_LINK, is_TextLink);

            if (is_TextOnly > 0)
                ThisType.Type.Add(ForumSigType.TYPE.TEXT_ONLY, is_TextOnly);

            if (is_Unkown > 0)
                ThisType.Type.Add(ForumSigType.TYPE.UNKOWN, is_Unkown);
        }


    }



}
