using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LeoWinner
{
    internal static class PriceHelper
    {       

        //public static string GetPricesAsString(string searchNumber, string htmlString)
        //{
        //    List<Price> prices = PriceScraper.ExtractPrices(htmlString);
        //    return PrintPrices(prices, searchNumber);
        //}

        public static string GetPricesAsString(ICollection<string> searchNumbers, string htmlString)
        {
            List<Price> prices = PriceScraper.ExtractPrices(htmlString);
            string strReturn = PrintIsWinnerOrLoser(searchNumbers, prices);
            return string.Concat(strReturn,PrintPricesOnly(prices));           
        }

        public static string PrintIsWinnerOrLoser(ICollection<string> searchNumbers, List<Price> prices)
        {
            string stringWinner = string.Empty;

            for (int day = 24; day >= 1; day--)
            {
                foreach (var p in prices.Where(x => x.Day == day))
                {
                    foreach (var number in searchNumbers)
                    {
                        if (p.Numbers.Any(x => x.Equals(number)))
                        {
                            stringWinner += $"**** TADAAA! {number} WINS on day {day}! ****\nPrice: {p.Description}\n";
                        }
                    }
                }
            }
            if (stringWinner == String.Empty)
            {
                stringWinner = "Better luck next time :)\n";
            }

            return stringWinner;
        }


        private static string PrintPricesOnly(List<Price> prices)
        {
            string pricesString = "Detected the following prices:\n"; 
            for (int day = 24; day >= 1; day--)
            {
                if (prices.Any(x => x.Day == day))
                {
                    pricesString += "Prices of day " + day + "\n";
                }
                foreach (var p in prices.Where(x => x.Day == day))
                {
                    pricesString += p.ToString() + "\n";                   
                }
            }        

            return pricesString;
        }


        //private static string PrintPrices(List<Price> prices, string searchNumber)
        //{
        //    string pricesString = "Detected the following prices:\n";
        //    string stringWinner = string.Empty;

        //    for (int day = 24; day >= 1; day--)
        //    {
        //        if (prices.Any(x => x.Day == day))
        //        {
        //            pricesString += "Prices of day " + day + "\n";
        //        }

        //        foreach (var p in prices.Where(x => x.Day == day))
        //        {
        //            pricesString += p.ToString() + "\n";
        //            if (p.Numbers.Any(x => x.Equals(searchNumber)))
        //            {
        //                stringWinner += " \n******* WINNNER *******\n\n";
        //                stringWinner += " Your price: " + p.Description;
        //            }
        //        }
        //    }
        //    if (stringWinner == String.Empty)
        //    {
        //        stringWinner = searchNumber + ": -  Better luck next time :)";
        //    }
        //    else
        //    {
        //        // Toast.MakeText(this, stringWinner, ToastLength.Short).Show();
        //    }

        //    return stringWinner + "\n\n" + pricesString;
        //}

        // return string.Empty if no price found
        public static string GetPriceOfNUmber(string number, string htmlString)
        {
            List<Price> prices = PriceScraper.ExtractPrices(htmlString);
            var winner = prices.FirstOrDefault(x => x.Numbers.Contains(number));
            if (winner != null)
            {
                return winner.Description;
            }

            return string.Empty;
        }
    }
}