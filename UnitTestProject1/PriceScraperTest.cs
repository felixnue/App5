using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeoWinner;

namespace UnitTestProject1
{
    [TestClass]
    public class PriceScraperTest
    {
        [TestInitialize]
        public void TestInit()
        {
            
        
            string htmlString = File.ReadAllText("TestData\\Gewinnzahlen01.html");
        }

        [TestMethod]
        public void TestMethod1()
        {
            string htmlString = File.ReadAllText("TestData\\Gewinnzahlen06.html");
            var prices =  PriceScraper.ExtractPrices(htmlString);

            Assert.IsNotNull(prices);

        }
    }
}
