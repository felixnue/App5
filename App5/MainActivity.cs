using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using Android.Views;

namespace LeoWinner
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        string myHtml = string.Empty;
        TextView myTextViewOutput;
        EditText myTextNumberSearch;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            Button buttonSearch = FindViewById<Button>(Resource.Id.buttonSearch);
            buttonSearch.Click += OnButtonSearchClick;

            myTextNumberSearch = FindViewById<EditText>(Resource.Id.editTextNumber);
            myTextNumberSearch.KeyPress += OnTextNumberSEarch_KeyPress;

            myTextViewOutput = FindViewById<TextView>(Resource.Id.textViewOutput);
            myTextViewOutput.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
        }

        private void OnTextNumberSEarch_KeyPress(object sender, Android.Views.View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                try
                {
                    myTextViewOutput.Text = GetAllPrices(myTextNumberSearch.Text, myHtml);
                    myTextViewOutput.ScrollTo(0, 0);
                }
                catch (Exception ex)
                {
                    myTextViewOutput.Text = ex.Message;
                }

                finally
                {

                    e.Handled = true;
                }
            }
        }

        private async void OnButtonSearchClick(object sender, System.EventArgs e)
        {
            try
            {
                myHtml = await LoadStringFromUrl();
                myTextViewOutput.Text = GetAllPrices(myTextNumberSearch.Text, myHtml);
            }
            catch (Exception ex)
            {
                myTextViewOutput.Text = ex.Message;
            }

            myTextViewOutput.ScrollTo(0, 0);
        }

        private async Task<string> LoadStringFromUrl()
        {
            var uri = @"https://www.leo-erlangen.de/adventskalender/adventskalender-gewinnzahlen/";

            WebClient webClient = new WebClient();
            string htmlString = await webClient.DownloadStringTaskAsync(uri);

            return htmlString;
        }

        private string GetAllPrices(string searchNumber, string htmlString)
        {
            string pricesString = "Detected the following prices:\n";
            string stringWinner = string.Empty;

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);

            // var nodeTitle = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            //Console.WriteLine("Node Name: " + nodeTitle.Name + "\n" + nodeTitle.OuterHtml);
            //Console.WriteLine("\n\n");            

            for (int day = 24; day >= 1; day--)
            {

                List<Price> prices = new List<Price>();
                if (TryGetNodeForDay(day, htmlDoc, out HtmlNode nodeForDay))
                {
                    pricesString += "Prices of day " + day + "\n";
                    prices = GetPricesOfDay(day, nodeForDay);
                    foreach (var p in prices)
                    {
                        pricesString += p.ToString() + "\n";

                        if (p.Numbers.Any(x => x.Equals(searchNumber)))
                        {
                            stringWinner += " \n******* WINNNER *******\n\n";
                            stringWinner += " Your price: " + p.Description;
                        }
                    }
                }
            }

            if (stringWinner == String.Empty)
            {
                stringWinner = "Better luck next time :)";
            }
            else
            {
                Toast.MakeText(this, stringWinner, ToastLength.Short).Show();
            }
            return stringWinner + "\n\n" + pricesString;
        }

        private class Price
        {
            public Price()
            {
                Description = string.Empty;
                Numbers = new List<string>();
            }

            public string Description;
            public IList<string> Numbers;

            public override string ToString()
            {
                return String.Concat("\"", Description, "\"\n", String.Join(" | ", Numbers));
            }
        }


        private bool TryGetNodeForDay(int day, HtmlDocument htmlDoc, out HtmlNode nodeFound)
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

        private List<Price> GetPricesOfDay(int day, HtmlNode node)
        {
            List<Price> pricesOfDay = new List<Price>();

            //debudgString += ("Treffer für Tag " + dayString + "; InnerHtml: " + node.InnerHtml + "\n");
            //    Console.WriteLine("Parent 2: "+ n.ParentNode.ParentNode.OuterHtml);


            // This works for all but the first day... error on site?
            HtmlNode nodeUL = tryFindNodeUL(node.ParentNode.NextSibling);
            // Solution for entry 1:
            if (nodeUL == null)
            {
                nodeUL = tryFindNodeUL(node.NextSibling);
            }

            if (nodeUL != null && nodeUL.ChildNodes != null)
            {
                //sibling is the ul list of the numbers and prices
                foreach (HtmlNode child in nodeUL.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element && child.Name == "li")
                    {
                        Price price = ExtractPrice(child);
                        pricesOfDay.Add(price);
                    }
                }
            }

            //var listNodes = node.ParentNode.ParentNode.Descendants("ul");
            //foreach (var li in listNodes)
            //{
            //    // Console.WriteLine("-----------------------------------");
            //    // Console.WriteLine("ul InnerHtml: " + li.InnerHtml);	
            //}                   

            return pricesOfDay;
        }

        private HtmlNode tryFindNodeUL(HtmlNode node)
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

        private Price ExtractPrice(HtmlNode htmlNode)
        {
            Price price = new Price();
            List<string> texts = new List<string>();

            //split Text for price and numbers. Find the <br>
            foreach (var cc in htmlNode.ChildNodes)
            {
                // First TextType Child is the price
                // Second TextType Child is the numbers, seperated by ','                           
                if (cc.NodeType == HtmlNodeType.Text)
                {
                    texts.Add(cc.InnerText);
                }
            }
            if (texts.Count >= 2)
            {
                FillPriceFromTexts(price, texts);
            }
            else if (texts.Count == 1) // WORKAORUND if there is no <br> -> only one text field -> seperate by the ":" 
                                       // TODO: sonething more intelligent that analyzes if strings are numbers as well
            {
                string[] newTexts = texts[0].Split(new[] { ':' });
                FillPriceFromTexts(price, newTexts.ToList());
            }

            return price;
        }

        private static void FillPriceFromTexts(Price price, List<string> texts)
        {
            string description = texts[0];
            description = description.Trim();
            if (description.Last() == ':')
            {
                description.Remove(description.Length - 1); // remove last 
            }

            if (!string.IsNullOrEmpty(description))
            {
                price.Description = texts[0];

                // debudgString += ("!!! PriceDescription = " + texts[0] + "\n");
                // debudgString += ("Text 1= " + texts[1] + "\n");

                string priceNumbers = texts[1].Replace("\n", String.Empty);
                priceNumbers = priceNumbers.Replace(" ", "");
                var numbers = priceNumbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var num in numbers)
                {
                    price.Numbers.Add(num);
                    // debudgString += ("!!! number found: " + num + "\n");
                }
            }
        }
    }


}

