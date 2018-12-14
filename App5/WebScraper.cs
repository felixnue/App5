using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HtmlAgilityPack;

namespace LeoWinner
{
    internal class PriceScraper
    {
        public static async Task<string> LoadStringFromUrl()
        {
            var uri = @"https://www.leo-erlangen.de/adventskalender/adventskalender-gewinnzahlen/";

            WebClient webClient = new WebClient();
            string htmlString = await webClient.DownloadStringTaskAsync(uri);

            return htmlString;
        }       

        public static string GetAllPrices(string searchNumber, string htmlString)
        {
            List<Price> prices = ExtractPrices(htmlString);
            return PrintPrices(prices, searchNumber);
        }

        // return string.Empty if no price found
        public static string GetPriceOfNUmber(string number, string htmlString)
        {
            List<Price> prices = ExtractPrices(htmlString);
            var winner = prices.FirstOrDefault(x => x.Numbers.Contains(number));
            if (winner != null)
            {
                return winner.Description;
            }

            return string.Empty;
        }

        public static bool IsUpToDate(int day, string htmlString)
        {
            var prices = ExtractPrices(htmlString);
            return prices.Any(x => x.Day == day);
        }

        private static List<Price> ExtractPrices(string htmlString)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);

            List<Price> prices = new List<Price>();
            for (int day = 24; day >= 1; day--)
            {
                prices.AddRange(GetPricesOfDay(day, htmlDoc));
            }

            return prices;
        }

        private static List<Price> GetPricesOfDay(int day, HtmlDocument htmlDoc)
        {
            List<Price> prices = new List<Price>();
            if (TryGetHtmlNodeForDay(day, htmlDoc, out HtmlNode nodeForDay))
            {
                prices = GetPricesOfDay(day, nodeForDay);
            }
            return prices;
        }

        private static string PrintPrices(List<Price> prices, string searchNumber)
        {
            string pricesString = "Detected the following prices:\n";
            string stringWinner = string.Empty;

            for (int day = 24; day >= 1; day--)
            {
                if (prices.Any(x => x.Day == day))
                {
                    pricesString += "Prices of day " + day + "\n";
                }               

                foreach (var p in prices.Where(x => x.Day == day))
                {
                    pricesString += p.ToString() + "\n";
                    if (p.Numbers.Any(x => x.Equals(searchNumber)))
                    {
                        stringWinner += " \n******* WINNNER *******\n\n";
                        stringWinner += " Your price: " + p.Description;
                    }
                }
            }
            if (stringWinner == String.Empty)
            {
                stringWinner = searchNumber + ": -  Better luck next time :)";
            }
            else
            {
               // Toast.MakeText(this, stringWinner, ToastLength.Short).Show();
            }

            return stringWinner + "\n\n" + pricesString;
        }       

        private class Price
        {
            public Price()
            {
                Day = -1;
                Description = "Undefined";
                Numbers = new List<string>();
            }

            public int Day;
            public string Description;
            public IList<string> Numbers;

            public override string ToString()
            {
                return String.Concat("\"", Description, "\"\n", String.Join(" | ", Numbers));
            }
        }


        private static bool TryGetHtmlNodeForDay(int day, HtmlDocument htmlDoc, out HtmlNode nodeFound)
        {
            bool found = false;
            nodeFound = null;

            // All "strong" below body -> headlines 
            // Check if it is a headline we want for xth day
            var possibleNodes = htmlDoc.DocumentNode.SelectNodes("//body//strong");
            foreach (HtmlNode node in possibleNodes)
            {
                if (node.InnerHtml.ToLower().Contains("dezember")
                  && node.InnerHtml.ToLower().Contains(day.ToString("D2"))
                  && node.InnerHtml.ToLower().Contains("gewinnzahlen"))
                {
                    //found!
                    // -> extract
                    nodeFound = node;
                    found = true;
                    break;
                }
                else
                {
                    //not found -> go on
                }
            }

            return found;
        }

        private static List<Price> GetPricesOfDay(int day, HtmlNode nodeOfDay)
        {
            List<Price> pricesOfDay = new List<Price>();

            //debudgString += ("Treffer für Tag " + dayString + "; InnerHtml: " + node.InnerHtml + "\n");
            //    Console.WriteLine("Parent 2: "+ n.ParentNode.ParentNode.OuterHtml);

            // This works for all but the first day... error on site?
            HtmlNode nodeUL = tryFindNodeUL(nodeOfDay.ParentNode.NextSibling);
            // Solution for entry 1:
            if (nodeUL == null)
            {
                nodeUL = tryFindNodeUL(nodeOfDay.NextSibling);
            }

            if (nodeUL != null && nodeUL.ChildNodes != null)
            {
                //sibling is the ul list of the numbers and prices
                foreach (HtmlNode child in nodeUL.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element && child.Name == "li")
                    {
                        Price price = GetPriceDecriptionAndNumbers(child);
                        price.Day = day;
                        pricesOfDay.Add(price);
                    }
                }
            }

            return pricesOfDay;
        }

        private static HtmlNode tryFindNodeUL(HtmlNode node)
        {
            while (node != null)
            {
                if (node.NodeType == HtmlNodeType.Element && node.Name == "ul")
                {
                    break;
                }
                node = node.NextSibling;
            }
            return node;
        }

        private static Price GetPriceDecriptionAndNumbers(HtmlNode htmlNode)
        {
            Price price = new Price();
            List<string> textNodes = new List<string>();

            //split Text for price and numbers. Find the <br>
            foreach (var childNode in htmlNode.ChildNodes)
            {
                // First TextType Child is the price
                // Second TextType Child is the numbers, seperated by ','                           
                if (childNode.NodeType == HtmlNodeType.Text)
                {
                    textNodes.Add(childNode.InnerText);
                }
            }
            if (textNodes.Count >= 2)
            {
                price = ParsePriceFromTexts( textNodes);
            }
            else if (textNodes.Count == 1) // WORKAORUND if there is no <br> -> only one text field -> seperate by the ":" 
                                           // TODO: sonething more intelligent that analyzes if strings are numbers as well
            {
                string[] newTexts = textNodes[0].Split(new[] { ':' });
                price = ParsePriceFromTexts( newTexts.ToList());
            }
            // TODO: ERROR handling..

            return price;
        }

        private static Price ParsePriceFromTexts(List<string> texts)
        {
            Price price = new Price();
            string description = texts[0];
            description = description.Trim();
            if (description.Last() == ':')
            {
                description = description.Remove(description.Length - 1); // remove last 
            }

            if (!string.IsNullOrEmpty(description))
            {
                price.Description = texts[0];
                string priceNumbers = texts[1].Replace("\n", String.Empty);
                priceNumbers = priceNumbers.Replace(" ", "");
                var numbers = priceNumbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var num in numbers)
                {
                    price.Numbers.Add(num);                  
                }
            }
            else
            {
                price.Description = "[ERROR]. Could not read out the price description.";
            }
            return price;
        }
    
    }
}