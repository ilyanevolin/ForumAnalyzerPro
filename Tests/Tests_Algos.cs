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
            string s = "http://www.blackhatworld.com/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 40, 10);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5);//shouldn pass filters

        }
        [TestMethod]
        public void Test_ParseSigs_vbulletin3_8_8()
        {
            ForumScrape fs = new ForumScrape();
            string s = "http://www.skyscrapercity.com/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //shouldn pass filters

        }
        [TestMethod]
        public void Test_ParseSigs_unkown_1()
        {
            ForumScrape fs = new ForumScrape();
            string s = "http://www.makeuptalk.com/f/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters

        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_4_1_4()
        {
            ForumScrape fs = new ForumScrape();
            string s = "http://forum.bodybuilding.com";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters

        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_3_8_7()
        {
            ForumScrape fs = new ForumScrape();
            string s = "http://warriorforum.com";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //shouldn't pass filters

        }
    }
}
