using Android.Graphics;

using Android.App;
using Android.OS;
using Android.Views;
using Gr.Net.MaroulisLib;
using Android.Support.V7.App;

namespace Lewd_Images.SplashScreen
{
    [Activity(Label = "@string/app_name", MainLauncher = true ,Theme = "@style/AppTheme", Icon = "@mipmap/ic_launcher")]
    public class SplashScreen : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var config = new EasySplashScreen(this)
                .WithFullScreen()
                .WithTargetActivity(Java.Lang.Class.FromType(typeof(MainActivity)))
                .WithBackgroundColor(Color.ParseColor("#36454f"))
                .WithSplashTimeOut(
#if DEBUG
                    1000
#else
                    2500
#endif
                )
                .WithBeforeLogoText("Welcome To Neko Viewer")
                .WithFooterText("Made By: Jay and Nobbele");

            //Text Color
            config.BeforeLogoTextView.SetTextColor(Color.White);
            config.FooterTextView.SetTextColor(Color.White);

            //Create View
            View view = config.Create();

            //Set Content View
            SetContentView(view);

            MainActivity.imageStore.Forward();
        }
    }
}