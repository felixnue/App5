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
    public static class PriceHelper
    {       

        //public static string GetPricesAsString(string searchNumber, string htmlString)
        //{
        //    List<Price> prices = PriceScraper.ExtractPrices(htmlString);
        //    return PrintPrices(prices, searchNumber);
        //}

        public static string GetPricesAsString(ICollection<string> searchNumbers, string htmlString)
        {
            List<Day> days = PriceScraper.ExtractPrices(htmlString);
            string strReturn = PrintIsWinnerOrLoser(searchNumbers, days);
            return string.Concat(strReturn, PrintDays(days));           
        }

        public static string PrintIsWinnerOrLoser(ICollection<string> searchNumbers, List<Day> days)
        {
            string stringWinner = string.Empty;

            for (int day = 24; day >= 1; day--)
            {
                Day dayN = days.SingleOrDefault(x => x.Date == day);
                if (dayN != null)
                {
                    foreach (var price in dayN.Prices)
                    {
                        foreach (var number in searchNumbers)
                        {
                            if (price.Numbers.Any(x => x.Equals(number)))
                            {
                                stringWinner += $"**** TADAAA! {number} WINS on day {dayN}! ****\nPrice: {price.Description}\n";
                            }
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

        public static string PrintDays(List<Day> days, bool numbersAsInt)
        {

            string pricesString = "Detected the following prices:\n";
            for (int day = 24; day >= 1; day--)
            {
                var dayN = days.SingleOrDefault(x => x.Date == day);
                if (dayN != null)
                {
                    pricesString += "Prices of day " + day + "\n";
                    foreach (var p in dayN.Prices)
                    {
                        string pricestring = numbersAsInt ? p.ToSortedIntString(true) : p.ToString();
                        pricesString = String.Concat(pricesString, " - ", pricestring + "\n");
                    }
                }
            }

            return pricesString;
          
        }

        public static string PrintDays(List<Day> days)
        {
            return PrintDays(days, false); 
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
            List<Day> days = PriceScraper.ExtractPrices(htmlString);

            List<Price> winners = new List<Price>();
            foreach (Day d in days)
            {
                winners.AddRange( d.GetWinningPrices(number));               
            }

            if (winners.Any())
            {
                return string.Join("\n", winners.Select(x => x.Description));
            }

            return string.Empty;
        }
    }
}