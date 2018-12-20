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
using Android.Content;
using Android.App.Job;
//using Java.Util;
using Java.Lang;

namespace LeoWinner
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        string myHtml = string.Empty;
        TextView myTextViewOutput;
        EditText myTextNumberSearch;
        ICollection<string> myNumbers = new List<string>();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            
            InitNumbers();

            Button buttAdd = FindViewById<Button>(Resource.Id.buttonAddNumber);
            buttAdd.Click += OnButtonAddClick;
            Button buttDelete = FindViewById<Button>(Resource.Id.buttonDeleteNumber);
            buttDelete.Click += OnButtonDeletNumberClick;
            Button buttonRefresh = FindViewById<Button>(Resource.Id.buttonRefresh);
            buttonRefresh.Click += OnButtonRefreshClick;           

            myTextNumberSearch = FindViewById<EditText>(Resource.Id.editTextNumber);
            myTextViewOutput = FindViewById<TextView>(Resource.Id.textViewOutput);
            myTextViewOutput.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
            Button buttonAlarm = FindViewById<Button>(Resource.Id.buttonAlarm);
            buttonAlarm.Click += OnButtonAlarmCLick;
            Button buttonTestAlarm = FindViewById<Button>(Resource.Id.buttonTestAlarm);
            buttonTestAlarm.Click += OnButtonTestAlarmClick;


            await RefreshHtml();
            UpdateSearchResults();
        }

        

        private void InitNumbers()
        {
            var preferences = this.GetSharedPreferences("myPrefs", FileCreationMode.Private);

            myNumbers = preferences.GetStringSet("keyNumbers", new HashSet<string>()).ToList();

            ShowNumbers();
        }

        private void OnButtonDeletNumberClick(object sender, EventArgs e)
        {
            string newNumber = myTextNumberSearch.Text;
            if (string.IsNullOrEmpty(newNumber) || !myNumbers.Contains(newNumber))
            {
                return;
            }
            myNumbers.Remove(newNumber);
            SafeAndShowNumbers();
            UpdateSearchResults();
        }

        private void OnButtonAddClick(object sender, EventArgs e)
        {
            string newNumber = myTextNumberSearch.Text;
            if (string.IsNullOrEmpty(newNumber) || myNumbers.Contains(newNumber))
            {
                return;
            }
            myNumbers.Add(newNumber);
            SafeAndShowNumbers();
            UpdateSearchResults();
        }

        private async void OnButtonRefreshClick(object sender, System.EventArgs e)
        {
            await RefreshHtml();
            UpdateSearchResults();
            myTextViewOutput.ScrollTo(0, 0);
        }       

        private void OnButtonAlarmCLick(object sender, EventArgs e)
        {
            if (myNumbers == null || !myNumbers.Any()) { return; }
            SetRepeatingAlarm(myNumbers, false);
        }

        private void OnButtonTestAlarmClick(object sender, EventArgs e)
        {
            if (myNumbers == null || !myNumbers.Any()) { return; }
            SetRepeatingAlarm(myNumbers, true);
        }



        private  void UpdateSearchResults()
        {
            try
            {
                myTextViewOutput.Text = PriceHelper.GetPricesAsString(myNumbers, myHtml);
                myTextViewOutput.ScrollTo(0, 0);
            }
            catch (System.Exception ex)
            {
                myTextViewOutput.Text = ex.Message;
            }
        }

        private void SafeAndShowNumbers()
        {
            var preferences = GetSharedPreferences("myPrefs", FileCreationMode.Private);
            var editor = preferences.Edit();
            editor.PutStringSet("keyNumbers", new HashSet<string>(myNumbers));
            editor.Commit();

            ShowNumbers();
        }

        private void ShowNumbers()
        {
            TextView textViewNumbers = FindViewById<TextView>(Resource.Id.textViewNumbers);
            textViewNumbers.Text = string.Join(", ", myNumbers);
        }

        private async Task RefreshHtml()
        {
            myTextViewOutput.Text = "Loading website html data...";
            myHtml = await LoadStringFromUrl();
        }

        public static async Task<string> LoadStringFromUrl()
        {
            var uri = @"https://www.leo-erlangen.de/adventskalender/adventskalender-gewinnzahlen/";

            WebClient webClient = new WebClient();
            string htmlString = await webClient.DownloadStringTaskAsync(uri);

            return htmlString;
        }

       

        private void SetRepeatingAlarm(ICollection<string> numbers, bool isTest)
        {
            Intent receiverIntent = new Intent(this, typeof(AlarmReceiver));           
            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService); 

            if (isTest)
            {
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, receiverIntent, PendingIntentFlags.CancelCurrent);

                int seconds = 5;              
                alarmManager.Set(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis() + (seconds * 1000), pendingIntent);
                myTextViewOutput.Text = $"Test Alarm set in {seconds} seconds.";
            }
            else
            {
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 1, receiverIntent, PendingIntentFlags.CancelCurrent);

                Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                calendar.TimeInMillis = (JavaSystem.CurrentTimeMillis());                       
                calendar.Set(Java.Util.CalendarField.HourOfDay, 9);
                calendar.Set(Java.Util.CalendarField.Minute, 0);
                calendar.Set(Java.Util.CalendarField.Second, 0);

                alarmManager.SetRepeating(AlarmType.RtcWakeup, calendar.TimeInMillis, AlarmManager.IntervalDay, pendingIntent);
                myTextViewOutput.Text = "Repeating daily Alarm set. Begin at: " + calendar.Time.ToString();
            }
        }
    }
}

