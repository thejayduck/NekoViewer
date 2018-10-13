using System.Collections.Generic;
using System.IO;
using System.Net;
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
        public List<string> Favorites { get; } = new List<string>();
        /// <summary>
        /// Adds current image to <see cref="Favorites"/>
        /// </summary>
        public void AddCurrentToFavorite()
        {
            Favorites.Add(GetLink());
        }
        /// <summary>
        /// Removes current image from <see cref="Favorites"/>
        /// </summary>
        public void RemoveCurrentFromFavorite()
        {
            Favorites.Remove(GetLink());
        }
        /// <summary>
        /// True when current image is in <see cref="Favorites"/> and false if not
        /// </summary>
        public bool IsCurrentFavorite => Favorites.Contains(GetLink());

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