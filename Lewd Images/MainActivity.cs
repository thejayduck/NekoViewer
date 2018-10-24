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

namespace Lewd_Images
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppDarkTheme")]
    public partial class MainActivity : AppCompatActivity
    {
        //#FD4281 (253, 66, 129, 100) - pink button color
        //#424040 (66, 64, 64, 100) - faded out pink color

        public static MainActivity Instance;

        //bools
        bool loading = false;
        bool downloading = false;

        //Buttons
        FloatingActionButton imageInfoButton;
        FloatingActionButton nextImageButton;
        public FloatingActionButton previousImageButton;   

        //Tags
        ImageView imagePanel;
        Spinner tagSpinner;
        public string ImageName => System.IO.Path.GetFileNameWithoutExtension(imageStore.GetLink());
        private static readonly string[] PERMISSIONS = { Manifest.Permission.WriteExternalStorage, Manifest.Permission.Internet , Manifest.Permission.AccessNetworkState};
        private static readonly int REQUEST_PERMISSION = 1;

        private string SelectedTag {
            get {
                if (tagSpinner.SelectedItemPosition >= 0)
                    return NekosLife.Instance.Tags[tagSpinner.SelectedItemPosition];
                else
                    return NekosLife.Instance.DefaultTag;
            }
        }

        public static LewdImageStore imageStore = new LewdImageStore(NekosLife.Instance);

        public int ScreenPanelOffscreenX => Resources.DisplayMetrics.WidthPixels;
        public int ScreenPanelOffscreenY => Resources.DisplayMetrics.HeightPixels;

        protected override void OnCreate(Bundle bundle) 
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            CheckForPermissions();

            Instance = this;

            Window.AddFlags(WindowManagerFlags.Fullscreen); //to show


            Settings.LoadFromFile();
            //imageStore.LoadFavorites();

            //Finding Resources
            tagSpinner = FindViewById<Spinner>(Resource.Id.apiEndPoints);
            imagePanel = FindViewById<ImageView>(Resource.Id.imageView);
            imageInfoButton = FindViewById<FloatingActionButton>(Resource.Id.imageInfoButton);
            nextImageButton = FindViewById<FloatingActionButton>(Resource.Id.nextImageButton);
            previousImageButton = FindViewById<FloatingActionButton>(Resource.Id.previousImageButton);
            //AdView adView = FindViewById<AdView>(Resource.Id.adView);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            imageInfoButton.Animate().TranslationY(ScreenPanelOffscreenY);

            //SetAdView
            //MobileAds.Initialize(this, "ca-app-pub-5157629142822799~8600251110");
            //var adRequest = new AdRequest.Builder().Build();
            //adView.LoadAd(adRequest);

            //Toolbar Configurations
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = null;

            //Spinner
            tagSpinner.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, new ArrayList(NekosLife.Instance.Tags));
            tagSpinner.ItemSelected += (o, e) =>
            {
                imageStore.Tag = SelectedTag;
                Toast.MakeText(this, $"Selected {SelectedTag}", ToastLength.Short).Show();
                GetNextImage();
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
                            imageInfoButton.Animate().TranslationY(ScreenPanelOffscreenY);
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
                imagePanel.Animate().TranslationX(-ScreenPanelOffscreenX);
                Task.Run(() =>
                {
                    imageStore.GotoLast();
                    Fix();
                    RunOnUiThread(() =>
                    {
                        ReloadImagePanel(() =>
                        {
                            CheckPreviousImageButton();
                            imagePanel.TranslationX = ScreenPanelOffscreenX;
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

            Settings.Instance.LewdTagsEnabled.OnChange += delegate
            {
                tagSpinner.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, new ArrayList(NekosLife.Instance.Tags));
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

            DateTime start = DateTime.Now;

            loading = true;
            if (animate && Settings.Instance.AnimationsEnabled)
                imagePanel.Animate().TranslationX(-ScreenPanelOffscreenX);

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
                            previousImageButton.Visibility = ViewStates.Visible;

                            if (animate && Settings.Instance.AnimationsEnabled)
                            {
                                imagePanel.TranslationX = ScreenPanelOffscreenX;
                                imagePanel.Animate().TranslationX(0);
                            }

                            DateTime end = DateTime.Now;
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
                imagePanel.Animate().TranslationX(ScreenPanelOffscreenX);

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
                            imagePanel.TranslationX = -ScreenPanelOffscreenX;
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

        public void CreateNotification(string title, string text)
        {
            if (!Settings.Instance.NotificationsEnabled)
                return;

            string imageFile = System.IO.Path.Combine(DownloadManager.DownloadPath, $"{ImageName}.png");

            Bitmap bitmap = BitmapFactory.DecodeFile(imageFile);

            Android.Net.Uri uri = Android.Net.Uri.Parse(imageFile);
            Intent intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(uri, "image/png");

            intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);  

            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetContentIntent(pendingIntent)
                .SetStyle(new NotificationCompat.BigPictureStyle().BigPicture(bitmap))
                .SetSmallIcon(Resource.Mipmap.app_icon)
                .SetLargeIcon(bitmap);

            NotificationManager notificationManager = GetSystemService(NotificationService) as NotificationManager;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelId = $"{PackageName}.general";
                var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);

                notificationManager.CreateNotificationChannel(channel);

                builder.SetChannelId(channelId);
            }

            const int notificationId = 0;
            notificationManager.Notify(notificationId, builder.Build());
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
            //if(item.ItemId == Resource.Id.menu_favorite)
            //{
            //    if (imagePanel.Drawable == null)
            //        return false;

            //    if (imageStore.IsCurrentFavorite)
            //        imageStore.RemoveCurrentFromFavorite();
            //    else
            //        imageStore.AddCurrentToFavorite();
            //    UpdateFavorite();

            //}
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
            /*if(item.ItemId == Resource.Id.menu_favoritelist)
            {
                ScrollView scroll = new ScrollView(this);
                scroll.SetPadding(30, 20, 30, 20);

                LinearLayout linearLayout = new LinearLayout(this);

                scroll.AddView(linearLayout);
                foreach(string i in imageStore.Favorites)
                {
                    Button btn = new Button(this)
                    {
                        Text = i
                    };
                    linearLayout.AddView(btn);
                }

                aDialog.SetView(scroll)
                .SetTitle("Favorites")
                .SetNeutralButton("Close", delegate { aDialog.Dispose(); })
                .Show();

            }*/
            if(item.ItemId == Resource.Id.menu_options)
            {
                LinearLayout layout = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical
                };
                layout.SetPadding(30, 20, 30, 20);

                Switch lewdSwitch = new Switch(this)
                {
                    Text = "Enable NSFW Tags",
                    Checked = Settings.Instance.LewdTagsEnabled
                };
                lewdSwitch.CheckedChange += delegate
                {
                    if (lewdSwitch.Checked)
                        NsfwInfo(lewdSwitch);
                    else
                    {
                        lewdSwitch.Checked = false;
                        Settings.Instance.LewdTagsEnabled.Set(lewdSwitch.Checked);
                    }
                };

                SettingSwitch notificationSwitch = new SettingSwitch(this, "Enable Notifications", Settings.Instance.NotificationsEnabled);

                SettingSwitch animationSwitch = new SettingSwitch(this, "Enable Animations", Settings.Instance.AnimationsEnabled);

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
                .SetOnDismissListener(new OptionsDialogCallbackHandler())
                .SetOnCancelListener(new OptionsDialogCallbackHandler())
                .Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        private void NsfwInfo(Switch @switch)
        {
            Android.App.AlertDialog.Builder aDialog = new Android.App.AlertDialog.Builder(this);

            if (@switch.Checked)
            {
                aDialog.SetCancelable(false);
                aDialog.SetTitle("You are about to enable NSFW tags!");
                aDialog.SetPositiveButton("Enable It", delegate
                {
                    @switch.Checked = true;
                    Settings.Instance.LewdTagsEnabled.Set(@switch.Checked);
                });

                aDialog.SetNegativeButton("Nevermind", delegate
                {
                    @switch.Checked = false;
                    Settings.Instance.LewdTagsEnabled.Set(@switch.Checked);
                });
            }
            aDialog.Show();
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

        class OptionsDialogCallbackHandler : Java.Lang.Object, IDialogInterfaceOnDismissListener, IDialogInterfaceOnCancelListener
        {
            public void OnCancel(IDialogInterface dialog)
            {
                Settings.SaveToFile();
            }

            public void OnDismiss(IDialogInterface dialog)
            {
                Settings.SaveToFile();
            }
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