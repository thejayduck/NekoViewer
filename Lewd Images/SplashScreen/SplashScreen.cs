using Android.Graphics;

using Android.App;
using Android.OS;
using Android.Views;
using Gr.Net.MaroulisLib;

namespace Lewd_Images.SplashScreen
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var config = new EasySplashScreen(this)
                .WithFullScreen()
                .WithTargetActivity(Java.Lang.Class.FromType(typeof(MainActivity)))
                .WithSplashTimeOut(2500)
                .WithBackgroundColor(Color.ParseColor("#36454f"))
                .WithHeaderText("Welcome To Neko Viewer")
                .WithFooterText("Made By \nJay and Nobbele");

            //Text Color
            config.HeaderTextView.SetTextColor(Color.White);
            config.FooterTextView.SetTextColor(Color.White);

            //Create View
            View view = config.Create();

            //Set Content View
            SetContentView(view);

            MainActivity.imageStore.Forward();
        }
    }
}