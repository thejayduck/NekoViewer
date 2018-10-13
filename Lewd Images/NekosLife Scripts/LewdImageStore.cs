using System.Collections.Generic;
using System.IO;
using System.Net;
using Plugin.Connectivity;

namespace Lewd_Images
{
    public class LewdImageStore : ImageStore
    {
        public List<string> Favorites = new List<string>();
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

            string apiResponse = "";
            using (HttpWebResponse response = NekosLife.Request(Tag))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                apiResponse = reader.ReadToEnd();
            }

            var json = new Org.Json.JSONObject(apiResponse);
            list.Add(json.GetString("url"));
            return true;
        }

        public LewdImageStore(string tag = "select_default")
        {
            if (tag == "select_default")
                Tag = NekosLife.DefaultTag;
            else
                Tag = tag;
        }
    }
}