using Android.Widget;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
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
        /// Called when setting is changed
        /// </summary>
        public event OnChangeCall OnChange;

        // Property
        private T t;
        public void Set(T newT, bool callChange = true)
        {
            t = newT;

            if(callChange)
                OnChange?.Invoke();
        }
        public T Get()
        {
            return t;
        }

        /// <summary>
        /// Settings constructor
        /// </summary>
        /// <param name="startingValue">Value to set the setting to</param>
        internal Setting(T startingValue)
        {
            t = startingValue;
        }
        /// <summary>
        /// Required for serialization
        /// </summary>
        public Setting() { }

        // Implictly convert between setting and internal value
        public static implicit operator T(Setting<T> me)
        {
            return me.t;
        }
    }
    public class Settings
    {
        public static Settings Instance = new Settings();
        public Settings() { }

        /// <summary>
        /// If NSFW tags should be displayed
        /// </summary>
        public Setting<bool> LewdTagsEnabled { get; } = new Setting<bool>(false);

        /// <summary>
        /// If notification should be sent when downloading
        /// </summary>
        public Setting<bool> DownloadNotificationEnabled { get; } = new Setting<bool>(true);

        /// <summary>
        /// If animation should play
        /// </summary>
        public Setting<bool> AnimationsEnabled { get; } = new Setting<bool>(true);

        /// <summary>
        /// If we should generate a new image automatically when changing the tag
        /// </summary>
        public Setting<bool> GenerateNewImageOnTagChange { get; } = new Setting<bool>(true);
 
        public static readonly string SettingsFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "settings.xml");

        public static void SaveToFile()
        {
            ObjectSaver.WriteToFile(SettingsFileLocation, Instance);
        }
        public static void LoadFromFile()
        {
            if (!File.Exists(SettingsFileLocation))
                SaveToFile();
            else
            {
                try
                {
                    Instance = ObjectSaver.ReadFromFile<Settings>(SettingsFileLocation);
                } catch
                {
                    Toast.MakeText(MainActivity.Instance.ApplicationContext, "Couldn't load settings, clearing...", ToastLength.Short).Show();
                    SaveToFile();
                }
            }
        }
    }
}