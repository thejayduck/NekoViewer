using System.Collections.Generic;
using System.IO;
using System.Net;
using Plugin.Connectivity;

namespace Lewd_Images
{
    public class LewdImageStore : ImageStore
    {
        public IApi Api { get; set; }

        public LewdImageStore(IApi api, string tag = "select_default")
        {
            Api = api;
            if (tag == "select_default")
                Tag = api.DefaultTag;
            else
                Tag = tag;
        }

        public List<string> Favorites { get; } = new List<string>();
        public void AddCurrentToFavorite()
        {
            Favorites.Add(GetLink());
        }
        public void RemoveCurrentFromFavorite()
        {
            Favorites.Remove(GetLink());
        }
        public bool IsCurrentFavorite => Favorites.Contains(GetLink());

        public string Tag { get; set; }
        public override bool AppendNew()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                return false;
            }

            list.Add(Api.GetImageUrl(Tag));
            return true;
        }
    }
}