using System;
using System.IO;
using System.Xml.Serialization;

namespace Lewd_Images
{
    /// <summary>
    /// Setting to be stored, contains <see cref="Get"/> and <see cref="Set"/>.
    /// Callback <see cref="OnChange"/> for updating stuff when setting is modified
    /// </summary>
    /// <typeparam name="T">Type of setting to be stored</typeparam>
    public class Setting<T>
    {
        /// <summary>
        /// Type of delgate used for changed calls
        /// </summary>
        public delegate void OnChangeCall();

        /// <summary>
        /// Settings constructor
        /// </summary>
        /// <param name="startingValue">Value to set the setting to</param>
        internal Setting(T startingValue)
        {
            t = startingValue;
        }

        /// <summary>
        /// Called when setting is changed
        /// </summary>
        public event OnChangeCall OnChange;

        // Property
        private T t;
        public void Set(T newT)
        {
            t = newT;
            OnChange?.Invoke();
        }
        public T Get()
        {
            return t;
        }

        // Implictly convert between setting and internal value
        public static implicit operator T(Setting<T> me)
        {
            return me.t;
        }
    }
    public class Settings
    {
        public static Settings Instance = new Settings();

        // Lewd Tags Enabled Setting
        public readonly Setting<bool> LewdTagsEnabled = new Setting<bool>(false);

        // Notification Enabled Setting
        public readonly Setting<bool> NotificationsEnabled = new Setting<bool>(true);

        // Animations Enabled Setting
        public readonly Setting<bool> AnimationsEnabled = new Setting<bool>(true);

        public static readonly string SettingsFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "settings.xml");

        public static void SaveToFile()
        {
            //ObjectSaver.WriteToXmlFile(SettingsFileLocation, Instance);
        }
        public static void LoadFromFile()
        {
            //if (!File.Exists(SettingsFileLocation))
            //    SaveToFile();
            //Instance = ObjectSaver.ReadFromXmlFile<Settings>(SettingsFileLocation);
        }
    }
}