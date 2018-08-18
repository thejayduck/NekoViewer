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
    }
}