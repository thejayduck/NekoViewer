using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Lewd_Images
{
    class NotificationController
    {
        public static void CreateDownloadNotification(Context context, string title, string text, string path, Bitmap icon)
        {
            if (!Settings.Instance.DownloadNotificationEnabled)
                return;

            Android.Net.Uri uri = Android.Net.Uri.Parse(path);

            Intent intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(uri, "image/png");

            intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetContentIntent(pendingIntent)
                .SetStyle(new NotificationCompat.BigPictureStyle().BigPicture(icon).BigLargeIcon(null))
                .SetSubText("Click To Open The Image")
                .SetSmallIcon(Resource.Drawable.Icon);

            NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelId = $"{context.PackageName}.general";
                var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);

                notificationManager.CreateNotificationChannel(channel);

                builder.SetChannelId(channelId);
            }

            Notification notification = builder.Build();
            notification.Flags |= NotificationFlags.AutoCancel;

            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }
    }
}