using System;
using System.IO;
using System.Xml.Serialization;

namespace Lewd_Images
{
    public delegate void Func();
    public class Settings
    {
        public static Settings Instance = new Settings();

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

        public static readonly string SettingsFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "settings.xml");

        public static void SaveToFile()
        {
            ObjectSaver.WriteToXmlFile(SettingsFileLocation, Instance);
        }
        public static void LoadFromFile()
        {
            if (!File.Exists(SettingsFileLocation))
                SaveToFile();
            Instance = ObjectSaver.ReadFromXmlFile<Settings>(SettingsFileLocation);
        }
    }
}