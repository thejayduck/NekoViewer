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

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
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
            //wbView.Settings.BuiltInZoomControls = true; wbView.Settings.DisplayZoomControls = true;
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            FindViewById<Button>(Resource.Id.btnGenerate).Click += (o, e) => GetImage();
            FindViewById<Button>(Resource.Id.btnDownload).Click += delegate
            {
                DownloadManager download = new DownloadManager(this, imagePanel, imageName);
                download.Execute(imageLink);
                Toast.MakeText(this, $"Downloading {imageName} from {imageLink}!", ToastLength.Long).Show();
            };
            CheckForPermissions();

            OnImageLinkGenerated += (string imageJson) =>
            {
                var json = new Org.Json.JSONObject(imageJson);
                imageLink = json.GetString("url");
                imagePanel.SetImageBitmap(GetImageBitmapFromUrl(imageLink));
                Toast.MakeText(this, "Generated New Image", ToastLength.Long).Show();
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