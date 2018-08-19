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
using System.Threading.Tasks;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        Bitmap[] buffer = new Bitmap[2];
        List<Bitmap> images = new List<Bitmap>();
        ImageView imagePanel;
        Spinner tagSpinner;
        string imageLink;
        string imageName => imageLink.Split('/').Last();
        private static string[] PERMISSIONS = { Manifest.Permission.Internet, Manifest.Permission.WriteExternalStorage };
        private static int REQUEST_INTERNET = 1;

        private static int DefaultTag => 0;
        private string SelectedTag {
            get {
                if (tagSpinner.SelectedItemPosition >= 0)
                    return NekosLife.Tags[tagSpinner.SelectedItemPosition];
                else
                    return NekosLife.Tags[DefaultTag];
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            CheckForPermissions();

            tagSpinner = FindViewById<Spinner>(Resource.Id.tagSpinner);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            FloatingActionButton nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);

            var adapter = new ArrayAdapter<string>(this, tagSpinner.Id, NekosLife.Tags);

            FindViewById<Button>(Resource.Id.btnDownload).Click += delegate
            {
                DownloadManager download = new DownloadManager(this, imagePanel, imageName);
                download.Execute(imageLink);
                Toast.MakeText(this, $"Downloading {imageName} from {imageLink}!", ToastLength.Long).Show();
            };

            string oldSelected = "";

            nextImageButton.Click += (o, e) =>
            {
                Toast.MakeText(this, "Generating New Image", ToastLength.Short).Show();
                if (oldSelected != SelectedTag)
                    RequestImage(SelectedTag);
                buffer[0] = buffer[1];
                imagePanel.SetImageBitmap(buffer[0]);
                Task.Factory.StartNew(() =>
                {
                    RequestImage(SelectedTag);
                });
                oldSelected = SelectedTag;
            };
            OnImageRecieved += (Bitmap image) =>
            {
                buffer[1] = image;
                //imagePanel.SetImageBitmap(image);
                images.Add(image);
            };
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
        }

        private void CheckForPermissions()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS, REQUEST_INTERNET);
            }
        }


        public event Action<Bitmap> OnImageRecieved;

        void RequestImage(string tag)
        {
            string apiResponse = "";
            using (HttpWebResponse response = NekosLife.Request(tag))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                apiResponse = reader.ReadToEnd();
            }

            var json = new Org.Json.JSONObject(apiResponse);
            imageLink = json.GetString("url");
            Bitmap image = GetImageBitmapFromUrl(imageLink);
            OnImageRecieved.Invoke(image);
        }
        public Bitmap GetImageBitmapFromUrl(string url)
        {
            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                    return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            }
            return null;
        }
    }
}