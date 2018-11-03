using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.IO;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Java.IO;
using Android.Views;
using System.Collections;
using Plugin.Share;
using System;
using System.Net;
using System.Threading;
using Android.Content;
using Plugin.Connectivity;
using Android.Gms.Ads;

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppDarkTheme")]
    public partial class MainActivity : AppCompatActivity
    {
        //#FD4281 (253, 66, 129, 100) - pink button color
        //#424040 (66, 64, 64, 100) - faded out pink color

        public static MainActivity Instance;

        public int getStatusBarHeight()
        {
            int result = 0;
            int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                result = Resources.GetDimensionPixelSize(resourceId);
            }
            return result;
        }

        //Booleans
        bool loading = false;
        bool downloading = false;

        //Buttons
        FloatingActionButton imageInfoButton;
        FloatingActionButton nextImageButton;
        public FloatingActionButton previousImageButton;   

        //Misc
        ImageView imagePanel;
        Spinner tagSpinner;
        public string ImageName => System.IO.Path.GetFileNameWithoutExtension(imageStore.GetLink());

        //Permissions
        private static readonly string[] PERMISSIONS = { Manifest.Permission.WriteExternalStorage, Manifest.Permission.Internet , Manifest.Permission.AccessNetworkState};
        private static readonly int REQUEST_PERMISSION = 1;

        //Currently selected tag, NekosLife.Instance.DefaultTag if none is selected
        private string SelectedTag {
            get {
                if (tagSpinner.SelectedItemPosition >= 0)
                    return NekosLife.Instance.Tags[tagSpinner.SelectedItemPosition];
                else
                    return NekosLife.Instance.DefaultTag;
            }
        }

        public static LewdImageStore imageStore = new LewdImageStore(NekosLife.Instance);

        //Screen sizes
        public int PhoneWidth => Resources.DisplayMetrics.WidthPixels;
        public int PhoneHeight => Resources.DisplayMetrics.HeightPixels;

        protected override void OnCreate(Bundle bundle) 
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            CheckForPermissions();

            Instance = this;

            //Window.AddFlags(WindowManagerFlags.Fullscreen); //to show

            Settings.LoadFromFile();
            imageStore.LoadFavorites();

            AdView adView = new AdView(this)
            {
                AdSize = AdSize.SmartBanner,
                AdUnitId = "ca-app-pub-3940256099942544/6300978111"
            };

            CoordinatorLayout cLayout = FindViewById<CoordinatorLayout>(Resource.Id.coordinatorLayout);
            cLayout.AddView(adView);
            adView.TranslationY = (PhoneHeight - adView.AdSize.GetHeightInPixels(this) - getStatusBarHeight());

            //Finding Resources
            tagSpinner = FindViewById<Spinner>(Resource.Id.apiEndPoints);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            imageInfoButton = FindViewById<FloatingActionButton>(Resource.Id.imageInfoButton);
            nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);
            previousImageButton = FindViewById<FloatingActionButton>(Resource.Id.previousImageButton);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            imageInfoButton.Animate().TranslationY(PhoneHeight);

            //SetAdView
            MobileAds.Initialize(this, "ca-app-pub-3940256099942544~3347511713");
            var adRequest = new AdRequest.Builder().Build();
            adView.LoadAd(adRequest);

            //Toolbar Configurations
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = null;

            //Spinner
            tagSpinner.Adapter = new TagsAdapter(this, Android.Resource.Layout.SimpleListItem1, NekosLife.Instance);
            tagSpinner.ItemSelected += (o, e) =>
            {
                if (Settings.Instance.GenerateNewImageOnTagChange)
                {
                    imageStore.Tag = SelectedTag;
                    Toast.MakeText(this, $"Selected {SelectedTag}", ToastLength.Short).Show();
                    GetNextImage();
                }
            };

            bool infoButtonIsUp = false;
            //Request image download vvv
            imagePanel.LongClick += (o, e) =>
            {
                if (!infoButtonIsUp)
                {
                    infoButtonIsUp = true;
                    imageInfoButton.Animate().TranslationY(-100);
                    Task.Run(() =>
                    {
                        Thread.Sleep(3000);
                        RunOnUiThread(() =>
                        {
                            imageInfoButton.Animate().TranslationY(PhoneHeight);
                            infoButtonIsUp = false;
                        });
                    });
                }
            };

            imageInfoButton.Click += (o, e) =>
            {
                if (imagePanel.Drawable == null)
                {
                    Toast.MakeText(this, "No Images Were Found!", ToastLength.Short).Show();
                    return;
                }

                Android.App.AlertDialog.Builder aDialog;
                aDialog = new Android.App.AlertDialog.Builder(this);
                aDialog.SetTitle("Image Options");
                aDialog.SetPositiveButton("Download Image", delegate
                {
                    Bitmap image = imageStore.GetImage();
                    string path = Android.Provider.MediaStore.Images.Media.InsertImage(ContentResolver, image, ImageName, "A neko from NekoViewer app");
                    if(path == null)
                    {
                        Toast.MakeText(this, "Couldn't download image", ToastLength.Short).Show();
                        return;
                    }
                    Toast.MakeText(this, $"Downloaded {ImageName}!", ToastLength.Short).Show();
                    NotificationController.CreateDownloadNotification(this, "Download Completed!", ImageName, path, image);
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
                imagePanel.Animate().TranslationX(-PhoneWidth);
                Task.Run(() =>
                {
                    imageStore.GotoLast();
                    Fix();
                    RunOnUiThread(() =>
                    {
                        ReloadImagePanel(() =>
                        {
                            CheckPreviousImageButton();
                            imagePanel.TranslationX = PhoneWidth;
                            imagePanel.Animate().TranslationX(0);
                        });
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

            previousImageButton.Visibility = ViewStates.Invisible;
        }

        protected override void OnDestroy()
        {
            Settings.SaveToFile();
            imageStore.SaveFavorites();
            base.OnDestroy();
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

            //DateTime start = DateTime.Now;

            loading = true;
            if (animate && Settings.Instance.AnimationsEnabled)
                imagePanel.Animate().TranslationX(-PhoneWidth);

            Task.Run(() =>
            {
                try
                {
                    imageStore.Forward();
                    if (animate && Settings.Instance.AnimationsEnabled)
                        Fix();

                    RunOnUiThread(() =>
                    {
                        ReloadImagePanel(() =>
                        {
                            CheckPreviousImageButton();

                            if (animate && Settings.Instance.AnimationsEnabled)
                            {
                                imagePanel.TranslationX = PhoneWidth;
                                imagePanel.Animate().TranslationX(0);
                            }

                            //DateTime end = DateTime.Now;
                            //Toast.MakeText(this, string.Format("takes {0} seconds to get next image", (end - start).TotalSeconds), ToastLength.Short).Show();
                        });
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

            loading = true;
            if(animate && Settings.Instance.AnimationsEnabled)
                imagePanel.Animate().TranslationX(PhoneWidth);

            Task.Run(() =>
            {
                imageStore.Back();
                if (animate && Settings.Instance.AnimationsEnabled)
                Fix();

                RunOnUiThread(() =>
                {
                    ReloadImagePanel(() =>
                    {
                    
                        CheckPreviousImageButton();

                        if (animate && Settings.Instance.AnimationsEnabled)
                        {
                            imagePanel.TranslationX = -PhoneWidth;
                            imagePanel.Animate().TranslationX(0);
                        }
                    });
                });
                loading = false;
            });
        }

        public void CheckPreviousImageButton()
        {
            previousImageButton.Visibility = imageStore.IsFirst ? ViewStates.Invisible : ViewStates.Visible;
        }

        public void ReloadImagePanel(Action post)
        {
            imageStore.SetImage(imagePanel, post);
            //UpdateFavorite();
        }

        //public void UpdateFavorite()
        //{
        //    imagePanel.SetBackgroundColor(imageStore.IsCurrentFavorite ? Color.Goldenrod : Color.Transparent);
        //}

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
            Android.App.AlertDialog.Builder aDialog = new Android.App.AlertDialog.Builder(this);

            View view = FindViewById(Android.Resource.Id.Content);

            if (item.ItemId == Resource.Id.menu_share)
            {
                if (!CrossShare.IsSupported || imagePanel.Drawable == null)
                {
                    return false;   
                }
                CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage
                {
                    Title = "Neko Image",
                    Text = "Checkout this Neko!",
                    Url = imageStore.GetLink()
                });
            }
            if (item.ItemId == Resource.Id.menu_info) 
            {
                aDialog.SetTitle("App Info");

                LinearLayout layout = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical
                };
                layout.SetPadding(30, 20, 30, 20);

                TextView creditsText = new TextView(this)
                {
                    Text = $":Made By:" +
                    $"\n" +
                    $"TheJayDuck and Nobbele" +
                    $"\n" +
                    $":Images From:" +
                    $"\n" +
                    $"Nekos.Life",
                    Gravity = GravityFlags.CenterHorizontal
                };

                layout.AddView(creditsText);

                aDialog.SetView(layout);


                aDialog.SetNeutralButton("Close", delegate { aDialog.Dispose(); })
                .Show();
            }
            if(item.ItemId == Resource.Id.menu_options)
            {
                LinearLayout layout = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical
                };
                layout.SetPadding(30, 20, 30, 20);

                TextView text_1 = new TextView(this)
                {
                    Text = "Auto Slide Timer"
                };

                SeekBar sliderWaitTime = new SeekBar(this)
                {
                    Max = 10
                };

                NsfwSwitch lewdSwitch = new NsfwSwitch(this, Settings.Instance.LewdTagsEnabled)
                {
                    Text = "Enable NSFW Tags"
                };

                SettingSwitch notificationSwitch = new SettingSwitch(this, Settings.Instance.DownloadNotificationEnabled)
                {
                    Text = "Enable Notifications"
                };

                SettingSwitch animationSwitch = new SettingSwitch(this, Settings.Instance.AnimationsEnabled)
                {
                    Text = "Enable Animations"
                };

                Button resetButton = new Button(this)
                {
                    Text = "Reset Image History"
                };
                resetButton.Click += (o, e) =>
                {
                    Snackbar.Make(view, "Cleared Image History", Snackbar.LengthShort).Show();
                    string link = imageStore.GetLink();
                    imageStore.Reset();
                    imageStore.AddLink(link);
                    previousImageButton.Visibility = ViewStates.Invisible;
                };

                Button serverCheckerButton = new Button(this)
                {
                    Text = "Check NekosLife Server"
                };
                serverCheckerButton.Click += delegate
                {
                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        Toast.MakeText(this, "No active internet connection", ToastLength.Short).Show();
                        return;
                    }

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

                layout.AddView(lewdSwitch);
                layout.AddView(animationSwitch);
                layout.AddView(notificationSwitch);
                layout.AddView(text_1);
                layout.AddView(sliderWaitTime);
                layout.AddView(resetButton);
                layout.AddView(serverCheckerButton);
                aDialog.ItemSelected += delegate
                {
                    ;
                };
                aDialog.SetView(layout)
                .SetTitle("Options")
                .SetNegativeButton("Help?", delegate
                {
                    aDialog.Dispose();
                    HelpInfo();
                })
                .SetNeutralButton("Close", delegate { aDialog.Dispose(); Settings.SaveToFile(); })
                .SetCancelable(false)
                .Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        
        private void HelpInfo()
        {
            Android.App.AlertDialog.Builder aDialog = new Android.App.AlertDialog.Builder(this);

            LinearLayout layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };
            layout.SetPadding(30, 20, 30, 20);

            //Components
            TextView helpText = new TextView(this)
            {
                Text = $":Buttons And Their Functionality:" +
                $"\n" +
                $"Forward: Generates New Image \n " +
                $"(you can hold down on forward to go back to the last image)" +
                $"\n" +
                $"Backwards Goes Back One Image" +
                $"\n" +
                $"Dropdown: Choose The Tag You Want!" +
                $"\n" +
                $"Image: When A New Image Is Generated Hold Your Finger Down On It To See More Options!" +
                $"\n" +
                $"Share Image: Gives Sharing Options" +
                $"\n" +
                $"Favorite Button: Saves Your Favorited Images In A List To Use Them In App" +
                $"\n" +
                $"\n" +
                $":Options And Their Functionality:" +
                $"\n" +
                $"Enable NSFW Tags: Enables (lewd) Tags" +
                $"\n" +
                $"Enable Animations: Enables Animations (Saves Performance When Disabled)" +
                $"\n" +
                $"Enable Notifications: Enables Notifications" +
                $"\n" +
                $"Reset Image History: Resets The Generated Image List (Saves Performance)" +
                $"\n" +
                $"Check NekosLife Server: To Check If The Host Is Online",
                Gravity = GravityFlags.CenterHorizontal
            };

            //Add Views
            layout.AddView(helpText);

            aDialog.SetView(layout)
            .SetTitle("Help")
            .SetNegativeButton("Close", delegate
            {
                aDialog.Dispose();
            })
            .Show();
        }


        private void CheckForPermissions()
        {
            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted) 
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