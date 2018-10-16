using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Lewd_Images
{
    class NotificationController
    {
        public void SendNotification(Context _context, string _title, string _text)
        {
            NotificationCompat.Builder builder = new NotificationCompat.Builder(_context)
                .SetContentTitle("Sample Notification")
                .SetContentText("Hello World! This is my first notification!")
                .SetSmallIcon(Resource.Mipmap.ic_launcher);

            Notification notification = builder.Build();

            NotificationManager notificationManager = GetSystemService(Context) as NotificationManager;

            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }
    }
}