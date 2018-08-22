using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using System.Net;
using System.IO;
using System;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Graphics;
using System.Collections.Generic;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Java.IO;
using Android.Views;
using Android.Content;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        Bitmap bufferImage;
        Bitmap currentImage;
        List<string> images = new List<string>();
        ImageView imagePanel;
        Spinner tagSpinner;
        string imageLink;
        string imageName => System.IO.Path.GetFileNameWithoutExtension(imageLink);
        private static string[] PERMISSIONS = { Manifest.Permission.WriteExternalStorage };
        private static int REQUEST_PERMISSION = 1;

        private static int DefaultTag => 0;
        private string SelectedTag {
            get {
                if (tagSpinner.SelectedItemPosition >= 0)
                    return NekosLife.Tags[tagSpinner.SelectedItemPosition];
                else
                    return NekosLife.Tags[DefaultTag];
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            CheckForPermissions();

            tagSpinner = FindViewById<Spinner>(Resource.Id.tagSpinner);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            FloatingActionButton nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);
            FloatingActionButton previousImageButton = FindViewById<FloatingActionButton>(Resource.Id.previousImageButton);


            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Lewd Viewer";


            var adapter = new ArrayAdapter<string>(this, tagSpinner.Id, NekosLife.Tags);

            FindViewById<Button>(Resource.Id.btnDownload).Click += delegate
            {
                MemoryStream buffer = new MemoryStream();
                currentImage.Compress(Bitmap.CompressFormat.Png, 0, buffer);
                buffer.Seek(0, SeekOrigin.Begin);
                BufferedInputStream stream = new BufferedInputStream(buffer);
                DownloadManager download = new DownloadManager(this, stream, buffer.Length);
                download.Execute(imageName + ".png");
                Toast.MakeText(this, $"Downloading {imageName} from {imageLink}!", ToastLength.Long).Show();
            };

            string oldSelected = "";

            nextImageButton.Click += (o,e) =>
            {
                Toast.MakeText(this, "Generating New Image", ToastLength.Short).Show();
                if (oldSelected != SelectedTag)
                    RequestImage(SelectedTag);
                imagePanel.SetImageBitmap(currentImage = bufferImage);
                Task.Factory.StartNew(() =>
                {
                    RequestImage(SelectedTag);
                });
                oldSelected = SelectedTag;
            };
            previousImageButton.Click += (o, e) =>
            {

            };

            OnImageRecieved += (Bitmap image) =>
            {
                bufferImage = image;
            };
        }

        public override void OnBackPressed()
        {
            Android.App.AlertDialog.Builder aDialog;
            aDialog = new Android.App.AlertDialog.Builder(this);
            aDialog.SetTitle("Are You Sure About Quitting?");
            aDialog.SetNeutralButton("YES", delegate { Process.KillProcess(Process.MyPid()); });
            aDialog.SetNegativeButton("NO", delegate { aDialog.Dispose(); });
            aDialog.Show();

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Android.App.AlertDialog.Builder aDialog;
            aDialog = new Android.App.AlertDialog.Builder(this);

            if (item.ItemId == Resource.Id.menu_info) 
            {
                aDialog.SetTitle("App Info");
                aDialog.SetMessage("Made By: \n Jay and Nobbele \n Images From: \n Nekos.life   ");
                aDialog.SetNeutralButton("OK", delegate { aDialog.Dispose(); });
                aDialog.Show();

            }
            if (item.ItemId == Resource.Id.menu_credits) 
            {
                Toast.MakeText(this, "Made By:" +
                    "\n Jay and Nobbele" +
                    "\n Images From:" +
                    "\n Nekos.Life", ToastLength.Long).Show();
            }
            return base.OnOptionsItemSelected(item);
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //TODO (reload stuff with new tag)
        }

        private void CheckForPermissions()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted) 
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS, REQUEST_PERMISSION);
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
            images.Add(imageLink = json.GetString("url"));
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