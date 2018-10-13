using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.IO;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Graphics;
using System.Collections.Generic;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Java.IO;
using Android.Views;
using System.Collections;
using Android.Runtime;
using Android.Gms.Ads;
using Plugin.Share;
using Plugin.CurrentActivity;
using System;
using System.Net;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        //#FD4281 (253, 66, 129, 100) - pink button color
        //#424040 (66, 64, 64, 100) - faded out pink color

        //bools
        bool loading = false;
        bool downloading = false;

        //Buttons
        FloatingActionButton nextImageButton;
        FloatingActionButton previousImageButton;

        //Ad Banner
        AdView adView;

        //Tags
        ArrayAdapter nekosTagAdapter;
        ArrayList nekosTags;
        readonly List<string> images = new List<string>();
        ImageView imagePanel;
        Spinner tagSpinner;
        string ImageName => System.IO.Path.GetFileNameWithoutExtension(imageStore.GetLink());
        private static readonly string[] PERMISSIONS = { Manifest.Permission.WriteExternalStorage, Manifest.Permission.Internet , Manifest.Permission.AccessNetworkState};
        private static readonly int REQUEST_PERMISSION = 3;

        private string SelectedTag {
            get {
                if (tagSpinner.SelectedItemPosition >= 0)
                    return NekosLife.Tags[tagSpinner.SelectedItemPosition];
                else
                    return NekosLife.DefaultTag;
            }
        }

        LewdImageStore imageStore = new LewdImageStore();

        int ImagePanelOffscreenX => Resources.DisplayMetrics.WidthPixels;

        protected override void OnCreate(Bundle bundle) 
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            CheckForPermissions();

            //Finding Resources
            adView = FindViewById<AdView>(Resource.Id.adView);
            nekosTags = new ArrayList();
            tagSpinner = FindViewById<Spinner>(Resource.Id.tagSpinner);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);
            previousImageButton = FindViewById<FloatingActionButton>(Resource.Id.previousImageButton);

            //SetAdView
            MobileAds.Initialize(this, "ca-app-pub-3940256099942544~3347511713");
            var adRequest = new AdRequest.Builder().Build();
            adView.LoadAd(adRequest);

            //Toolbar Configurations
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Lewds";

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
                    if (downloading)
                    {
                        Toast.MakeText(this, "An Image Is Being Downloaded Please Be Patient", ToastLength.Short).Show();
                        return;
                    }

                    downloading = true;
                    imagePanel.Animate().ScaleX(1.1f);
                    imagePanel.Animate().ScaleY(1.1f);
                    Task.Run(() =>
                    {
                        MemoryStream buffer = new MemoryStream();
                        imageStore.GetImage().Compress(Bitmap.CompressFormat.Png, 0, buffer);
                        buffer.Seek(0, SeekOrigin.Begin);
                        BufferedInputStream stream = new BufferedInputStream(buffer);
                        DownloadManager download = new DownloadManager(this, stream, buffer.Length);
                        download.Execute(ImageName + ".png");
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, $"Downloaded {ImageName}!", ToastLength.Short).Show();
                            imagePanel.Animate().ScaleX(1);
                            imagePanel.Animate().ScaleY(1);
                        });
                        downloading = false;
                    });
                });
                aDialog.SetNegativeButton("NO", delegate { aDialog.Dispose(); });
                aDialog.Show();
            };

            //Buttons Functions
            nextImageButton.LongClick += (o, e) =>
            {
                if (loading || downloading)
                {
                    Toast.MakeText(this, "An Image Is Being Downloaded or Loading Please Be Patient", ToastLength.Short).Show();
                    return;
                }
                Toast.MakeText(this, "Last image", ToastLength.Short).Show();
                loading = true;
                imagePanel.Animate().TranslationX(-ImagePanelOffscreenX);
                Task.Run(() =>
                {
                    imageStore.GotoLast();
                    imageStore.Fix();
                    RunOnUiThread(() =>
                    {
                        ReloadImagePanel();
                        CheckPreviousImageButton();
                        imagePanel.TranslationX = ImagePanelOffscreenX;
                        imagePanel.Animate().TranslationX(0);
                    });
                    loading = false;
                });
            };
            nextImageButton.Click += (o,e) =>
            {
                GetNextImage();
            };
            previousImageButton.Click += (o, e) =>
            {
                GetPreviousImage();
            };
        }

        public void GetNextImage()
        {
            if (loading || downloading)
            {
                Toast.MakeText(this, "An Image Is Being Downloaded or Loading Please Be Patient", ToastLength.Short).Show();
                return;
            }
            Toast.MakeText(this, "Forward", ToastLength.Short).Show();
            loading = true;
            imagePanel.Animate().TranslationX(-ImagePanelOffscreenX);

            Task.Run(() =>
            {
                try
                {
                    imageStore.Forward();
                    imageStore.Fix();
                    RunOnUiThread(() =>
                    {
                        ReloadImagePanel();
                        CheckPreviousImageButton();
                        imagePanel.TranslationX = ImagePanelOffscreenX;
                        imagePanel.Animate().TranslationX(0);
                    });
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, e.ToString(), ToastLength.Long).Show();
                    });
                }
                finally
                {
                    loading = false;
                }
            });
        }
        public void GetPreviousImage()
        {
            if (loading || downloading)
            {
                Toast.MakeText(this, "An Image Is Being Downloaded or Loading Please Be Patient", ToastLength.Short).Show();
                return;
            }
            Toast.MakeText(this, "Backwards", ToastLength.Short).Show();
            loading = true;
            imagePanel.Animate().TranslationX(ImagePanelOffscreenX);
            Task.Run(() =>
            {
                imageStore.Back();
                imageStore.Fix();
                RunOnUiThread(() =>
                {
                    ReloadImagePanel();
                    CheckPreviousImageButton();
                    imagePanel.TranslationX = -ImagePanelOffscreenX;
                    imagePanel.Animate().TranslationX(0);
                });
                loading = false;
            });
        }

        public void CheckPreviousImageButton()
        {
            previousImageButton.Visibility = imageStore.IsFirst ? ViewStates.Invisible : ViewStates.Visible;
        }

        public void ReloadImagePanel()
        {
            imagePanel.SetImageBitmap(imageStore.GetImage());
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

        public void RemoveFromFavorite(string imageUrl)
        {

        }

        public void SetAsFavorite(string imageUrl)
        {

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Android.App.AlertDialog.Builder aDialog;
            aDialog = new Android.App.AlertDialog.Builder(this);

            Activity activity = CrossCurrentActivity.Current.Activity;
            View view = FindViewById(Android.Resource.Id.Content);

            if (item.ItemId == Resource.Id.menu_share)
            {
                if (!CrossShare.IsSupported || imagePanel.Drawable == null)
                {
                    return false;   
                }

                CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage
                {
                    Title = "Lewd Image",
                    Text = "Checkout this Neko!",
                    Url = imageStore.GetLink().ToString()
                });
            }
            if(item.ItemId == Resource.Id.menu_favorite)
            {
                if (imagePanel.Drawable == null)
                    return false;

                Snackbar.Make(view, "Added As Favorite", Snackbar.LengthShort)
                    .SetAction("Undo", v => RemoveFromFavorite("IMPLEMENT THE URL HERE"));

            }
            if (item.ItemId == Resource.Id.menu_info) 
            {
                aDialog.SetTitle("App Info");
                aDialog.SetMessage("Made By:\nJay and Nobbele\nImages From\nNekos.life");
                aDialog.SetNeutralButton("OK", delegate { aDialog.Dispose(); });
                aDialog.Show();
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
            if(item.ItemId == Resource.Id.menu_options)
            {
                LinearLayout layout = new LinearLayout(this);
                layout.Orientation = Orientation.Vertical;
                layout.SetPadding(30, 20, 30, 0);
                //Variables
                Switch lewdSwitch = new Switch(this);
                lewdSwitch.Text = "Enable NSFW Tags";
                Button resetButton = new Button(this);
                resetButton.Text = "Reset Image History";
                Button serverCheckerButton = new Button(this);
                serverCheckerButton.Text = "Check NekosLife Server";

                serverCheckerButton.Click += delegate
                {
                    HttpWebRequest _request = (HttpWebRequest)WebRequest.Create("https://nekos.life/");
                    using (HttpWebResponse _response = (HttpWebResponse)_request.GetResponse())
                        if (_response == null || _response.StatusCode != HttpStatusCode.OK)
                        {
                            Toast.MakeText(this, "Server Does Not Respond", ToastLength.Short).Show();
                            serverCheckerButton.SetTextColor(Color.Red);
                            serverCheckerButton.Text = "Error";
                        }
                        else
                        {
                            Toast.MakeText(this, "Server Works Fine", ToastLength.Short).Show();
                            serverCheckerButton.SetTextColor(Color.DarkGreen);
                            serverCheckerButton.Text = "Success";
                        }
                };

                resetButton.Click += (o, e) =>
                {
                    Snackbar.Make(view, "Cleared Image History", Snackbar.LengthShort).Show();
                    imageStore.Reset();
                    CheckPreviousImageButton();
                };

                aDialog.SetTitle("Options");
                layout.AddView(lewdSwitch);
                layout.AddView(resetButton);
                layout.AddView(serverCheckerButton);
                aDialog.SetView(layout);
                aDialog.SetNegativeButton("Help?", delegate
                {
                    //Implement Help Here...
                });
                aDialog.SetPositiveButton("Apply!", delegate
                {
                    Toast.MakeText(this, "Applied!", ToastLength.Short).Show();
                    //Implement Lewd Tag Switcher
                    //Apply the option and save it
                    aDialog.Dispose();
                });
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