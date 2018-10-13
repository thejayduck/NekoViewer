using System;
using System.Linq;
using System.Net;

namespace Lewd_Images
{
    class NekosLife
    {
        public class DownException : Exception
        {
            public override string ToString()
            {
                return "NekosLife may be down, try again later";
            }
        }

        public static Uri APIUri = new Uri("https://nekos.life/api/v2/img/");

        public static HttpWebResponse Request(string type)
        {
            WebRequest request = WebRequest.Create(APIUri + type);
            if (request.ContentLength < 0)
                throw new DownException();
            return (HttpWebResponse)request.GetResponse();
        }

        public static string DefaultTag => Tags[0];
        public static string[] Tags
        {
            get => new string[]
            {
                "neko",
                "yuri",
                "trap",
                "futanari",
                "hololewd",
                "lewdkemo",
                "solog",
                "feetg",
                "erokemo",
                "les",
                "wallpaper",
                "lewdk",
                "meow",
                "lewd",
                "gecg",
                "eroyuri",
                "eron",
                "cum_jpg",
                "solo",
                "kemonomimi",
                "gasm",
                "anal",
                "erofeet",
                "holo",
                "keta",
                "pussy",
                "tits",
                "holoero",
                "lizard",
                "pussy_jpg",
                "pwankg",
                "kuni",
                "waifu",
                "femdom",
                "feet",
                "erok",
                "fox_girl",
                "boobs",
                "smallboobs",
                "ero"
            }
            .Where(tag => !BlacklistTags.Contains(tag)) //Remove blacklisted tags
            .ToArray(); //IEnumerable<string> -> string[]
        }
        private static string[] BlacklistTags {
            get => new string[]
            {
                "classic",
                "smug",
                "nsfw_avatar",
                "avatar",
                "bj",
                "cum",
                "tickle",
                "feed",
                "baka",

            };
        }
    }
}