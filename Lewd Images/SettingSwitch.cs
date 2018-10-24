using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace Lewd_Images
{
    class SettingSwitch : Switch
    {
        private readonly Setting<bool> m_setting;

        public SettingSwitch(Context context, string text, Setting<bool> setting) : base(context) {
            m_setting = setting;
            Text = text;
            Checked = setting;

            CheckedChange += delegate
            {
                setting.Set(Checked);
            };
        }
    }
}
