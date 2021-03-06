﻿using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Android.Views;
using Plugin.Share;
using System;
using System.Net;
using System.Threading;
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

        public int GetStatusBarHeight()
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
        bool AutoSlideEnabled = false;

        //Buttons
        FloatingActionButton imageInfoButton;
        FloatingActionButton nextImageButton;
        public FloatingActionButton previousImageButton;

        //Timer
        int AutoSliderWaitTime = 5;

        //Misc
        ImageView imagePanel;
        Spinner tagSpinner;
        public string ImageName => System.IO.Path.GetFileNameWithoutExtension(imageStore.GetLink());
        AdRequest adRequest = new AdRequest.Builder().Build();

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
            adView.TranslationY = (PhoneHeight - adView.AdSize.GetHeightInPixels(this) - GetStatusBarHeight());

            //SetAdView
            MobileAds.Initialize(this, "ca-app-pub-3940256099942544~3347511713");
            adView.LoadAd(adRequest);

            CrossConnectivity.Current.ConnectivityChanged += (o, e) =>
            {
                RunOnUiThread(() =>
                {
                    //SetAdView when the connection changes
                    MobileAds.Initialize(this, "ca-app-pub-3940256099942544~3347511713");
                    adView.LoadAd(adRequest);
                });
            };

            //Finding Resources
            tagSpinner = FindViewById<Spinner>(Resource.Id.apiEndPoints);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            imageInfoButton = FindViewById<FloatingActionButton>(Resource.Id.imageInfoButton);
            nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);
            previousImageButton = FindViewById<FloatingActionButton>(Resource.Id.previousImageButton);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            imageInfoButton.Animate().TranslationY(PhoneHeight);

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
                    Toast.MakeText(this, $"Selected '{SelectedTag}'", ToastLength.Short).Show();
                    //GetNextImage();
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
                Android.App.AlertDialog.Builder aDialog;
                aDialog = new Android.App.AlertDialog.Builder(this);
                aDialog.SetTitle("Choose An Option");
                aDialog.SetNegativeButton("Auto Mode", delegate
                {
                    AutoSlideEnabled = !AutoSlideEnabled;
                    Toast.MakeText(this, $"Auto Mode Is '{AutoSlideEnabled.ToString()}'", ToastLength.Short).Show();
                    DoAutoSlide();
                });
                aDialog.SetPositiveButton("Last Image", delegate
                {
                    if (loading || downloading || AutoSlideEnabled)
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
                });
                aDialog.Show();
            };

            nextImageButton.Click += (o,e) =>
            {
                if (AutoSlideEnabled)
                    return;

                GetNextImage();
            };
            previousImageButton.Click += (o, e) =>
            {
                if (AutoSlideEnabled)
                    return;

                GetPreviousImage();
            };

            Settings.Instance.AnimationsEnabled.OnChange += delegate
            {
                //Do something to disable system animations
            };

            previousImageButton.Visibility = ViewStates.Invisible;
        }

        private void DoAutoSlide()
        {

            Task.Run(() =>
            {
                while (AutoSlideEnabled)
                {
                    if (!loading)
                    {
                        Thread.Sleep(AutoSliderWaitTime * 1000);
                        RunOnUiThread(() =>
                        {
                            GetNextImage();
                        });
                    }
                }
            });
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
        }

        public override void OnBackPressed()
        {
            Android.App.AlertDialog.Builder aDialog;
            aDialog = new Android.App.AlertDialog.Builder(this);
            aDialog.SetTitle("Meow! Don't Abandon Us!");
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
                    Max = 15
                };

                sliderWaitTime.Progress = AutoSliderWaitTime;

                sliderWaitTime.ProgressChanged += (o, e) =>
                {
                    if(sliderWaitTime.Progress <= 4)
                        sliderWaitTime.Progress = 5;

                    AutoSliderWaitTime = sliderWaitTime.Progress;
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
                            serverCheckerButton.SetTextColor(Color.Green);
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
            //:Buttons And Their Functionality:
            //Forward: Generates New Image
            //
            //(you can hold down on forward to go back to the last image)
            //Backwards Goes Back One Image
            //Dropdown: Choose The Tag You Want!
            //Image: When A New Image Is Generated Hold Your Finger Down On It To See More Options!
            //Share Image: Gives Sharing Options
            //Favorite Button: Saves Your Favorited Images In A List To Use Them In App
            //
            //:Options And Their Functionality:
            //Enable NSFW Tags: Enables (lewd) Tags
            //Enable Animations: Enables Animations (Saves Performance When Disabled)
            //Enable Notifications: Enables Notifications
            //Reset Image History: Resets The Generated Image List (Saves Performance)
            //Check NekosLife Server: To Check If The Host Is Online

            var aDialog = new Android.App.AlertDialog.Builder(this);
            aDialog.SetView(Resource.Layout.help_activity)
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