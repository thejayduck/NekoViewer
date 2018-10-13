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
using Android.Gms.Ads;
using Plugin.Share;
using Plugin.CurrentActivity;
using System;
using System.Net;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
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

        public static LewdImageStore imageStore = new LewdImageStore();

        int ImagePanelOffscreenX => Resources.DisplayMetrics.WidthPixels;

        protected override void OnCreate(Bundle bundle) 
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            CheckForPermissions();

            //Finding Resources
            adView = FindViewById<AdView>(Resource.Id.adView);
            tagSpinner = FindViewById<Spinner>(Resource.Id.tagSpinner);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);
            previousImageButton = FindViewById<FloatingActionButton>(Resource.Id.previousImageButton);

            //SetAdView
            MobileAds.Initialize(this, "ca-app-pub-5157629142822799~8600251110");
            var adRequest = new AdRequest.Builder().Build();
            adView.LoadAd(adRequest);

            //Toolbar Configurations
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Lewds";

            tagSpinner.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, new ArrayList(NekosLife.Tags));

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
                aDialog.SetTitle("Image Options");
                aDialog.SetMessage("Are you sure about downloading this image?");
                aDialog.SetPositiveButton("Download Image", delegate 
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
                aDialog.SetNeutralButton("Set As Wallpaper", delegate 
                {
                    WallpaperManager.GetInstance(this).SetBitmap(imageStore.GetImage());
                    Toast.MakeText(this, "Wallpaper has been applied!", ToastLength.Short).Show();
                });
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
                    Fix();
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

            Settings.OnLewdTagsEnabledChange += delegate
            {
                tagSpinner.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, new ArrayList(NekosLife.Tags));
            };

            //Load image first time
            ReloadImagePanel();
        }

        /// <summary>
        /// Gets the next image and sets it to the image panel
        /// </summary>
        /// <param name="animation">if it should be animated(may not be animated if Settings.AnimationsEnabled is false)</param>
        public void GetNextImage(bool animate = true)
        {
            if (loading || downloading)
            {
                Toast.MakeText(this, "An Image Is Being Downloaded or Loading Please Be Patient", ToastLength.Short).Show();
                return;
            }

            Toast.MakeText(this, "Forward", ToastLength.Short).Show();
            loading = true;
            if (animate && Settings.AnimationsEnabled)
                imagePanel.Animate().TranslationX(-ImagePanelOffscreenX);

            Task.Run(() =>
            {
                try
                {
                    imageStore.Forward();
                    if (animate && Settings.AnimationsEnabled)
                        Fix();

                    RunOnUiThread(() =>
                    {
                        ReloadImagePanel();
                        CheckPreviousImageButton();

                        if (animate && Settings.AnimationsEnabled)
                        {
                            imagePanel.TranslationX = ImagePanelOffscreenX;
                            imagePanel.Animate().TranslationX(0);
                        }
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

        /// <summary>
        /// Gets the previous image and sets it to the image panel
        /// </summary>
        /// <param name="animation">if it should be animated(may not be animated if Settings.AnimationsEnabled is false)</param>
        public void GetPreviousImage(bool animate = true)
        {
            if (loading || downloading)
            {
                Toast.MakeText(this, "An Image Is Being Downloaded or Loading Please Be Patient", ToastLength.Short).Show();
                return;
            }

            Toast.MakeText(this, "Backwards", ToastLength.Short).Show();
            loading = true;
            if(animate && Settings.AnimationsEnabled)
                imagePanel.Animate().TranslationX(ImagePanelOffscreenX);

            Task.Run(() =>
            {
                imageStore.Back();
                if (animate && Settings.AnimationsEnabled)
                    Fix();

                RunOnUiThread(() =>
                {
                    ReloadImagePanel();
                    CheckPreviousImageButton();

                    if (animate && Settings.AnimationsEnabled)
                    {
                        imagePanel.TranslationX = -ImagePanelOffscreenX;
                        imagePanel.Animate().TranslationX(0);
                    }
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
            UpdateFavorite();
        }

        public void UpdateFavorite()
        {
            imagePanel.SetBackgroundColor(imageStore.IsCurrentFavorite ? Color.Gold : Color.Transparent);
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
                    Url = imageStore.GetLink()
                });
            }
            if(item.ItemId == Resource.Id.menu_favorite)
            {
                if (imagePanel.Drawable == null)
                    return false;

                if (imageStore.IsCurrentFavorite)
                    imageStore.RemoveCurrentFromFavorite();
                else
                    imageStore.AddCurrentToFavorite();
                UpdateFavorite();

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
                LinearLayout layout = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical
                };
                layout.SetPadding(30, 20, 30, 20);

                //Variables
                Switch lewdSwitch = new Switch(this)
                {
                    Text = "Enable NSFW Tags",
                    Checked = Settings.LewdTagsEnabled
                };
                lewdSwitch.CheckedChange += delegate
                {
                    Settings.LewdTagsEnabled = lewdSwitch.Checked;
                };

                Button resetButton = new Button(this)
                {
                    Text = "Reset Image History"
                };
                resetButton.Click += (o, e) =>
                {
                    Snackbar.Make(view, "Cleared Image History", Snackbar.LengthShort).Show();
                    imageStore.Reset();
                    CheckPreviousImageButton();
                };

                Button serverCheckerButton = new Button(this)
                {
                    Text = "Check NekosLife Server"
                };
                serverCheckerButton.Click += delegate
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://nekos.life/");
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        if (response == null || response.StatusCode != HttpStatusCode.OK)
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

                aDialog.SetTitle("Options");
                layout.AddView(lewdSwitch);
                layout.AddView(resetButton);
                layout.AddView(serverCheckerButton);
                aDialog.SetView(layout);
                aDialog.SetNegativeButton("Help?", delegate
                {
                    //Implement Help Here...
                });
                aDialog.Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        private void CheckForPermissions()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted) 
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS, REQUEST_PERMISSION);
            }
        }

        //wtf, fixes animations for some reason
        public void Fix()
        {
            var _ = WebRequest.Create(NekosLife.APIUri + "neko").GetResponse();
        }
    }
}