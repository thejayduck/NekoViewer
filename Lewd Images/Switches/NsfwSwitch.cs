using Android.Content;
using Android.Widget;

namespace Lewd_Images
{
    class NsfwSwitch : SettingSwitch
    {
        public NsfwSwitch(Context context, Setting<bool> setting) : base(context, setting)
        {
            CheckedChange += delegate
             {
                 if (Checked)
                     ShowNsfwPrompt();
             };
        }

        public void ShowNsfwPrompt()
        {
            if (Checked)
            {
                new Android.App.AlertDialog.Builder(Context)
                    .SetCancelable(false)
                    .SetTitle("You are about to enable NSFW tags!")
                    .SetPositiveButton("Enable It", delegate
                    {
                        Setting.Set(true);
                    })
                    .SetNegativeButton("Nevermind", delegate
                    {
                        Setting.Set(false);
                        Checked = false;
                    })
                    .Show();
            }
        }
    }
}