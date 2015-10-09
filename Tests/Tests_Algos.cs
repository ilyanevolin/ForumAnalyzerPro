using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForumAnalyzerPro.Algorithms;

namespace Tests
{
    [TestClass]
    public class Tests_Algos
    {
        [TestMethod]
        public void Test_ParseSigs_vbulletin4_2_2()
        {
            ForumScrape fs = new ForumScrape();
            var set = fs.GetSignaturesFromForum(new Uri("http://www.blackhatworld.com/"));
            System.Diagnostics.Debug.WriteLine(set.Count);
            Assert.IsTrue(set.Count > 5);//shouldn pass filters

        }
        [TestMethod]
        public void Test_ParseSigs_vbulletin3_8_8()
        {
            ForumScrape fs = new ForumScrape();
            var set = fs.GetSignaturesFromForum(new Uri("http://www.skyscrapercity.com/"));
            System.Diagnostics.Debug.WriteLine(set.Count);
            Assert.IsTrue(set.Count > 5); //shouldn pass filters

        }
        [TestMethod]
        public void Test_ParseSigs_unkown_1()
        {
            ForumScrape fs = new ForumScrape();
            var set = fs.GetSignaturesFromForum(new Uri("http://www.makeuptalk.com/f/"));
            System.Diagnostics.Debug.WriteLine(set.Count);
            Assert.IsTrue(set.Count <= 5); //shouldn't pass filters

        }
    }
}
