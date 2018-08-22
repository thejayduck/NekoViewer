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
        int index = 0;
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

            imagePanel.LongClick += (o, e) =>
            {
                if(imagePanel.Drawable == null)
                {
                    Toast.MakeText(this, "No Images Were Found!", ToastLength.Short).Show();
                    return;
                }

                Android.App.AlertDialog.Builder aDialog;
                aDialog = new Android.App.AlertDialog.Builder(this);
                aDialog.SetTitle("Image Download Request");
                aDialog.SetMessage("Are you sure about downloading this image?");
                aDialog.SetPositiveButton("YES", delegate 
                {
                    MemoryStream buffer = new MemoryStream();
                    currentImage.Compress(Bitmap.CompressFormat.Png, 0, buffer);
                    buffer.Seek(0, SeekOrigin.Begin);
                    BufferedInputStream stream = new BufferedInputStream(buffer);
                    DownloadManager download = new DownloadManager(this, stream, buffer.Length);
                    download.Execute(imageName + ".png");
                    Toast.MakeText(this, $"Downloading {imageName} from {imageLink}!", ToastLength.Long).Show();
                });
                aDialog.SetNegativeButton("NO", delegate { aDialog.Dispose(); });
                aDialog.Show();
            };

            nextImageButton.Click += (o,e) =>
            {
                Toast.MakeText(this, "Generating New Image", ToastLength.Short).Show();
                if (oldSelected != SelectedTag)
                    RequestNewImage(SelectedTag);
                SetCurrentImage();
                Task.Factory.StartNew(() =>
                {
                    RequestNewImage(SelectedTag);
                });
                oldSelected = SelectedTag;
            };
            previousImageButton.Click += (o, e) =>
            {
                RequestOldImage(index - 3);
            };

            OnImageRecieved += (Bitmap image) =>
            {
                bufferImage = image;
            };
        }

        string oldSelected = "";

        public void SetCurrentImage()
        {
            imagePanel.SetImageBitmap(currentImage = bufferImage);
        }

        public override void OnBackPressed()
        {
            Android.App.AlertDialog.Builder aDialog;
            aDialog = new Android.App.AlertDialog.Builder(this);
            aDialog.SetTitle("Are You Sure About Quitting?");
            aDialog.SetPositiveButton("YES", delegate { Process.KillProcess(Process.MyPid()); });
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
                aDialog.SetMessage("Made By: \n Jay and Nobbele \n Images From \n Nekos.life   ");
                aDialog.SetNeutralButton("OK", delegate { aDialog.Dispose(); });
                aDialog.Show();
            }
            if(item.ItemId == Resource.Id.menu_icon)
            {
                Toast.MakeText(this, "Orokana hentai", ToastLength.Short).Show();
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

        void UseApiResponse(Org.Json.JSONObject json)
        {
            Bitmap image = GetImage(imageLink = json.GetString("url"));
            images.Add(imageLink);
            OnImageRecieved.Invoke(image);
        }
        Bitmap GetImage(string imageLink)
        {
            index++;
            return GetImageBitmapFromUrl(imageLink);
        }

        void RequestNewImage(string tag)
        {
            if (images.Count <= index)
            {
                string apiResponse = "";
                using (HttpWebResponse response = NekosLife.Request(tag))
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    apiResponse = reader.ReadToEnd();
                }

                var json = new Org.Json.JSONObject(apiResponse);
                UseApiResponse(json);
            } else
            {
                RequestOldImage(index);
            }
        }
        void RequestOldImage(int i)
        {
            imageLink = images[index = i];
            Bitmap image = GetImageBitmapFromUrl(imageLink);

            i++;
            OnImageRecieved(image);
            SetCurrentImage();
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