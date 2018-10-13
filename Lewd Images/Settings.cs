using System;

namespace Lewd_Images
{
    class Settings
    {

        public static event Action OnLewdTagsEnabledChange;
        private static bool m_lewdTagsEnabled = false;
        public static bool LewdTagsEnabled {
            get => m_lewdTagsEnabled;
            set {
                m_lewdTagsEnabled = value;
                OnLewdTagsEnabledChange();
            }
        }
    }
}