using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForumAnalyzerPro.Algorithms;
using ForumAnalyzerPro.Common;
using System.ComponentModel;

namespace Tests
{
    [TestClass]
    public class Test_Http
    {
        Http http = new Http();
        [TestMethod]
        public void Test_CPAe()
        {
            string rs = http.GET("http://cpaelites.com/", "", Http.DefaultHeaders, null);            
            Assert.IsTrue(!rs.Contains("[[[ERROR]]]"));
            
        }
    }
}
