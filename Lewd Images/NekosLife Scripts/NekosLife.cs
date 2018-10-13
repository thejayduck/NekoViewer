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

        private static readonly string[] SfwEndpoints =
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
                
        private static readonly string[] NsfwEndpoints =
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

        private static string[] BlacklistTags {
            get => new string[]
            {

            };
        }
    }
}