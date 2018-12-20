using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeoWinner;
using System.Linq;
using System.Collections.Generic;

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
            var prices = PriceScraper.ExtractPrices(htmlString);

            Assert.IsNotNull(prices);
        }

        [TestMethod]
        public void TestMethod19()
        {
            string htmlString = File.ReadAllText("TestData\\Gewinnzahlen19.html");
            List<Day> days = PriceScraper.ExtractPrices(htmlString);
            Assert.IsNotNull(days);


          Console.Write( PriceHelper.PrintDays(days, true));

            for (int i = 1; i <= 16; i++)
            {
                Day day = days.SingleOrDefault(x => x.Date == i);

                Assert.IsNotNull(day);

                

                Assert.IsTrue(0 < day.Prices.Count, $"Day {i} has no Prices.");

                foreach(Price p in day.Prices)
                {
                    Assert.AreEqual(p.Numbers.Count, p.IntNumbers.Count());

                    Assert.IsTrue(p.Numbers.Any(n => !string.IsNullOrEmpty(n)), $"Price '{p.Description}' of day {i} has no numbers.");
                }
            }
        }
    }
}
