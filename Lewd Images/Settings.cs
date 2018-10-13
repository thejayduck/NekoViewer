using System;
using System.Collections.Generic;

namespace Lewd_Images
{
    public delegate void OnLewdTagsEnabledChanged();
    class Settings
    {

        public static event OnLewdTagsEnabledChanged OnLewdTagsEnabledChange;
        private static bool m_lewdTagsEnabled = false;
        public static bool LewdTagsEnabled {
            get => m_lewdTagsEnabled;
            set {
                m_lewdTagsEnabled = value;
                OnLewdTagsEnabledChange?.Invoke();
            }
        }
    }
}