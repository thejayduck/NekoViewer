using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Lewd_Images
{
    public class ImageStore
    {
        private List<string> m_list = new List<string>();
        int index = 0;

        public void AppendNew()
        {
            string apiResponse = "";
            using (HttpWebResponse response = (HttpWebResponse)WebRequest.Create(GetLink()).GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                apiResponse = reader.ReadToEnd();
            }

            var json = new Org.Json.JSONObject(apiResponse);
            m_list.Add(json.GetString("url"));
        }

        #region Moving
        public void Forward(int count = 1)
        {
            index += count;
            while(index >= m_list.Count)
            {
                AppendNew();
            }
        }
        public void Back(int count = 1)
        {
            index -= count;
            if (index < 0) index = 0;
        }
        #endregion

        #region Gets
        public Bitmap GetImage()
        {
            return Helper.GetImageBitmapFromUrl(GetLink());
        }
        public string GetLink()
        {
            return m_list[index];
        }
        #endregion
    }
}