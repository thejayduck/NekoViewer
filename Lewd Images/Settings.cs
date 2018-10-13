using System;
using System.Collections.Generic;

namespace Lewd_Images
{
    public delegate void Func();
    class Settings
    {
        //Lewd Tags Enabled Setting
        public static event Func OnLewdTagsEnabledChange;
        private static bool m_lewdTagsEnabled = false;
        public static bool LewdTagsEnabled {
            get => m_lewdTagsEnabled;
            set {
                m_lewdTagsEnabled = value;
                OnLewdTagsEnabledChange?.Invoke();
            }
        }

        //Animations Enabled Setting
        public static event Func OnAnimationsEnabledChange;
        private static bool m_animationsEnabled = true;
        public static bool AnimationsEnabled {
            get => m_animationsEnabled;
            set {
                m_animationsEnabled = value;
                OnAnimationsEnabledChange?.Invoke();
            }
        }
    }
}