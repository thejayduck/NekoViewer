using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Lewd_Images
{
    class NekosLife
    {
        static Uri NekosLifeAPIUri = new Uri("https://nekos.life/api/v2/img/");

        public static HttpWebResponse Request(string type)
        {
            return (HttpWebResponse)((HttpWebRequest)WebRequest.Create(NekosLifeAPIUri + type)).GetResponse();
        }

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
            };
        }
    }
}