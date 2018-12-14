using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LeoWinner
{
    [BroadcastReceiver] 
    public class AlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            string number = intent.GetStringExtra("number");

            Toast.MakeText(context, "Alarm Ringing! Number = " + number, ToastLength.Short).Show();

            var html =  Task.Run(PriceScraper.LoadStringFromUrl).Result;
            string price = PriceScraper.GetPriceOfNUmber(number, html);

            MakeNotification(context, price, number);

        }

        private void MakeNotification(Context context, string price, string number)
        {
            var resultIntent = new Intent(context, typeof(MainActivity));
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            var pending = PendingIntent.GetActivity(context, 0, resultIntent, 0);

            string title = "No price found today :(";
            string text = "Number: " + number;
            if (!String.IsNullOrEmpty( price))
            {
                title = "You are a WINNER!";
                text += " Price: " + price;
            }

            var builder = new Notification.Builder(context)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.notification_template_icon_bg);
            builder.SetContentIntent(pending);
            var notification = builder.Build();
            var manager = NotificationManager.FromContext(context);
            manager.Notify(1337, notification);
        }
    }
}