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
using System.Linq;
using Android.Support.V4.Widget;
using System.Collections.Generic;
using Android.Support.Design.Widget;
using Android.Views;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppCompat", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        List<Bitmap> images = new List<Bitmap>();
        ImageView imagePanel;
        string imageLink;
        string imageName => imageLink.Split('/').Last();
        private static string[] PERMISSIONS = { Manifest.Permission.Internet, Manifest.Permission.WriteExternalStorage };
        private static int REQUEST_INTERNET = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            CheckForPermissions();

            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            FloatingActionButton nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);

            FindViewById<Button>(Resource.Id.btnDownload).Click += delegate
            {
                DownloadManager download = new DownloadManager(this, imagePanel, imageName);
                download.Execute(imageLink);
                Toast.MakeText(this, $"Downloading {imageName} from {imageLink}!", ToastLength.Long).Show();
            };

            nextImageButton.Click += (o, e) =>
            {
                Toast.MakeText(this, "Generating New Image", ToastLength.Short).Show();
                GetImage();
            };
            OnImageLinkGenerated += (string imageJson) =>
            {
                var json = new Org.Json.JSONObject(imageJson);
                imageLink = json.GetString("url");
                imagePanel.SetImageBitmap(GetImageBitmapFromUrl(imageLink));
            };
        }

        private void CheckForPermissions()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS, REQUEST_INTERNET);
            }
        }


        public event Action<string> OnImageLinkGenerated;

        void GetImage()
        {
            string apiResponse = "";
            using (HttpWebResponse response = NekosLife.Request("lewd"))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                apiResponse = reader.ReadToEnd();
            }
            OnImageLinkGenerated.Invoke(apiResponse);
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