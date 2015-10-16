using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForumAnalyzerPro.Algorithms;
using ForumAnalyzerPro.Common;
using System.ComponentModel;

namespace Tests
{
    [TestClass]
    public class Tests_Algos
    {
        Http http = new Http();
        public Tests_Algos()
        {
        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_4_2_2__1()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://www.blackhatworld.com/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 40, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5);//shouldn pass filters

        }
        [TestMethod]
        public void Test_ParseSigs_vbulletin_3_8_8()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://www.skyscrapercity.com/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //shouldn pass filters

        }
        [TestMethod]
        public void Test_ParseSigs_unkown_1()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://www.makeuptalk.com/f/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters

        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_4_1_4()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://forum.bodybuilding.com";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters

        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_3_8_7__1()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://warriorforum.com";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters
        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_3_8_7__2()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://forum.illpumpyouup.com";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters
        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_4_2_2__2()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://wlforums.com/forums/"; //remove forum.php or any other tail such as .html
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters
        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_3_8_9__1()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "https://www.catalystathletics.com/forum/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters
        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_4_2_2__3()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://wannabebig.com/forums/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 10, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //should pass filters
        }

        [TestMethod]
        public void Test_ParseSigs_vbulletin_3_8_8__2()
        {
            ForumScrape fs = new ForumScrape(http);
            string s = "http://forums.anandtech.com/";
            var set = fs.GetSignaturesFromForum(new Uri(s), 60, 20, 4);
            System.Diagnostics.Debug.WriteLine(set.Sigs.Count);
            foreach (var kv in set.Type)
                System.Diagnostics.Debug.WriteLine("Type: " + kv.Key + ":    " + kv.Value);
            System.Diagnostics.Debug.WriteLine("");
            Assert.IsTrue(set.Sigs.Count > 5); //shouldn pass filters

        }
    }
}
