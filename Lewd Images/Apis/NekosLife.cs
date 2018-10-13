using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Lewd_Images
{
    abstract class NekosLife : IApi
    {
        /// <summary>
        /// Prevents instanciation outside of the class
        /// </summary>
        class Dummy : NekosLife { }
        /// <summary>
        /// An instance of NekosLife class
        /// </summary>
        public static NekosLife Instance = new Dummy();

        /// <summary>
        /// Thrown when NekosLife api is down
        /// </summary>
        public class DownException : Exception
        {
            public override string ToString()
            {
                return "NekosLife may be down, try again later";
            }
        }

        /// <summary>
        /// Base url for api calls
        /// </summary>
        public static Uri APIUri = new Uri("https://nekos.life/api/v2/img/");

        /// <summary>
        /// Return a random image containing the tag specified
        /// </summary>
        /// <param name="tag">Tag to search for</param>
        /// <returns>Url to image</returns>
        public string GetImageUrl(string tag)
        {
            string apiResponse = "";
            using (WebResponse response = WebRequest.Create(APIUri + tag).GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                apiResponse = reader.ReadToEnd();
            }

            var json = new Org.Json.JSONObject(apiResponse);
            return json.GetString("url");
        }

        /// <summary>
        /// Default tag to use
        /// </summary>
        public string DefaultTag => Tags[0];

        /// <summary>
        /// Available tags
        /// </summary>
        public string[] Tags
        {
            get {
                IEnumerable<string> endpoints = SfwEndpoints;

                //Adds nsfw endpoints to the endpoints array if LewdTags are enabled
                if (Settings.Instance.LewdTagsEnabled)
                    endpoints = endpoints.Concat(NsfwEndpoints);

                return endpoints
                            .Where(tag => !BlacklistTags.Contains(tag)) //Remove blacklisted tags
                            .ToArray(); //IEnumerable<string> -> string[]
            }
        }

        /// <summary>
        /// Tags that are Safe For Work
        /// </summary>
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
            
        /// <summary>
        /// Tags that are Not Safe For Work
        /// </summary>
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


        /// <summary>
        /// List of tags that will be removed when getting the Tags property
        /// </summary>
        private string[] BlacklistTags {
            get => new string[]
            {

            };
        }
    }
}