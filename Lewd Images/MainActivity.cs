using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.Net;
using Android.Webkit;
using System.IO;
using System;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Graphics;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        ImageView imagePanel;
        string imageLink;
        private static String[] PERMISSIONS_INTERNET = { Manifest.Permission.Internet, Manifest.Permission.WriteExternalStorage };
        private static int REQUEST_INTERNET = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            //wbView.Settings.BuiltInZoomControls = true; wbView.Settings.DisplayZoomControls = true;
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            FindViewById<Button>(Resource.Id.btnGenerate).Click += (o, e) => GetImage();
            FindViewById<Button>(Resource.Id.btnDownload).Click += delegate
            {
                DownloadManager download = new DownloadManager(this, imagePanel, imageLink.Split('"')[3]);
                download.Execute(imageLink.Split('"')[3]);
                Toast.MakeText(this, $"Downloading {imageLink.Split('"')[3]}!", ToastLength.Long).Show();
            };
            CheckForPermissions();
        }

        private void CheckForPermissions()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_INTERNET, REQUEST_INTERNET);
            }
        }

        void GetImage()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://nekos.life/api/v2/img/lewd");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                imageLink = reader.ReadToEnd();
            }

            imagePanel.SetImageBitmap(GetImageBitmapFromUrl(imageLink.Split('"')[3]));
            Toast.MakeText(this, "Generated New Image", ToastLength.Long).Show();
        }
        public Bitmap GetImageBitmapFromUrl(string url)
        {
            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0) return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            }
            return null;
        }
    }
}