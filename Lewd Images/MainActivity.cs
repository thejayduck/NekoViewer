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
using System.Collections;
//using Felipecsl.GifImageViewLib;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        //#FD4281 (253, 66, 129, 100) - pink button color
        //#424040 (66, 64, 64, 100) - faded out pink color

        //Buttons
        FloatingActionButton nextImageButton;
        FloatingActionButton previousImageButton;

        //Tags
        ArrayAdapter nekosTagAdapter;
        ArrayList nekosTags;
        readonly List<string> images = new List<string>();
        ImageView imagePanel;
        Spinner tagSpinner;
        string ImageName => System.IO.Path.GetFileNameWithoutExtension(imageStore.GetLink());
        private static readonly string[] PERMISSIONS = { Manifest.Permission.WriteExternalStorage, Manifest.Permission.Internet };
        private static readonly int REQUEST_PERMISSION = 1;

        private string SelectedTag {
            get {
                if (tagSpinner.SelectedItemPosition >= 0)
                    return NekosLife.Tags[tagSpinner.SelectedItemPosition];
                else
                    return NekosLife.DefaultTag;
            }
        }

        LewdImageStore imageStore = new LewdImageStore();

        protected override void OnCreate(Bundle bundle) 
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            CheckForPermissions();

            //Finding Resources
            nekosTags = new ArrayList();
            tagSpinner = FindViewById<Spinner>(Resource.Id.tagSpinner);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);
            previousImageButton = FindViewById<FloatingActionButton>(Resource.Id.previousImageButton);

            //Toolbar Configurations
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Lewd Viewer";

            foreach(string i in NekosLife.Tags)
            {
                nekosTags.Add(i);
            }

            nekosTagAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, nekosTags);
            tagSpinner.Adapter = nekosTagAdapter;

            tagSpinner.ItemSelected += (o, e) =>
            {
                imageStore.Tag = SelectedTag;
                Toast.MakeText(this, $"Selected {SelectedTag}", ToastLength.Short).Show();
            };

            //Request image download vvv
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
                    download.Execute(ImageName + ".png");
                    Toast.MakeText(this, $"Downloading {ImageName} from {imageStore.GetLink()}!", ToastLength.Short).Show();
                });
                aDialog.SetNegativeButton("NO", delegate { aDialog.Dispose(); });
                aDialog.Show();
            };

            //Buttons Functions
            nextImageButton.Click += (o,e) =>
            {
                Toast.MakeText(this, "Forwards", ToastLength.Short).Show();
                imageStore.Forward();
                ReloadImagePanel();
                CheckPreviousImageButton();
            };
            nextImageButton.LongClick += (o, e) =>
            {
                Toast.MakeText(this, "Going Back To Last Image", ToastLength.Short).Show();
                imageStore.GotoLast();
                ReloadImagePanel();
                CheckPreviousImageButton();
            };
            previousImageButton.Click += (o, e) =>
            {
                Toast.MakeText(this, "Backwards", ToastLength.Short).Show();
                imageStore.Back();
                ReloadImagePanel();
                CheckPreviousImageButton();
            };
        }

        public void CheckPreviousImageButton()
        {
            previousImageButton.Visibility = imageStore.IsFirst ? ViewStates.Invisible : ViewStates.Visible;
        }

        Bitmap currentImage;

        public void ReloadImagePanel()
        {
            imagePanel.SetImageBitmap(currentImage = imageStore.GetImage());
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
                aDialog.SetMessage("Made By: \n Jay and Nobbele \n Images From \n Nekos.life");
                aDialog.SetNeutralButton("OK", delegate { aDialog.Dispose(); });
                aDialog.Show();
            }
            if (item.ItemId == Resource.Id.menu_icon)
            {
                Toast.MakeText(this, "Orokana hentai", ToastLength.Short).Show();
            }
            if (item.ItemId == Resource.Id.menu_help)
            {
                aDialog.SetTitle("How To Use?");
                aDialog.SetMessage("The way you use the app is easy. " +
                    "You can choose the tags that you want and then " +
                    "click the purple buttons to go forward(generate new image + if you hold forward button you can go back to your latest image) " +
                    "or " +
                    "backwards(go back to the old image) " +
                    "after that when you hold down the image it will ask you to download the image into -internal/Downloads- folder");
                aDialog.SetNeutralButton("OK", delegate { aDialog.Dispose(); });
                aDialog.Show();
            }   
            if(item.ItemId == Resource.Id.menu_resetHistory)
            {
                aDialog.SetTitle("You Are About To Reset Your Image History");
                aDialog.SetMessage("If your image history filled doing this action is a great choice!" +
                    "\nSo are you sure about resetting your image history?");
                aDialog.SetPositiveButton("YES", delegate 
                {
                    imageStore.Reset();
                    previousImageButton.Visibility = ViewStates.Invisible;
                });
                aDialog.SetNegativeButton("NO", delegate { aDialog.Dispose(); });
                aDialog.Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        //private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        //{
        //    //TODO (reload stuff with new tag)
        //}

        private void CheckForPermissions()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted) 
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS, REQUEST_PERMISSION);
            }
        }
    }
}