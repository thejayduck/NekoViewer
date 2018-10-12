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
                "smug",
                "futanari",
                "hololewd",
                "lewdkemo",
                "solog",
                "baka",
                "feetg",
                "cum",
                "erokemo",
                "les",
                "wallpaper",
                "lewdk",
                "meow",
                "tickle",
                "lewd",
                "feed",
                "gecg",
                "eroyuri",
                "eron",
                "cum_jpg",
                "bj",
                "solo",
                "kemonomimi",
                "nsfw_avatar",
                "gasm",
                "anal",
                "avatar",
                "erofeet",
                "holo",
                "keta",
                "pussy",
                "tits",
                "holoero",
                "lizard",
                "pussy_jpg",
                "pwankg",
                "classic",
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

            };
        }
    }
}