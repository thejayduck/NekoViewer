using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace Lewd_Images
{
    class SettingSwitch : Switch
    {
        protected Setting<bool> Setting { get; }

        public SettingSwitch(Context context, Setting<bool> setting) : base(context)
        {
            Setting = setting;
            Checked = setting.Get();
            CheckedChange += delegate
            {
                setting.Set(Checked);
            };
        }
    }
}
