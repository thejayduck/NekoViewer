namespace Lewd_Images
{
    public delegate void Func();
    abstract class Settings
    {
        class Dummy : Settings { }
        public static Settings Instance = new Dummy();

        //Lewd Tags Enabled Setting
        public event Func OnLewdTagsEnabledChange;
        private bool m_lewdTagsEnabled = false;
        public bool LewdTagsEnabled {
            get => m_lewdTagsEnabled;
            set {
                m_lewdTagsEnabled = value;
                OnLewdTagsEnabledChange?.Invoke();
            }
        }

        //Animations Enabled Setting
        public event Func OnAnimationsEnabledChange;
        private bool m_animationsEnabled = true;
        public bool AnimationsEnabled {
            get => m_animationsEnabled;
            set {
                m_animationsEnabled = value;
                OnAnimationsEnabledChange?.Invoke();
            }
        }
    }
}