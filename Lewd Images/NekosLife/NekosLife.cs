using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Lewd_Images
{
    abstract class NekosLife : IApi
    {
        class Dummy : NekosLife { }
        public static NekosLife Instance = new Dummy();

        public class DownException : Exception
        {
            public override string ToString()
            {
                return "NekosLife may be down, try again later";
            }
        }

        public static Uri APIUri = new Uri("https://nekos.life/api/v2/img/");

        private HttpWebResponse Request(string type)
        {
            WebRequest request = WebRequest.Create(APIUri + type);
                
            return (HttpWebResponse)request.GetResponse();
        }

        public string GetImageUrl(string tag)
        {
            string apiResponse = "";
            using (HttpWebResponse response = Request(tag))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                apiResponse = reader.ReadToEnd();
            }

            var json = new Org.Json.JSONObject(apiResponse);
            return json.GetString("url");
        }

        public string DefaultTag => Tags[0];
        public string[] Tags
        {
            get {
                IEnumerable<string> endpoints = SfwEndpoints;
                if (Settings.LewdTagsEnabled)
                    endpoints = endpoints.Concat(NsfwEndpoints);
                return endpoints
                            .Where(tag => !BlacklistTags.Contains(tag)) //Remove blacklisted tags
                            .ToArray(); //IEnumerable<string> -> string[]
            }
        }

        private readonly string[] SfwEndpoints =
        {
                "neko",
                "wallpaper",
                "ngif",
                "meow",
                "tickle",
                "feed",
                "gecg",
                "kemonomimi",
                "gasm",
                "poke",
                "slap",
                "avatar",
                "lizard",
                "waifu",
                "pat",
                "kiss",
                "cuddle",
                "fox_girl",
                "hug",
                "baka",
                "smug"
        };
                
        private readonly string[] NsfwEndpoints =
        {
            "Random_hentai_gif",
            "pussy",
            "nsfw_neko_gif",
            "lewd",
            "les",
            "kuni",
            "cum",
            "classic",
            "boobs",
            "bj",
            "anal",
            "yuri",
            "trap",
            "tits",
            "smallboobs",
            "pussy_jpg",
            "hentai",
            "cum_jpg",
            "solo",
            "futanari",
            "hololewd",
            "lewdk",
            "spank",
            "erokemo",
            "ero",
            "erofeet",
            "blowjob",
            "erok",
            "keta",
            "eroyuri",
            "eron",
            "holoero",
            "solog",
            "feetg",
            "nsfw_avatar",
            "feet",
            "holo",
            "femdom",
            "pwankg",
            "lewdkemo"
        };

        private string[] BlacklistTags {
            get => new string[]
            {

            };
        }
    }
}