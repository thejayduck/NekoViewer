using System;
using System.Collections.Generic;
using System.IO;
using Plugin.Connectivity;

namespace Lewd_Images
{
    public class LewdImageStore : ImageStore
    {
        /// <summary>
        /// Api to use when getting image urls
        /// </summary>
        public IApi Api { get; set; }

        public LewdImageStore(IApi api, string tag = "select_default")
        {
            Api = api;
            if (tag == "select_default")
                Tag = api.DefaultTag;
            else
                Tag = tag;
        }

        /// <summary>
        /// List of favorite images
        /// </summary>
        public List<string> Favorites { get; private set; } = new List<string>();
        /// <summary>
        /// Adds current image to <see cref="Favorites"/>
        /// </summary>
        public void AddCurrentToFavorite()
        {
            Favorites.Add(GetLink());
            SaveFavorites();
        }
        /// <summary>
        /// Removes current image from <see cref="Favorites"/>
        /// </summary>
        public void RemoveCurrentFromFavorite()
        {
            Favorites.Remove(GetLink());
            SaveFavorites();
        }
        /// <summary>
        /// True when current image is in <see cref="Favorites"/> and false if not
        /// </summary>
        public bool IsCurrentFavorite => Favorites.Contains(GetLink());

        public static readonly string FavoritesFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "favorites.dat");

        public void SaveFavorites()
        {
            ObjectSaver.SerializeObject(FavoritesFileLocation, Favorites);
        }
        public void LoadFavorites()
        {
            if (!File.Exists(FavoritesFileLocation))
                SaveFavorites();
            Favorites = ObjectSaver.DeserializeObject<List<string>>(FavoritesFileLocation);
        }

        /// <summary>
        /// Tag to use when getting new image
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets an image url grabbed from <see cref="Api"/>
        /// </summary>
        /// <returns>Image url</returns>
        protected override string GetNew()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new System.Exception("No active internet connection");
            }

            return Api.GetImageUrl(Tag);
        }
    }
}