using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Util;


namespace LeoWinner
{
    [BroadcastReceiver]
    public class AlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var preferences = context.GetSharedPreferences("myPrefs", FileCreationMode.Private);
            var numbers = preferences.GetStringSet("keyNumbers", new HashSet<string>()).ToList();
            // string number = intent.GetStringExtra("number");

            //  Toast.MakeText(context, "Leo Winner scheduler searching for number " + number, ToastLength.Long).Show();

            var html = Task.Run(MainActivity.LoadStringFromUrl).Result;
            List<Price> prices = PriceScraper.ExtractPrices(html);
            string searchResult =  PriceHelper.PrintIsWinnerOrLoser(numbers, prices);          

            MakeNotification(context, searchResult);
        }

        private void MakeNotification(Context context, string searchResult)
        {
            var resultIntent = new Intent(context, typeof(MainActivity));
            // resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            resultIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
 
             var pending = PendingIntent.GetActivity(context, 0, resultIntent, 0);

            string title = "Leo Winner search";
            string text = searchResult;
            
            CreateNotificationChannel(context);
            var builder = new NotificationCompat.Builder(context, "LEO_WINNER_CHANNEL_ID")
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.notification_template_icon_bg);
            builder.SetContentIntent(pending);
            var notification = builder.Build();
            var manager = NotificationManager.FromContext(context);

            int numberInt = -1;
            manager.Notify(text, numberInt, notification);            
        }

        private void CreateNotificationChannel(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string name = context.Resources.GetString(Resource.String.channel_name);
                   
                var descriptionText = context.Resources.GetString(Resource.String.channel_description);
                var importance = NotificationImportance.Default;
                var channel = new NotificationChannel("LEO_WINNER_CHANNEL_ID", name, importance);
              //  channel.Description = descriptionText;

                // Register the channel with the system; you can't change the importance
                // or other notification behaviors after this 

                NotificationManager notificationManager = NotificationManager.FromContext(context);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}