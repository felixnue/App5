﻿using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace LeoWinner
{
    public  class PriceScraper
    {
        public static bool IsUpToDate(int day, string htmlString)
        {
            List<Day> days = ExtractPrices(htmlString);
            return days.Any(x => x.Date == day);
        }

        public static List<Day> ExtractPrices(string htmlString)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);

            List<Day> days = new List<Day>();
            for (int d = 24; d >= 1; d--)
            {
                var prices = GetPricesOfDay(d, htmlDoc);
                if (prices.Any())
                {                   
                    Day day = new Day(d);
                    day.Prices = prices;
                    days.Add(day);
                }
             
            }

            return days;
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

        public static string GetHeaderStringForDay(int day, HtmlDocument htmlDoc)
        {
            string returnString = string.Empty;
            if (TryGetHtmlNodeForDay(day, htmlDoc, out HtmlNode nodeFound))
            {
                returnString = nodeFound.InnerText;
            }
            return returnString;
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

            //  debudgString += ("Treffer für Tag " + dayString + "; InnerHtml: " + node.InnerHtml + "\n");
            //  Console.WriteLine("Parent 2: "+ n.ParentNode.ParentNode.OuterHtml);

            // This works for all but the first day... error on site?
            bool success = TryFindNodeULInSiblings(nodeOfDay.ParentNode.NextSibling, out var nodeUL);
            // Solution for entry 1:
            if (!success)
            {
                success = TryFindNodeULInSiblings(nodeOfDay.NextSibling, out nodeUL);
            }

            if (success && nodeUL.ChildNodes != null)
            {
                //sibling is the ul list of the numbers and prices
                foreach (HtmlNode child in nodeUL.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element && child.Name == "li")
                    {
                        try
                        {

                            Price price = GetPriceDecriptionAndNumbers(child);
                            price.Day = day;
                            pricesOfDay.Add(price);
                        }
                        catch (Exception ex)
                        {
                            pricesOfDay.Add(new Price() { Description = $"ERROR parsing day {day}. Message: \"{ex.Message}\"" });
                        }
                    }
                }
            }

            return pricesOfDay;
        }

        private static bool TryFindNodeULInSiblings(HtmlNode node, out HtmlNode ulNode)
        {
            bool success = false;

            while (node != null)
            {
                if (node.NodeType == HtmlNodeType.Element && node.Name == "ul")
                {
                    success = true;
                    break;
                }
                node = node.NextSibling;
            }

            ulNode = node;
            return success;
        }

        private static Price GetPriceDecriptionAndNumbers(HtmlNode htmlNode)
        {
            bool success = false;
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
                success =  TryParsePriceFrom2Texts(textNodes, out price);
            }
            else if (textNodes.Count == 1) // WORKAORUND if there is no <br> -> only one text field -> seperate by the ":" 
                                           // TODO: something more intelligent that analyzes if strings are numbers as well
            {
                string[] newTexts = textNodes[0].Split(new[] { ':' });
                success = TryParsePriceFrom2Texts(newTexts.ToList(), out price);
            }

            if (!success || !price.Numbers.Any()) //Error handling try differently, if the numbers are not inside the <li> element!
            {
                // All following Textnodes are considered to contain numbers potentially..

                var textsOfAllSiblings = GetTextsOfAllTextElementsInSiblings(htmlNode.NextSibling);
                price.Numbers = ParseAllNumbers(textsOfAllSiblings);
            }

            if (!success || !price.Numbers.Any())
            {
                //...
            }

            return price;
        }



        private static List<string> GetTextsOfAllTextElementsInSiblings(HtmlNode node)
        {
            List<string> texts = new List<string>();
            while (node != null)
            {
                //Console.WriteLine("Nodes next sibling: "+ node.OuterHtml);						
                if (node.NodeType == HtmlNodeType.Text)
                {
                    texts.Add(node.InnerText);
                    Console.WriteLine("New node of type " + node.NodeType + " found!!!: " + node.OuterHtml);
                    //break;
                }
                node = node.NextSibling;
            }

            return texts;
        }

        private static bool TryParsePriceFrom2Texts(List<string> texts, out Price price)
        {
            bool canParse = false;
            price = new Price();

            string description = ParseDescription(texts.First());
            if (!string.IsNullOrEmpty(description))
            {                
                price.Description = description;
                price.Numbers = ParseNumbers(texts[1]);
                canParse = true;
            }
            
            return canParse;
        }

        private static IList<string> ParseAllNumbers(List<string> textsOfAllSiblings)
        {
            List<string> numbbers = new List<string>();
            foreach (var text in textsOfAllSiblings.Where(x => !string.IsNullOrEmpty(x)))
            {
                numbbers.AddRange(ParseNumbers(text));
            }

            return numbbers;
        }

        private static IList<string> ParseNumbers(string text)
        {
            string priceNumbers = text.Replace("\n", String.Empty);
            priceNumbers = priceNumbers.Replace(" ", "");
            List<string> numbers = priceNumbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return numbers;
        }

        private static string ParseDescription(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            string description = text.Trim();
            if (!string.IsNullOrEmpty(description) && description.Last() == ':')
            {
                description = description.Remove(description.Length - 1); // remove last 
            }

            return description;
        }
    }

    public class Day {

        public Day(int date)
        {
            Date = date;
            Header = "undefined";
            Prices = new List<Price>();
        }

        public int Date { get; private set; }
        public string Header;
        public List<Price> Prices;

        public IList<Price> GetWinningPrices(string number)
        {
            return Prices.Where(x => x.IsWinner(number)).ToList();
        }

    }

    public class Price
    {
        internal Price()
        {
            Header = "undefined";
            Day = -1;
            Description = "undefined";
            Numbers = new List<string>();
        }

        public int Day;
        public string Description;
        public string Header;
        public IList<string> Numbers;
        public IEnumerable<int> IntNumbers
        {
            get
            {
                foreach(string num in Numbers)
                {
                    int result = -1;
                    if (Int32.TryParse(num, out result))
                    {
                        yield return result;
                    }
                }                
            }
        }
       
        public bool IsWinner(string number)
        {
            return Numbers.Any(x => x.Equals(number, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string ToString()
        {
            return ToSortedIntString(false);
        }

        public string ToSortedIntString(bool sorted)
        {
            string numbers = sorted ? IntNumbersAsString(true) : NumbersAsString();          
            return String.Concat("\"", Description, "\"\n    ", numbers);
        }

        public string IntNumbersAsString(bool sorted)
        {
            var numbers = IntNumbers.ToList();

            if (sorted)
            {
                numbers.Sort();
            }

            return string.Concat("[", String.Join("] [", numbers), "]");
        }

        public string NumbersAsString()
        {
            return string.Concat( "[", String.Join("] [", Numbers), "]");
        }
    }
}