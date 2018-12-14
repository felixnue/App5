﻿using Android.App;
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
using Java.Util;
using Java.Lang;

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

            Button buttonRefresh = FindViewById<Button>(Resource.Id.buttonRefresh);
            buttonRefresh.Click += OnButtonRefreshClick;

            myTextNumberSearch = FindViewById<EditText>(Resource.Id.editTextNumber);
            myTextNumberSearch.KeyPress += OnTextNumberSEarch_KeyPress;

            myTextViewOutput = FindViewById<TextView>(Resource.Id.textViewOutput);
            myTextViewOutput.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();

            Button buttonAlarm = FindViewById<Button>(Resource.Id.buttonAlarm);
            buttonAlarm.Click += OnButtonAlarmCLick;

        }        

        private async void OnTextNumberSEarch_KeyPress(object sender, Android.Views.View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                try
                {
                    if(string.IsNullOrEmpty(myHtml))
                    {
                        await RefreshHtml();
                    }
                    myTextViewOutput.Text = PriceScraper.GetAllPrices(myTextNumberSearch.Text, myHtml);
                    myTextViewOutput.ScrollTo(0, 0);
                }
                catch (System.Exception ex)
                {
                    myTextViewOutput.Text = ex.Message;
                }

                finally
                {

                    e.Handled = true;
                }
            }
        }

        private async Task RefreshHtml()
        {
            myTextViewOutput.Text = "Loading website html data...";
            myHtml = await PriceScraper.LoadStringFromUrl();
        }

        private async void OnButtonRefreshClick(object sender, System.EventArgs e)
        {
            try
            {
                // Remind(DateTime.Now, "Search BUtton", "Was clicked");
                await RefreshHtml();
                myTextViewOutput.Text = PriceScraper.GetAllPrices(myTextNumberSearch.Text, myHtml);
            }
            catch (System.Exception ex)
            {
                myTextViewOutput.Text = ex.Message;
            }

            myTextViewOutput.ScrollTo(0, 0);
        }


        private void OnButtonAlarmCLick(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty( myTextNumberSearch.Text) ) { return; }

            SetRepeatingAlarm(myTextNumberSearch.Text);
        }

        private void SetRepeatingAlarm(string number)
        {
            //GET TIME IN SECONDS AND INITIALIZE INTENT

            Intent receiverIntent = new Intent(this, typeof(AlarmReceiver));
            receiverIntent.PutExtra("number", number);

            //PASS CONTEXT,YOUR PRIVATE REQUEST CODE,INTENT OBJECT AND FLAG
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, receiverIntent, PendingIntentFlags.CancelCurrent);

            //INITIALIZE ALARM MANAGER
            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);

            Calendar calendar = Calendar.Instance;
            calendar.TimeInMillis = (JavaSystem.CurrentTimeMillis());
            //calendar.Add(CalendarField.Second, secondsTillAlarm);
            calendar.Add(CalendarField.DayOfYear, 1); // tomorrow            
            calendar.Set(CalendarField.HourOfDay, 8);
            calendar.Set(CalendarField.Minute, 15); // 15 min

            //SET THE ALARM

            // alarmManager.Set(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis() + (5 * 1000), pendingIntent);
              alarmManager.SetInexactRepeating(AlarmType.RtcWakeup, calendar.TimeInMillis, AlarmManager.IntervalDay , pendingIntent);

            myTextViewOutput.Text = number + ": Repeating daily Alarm set. Next at: " + calendar.Time.ToString();
          //  Toast.MakeText(this, ToastLength.Long).Show();
          
        }
    }
}

