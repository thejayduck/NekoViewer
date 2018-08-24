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
    public abstract class ImageStore
    {
        protected List<string> list = new List<string>();
        int index = 0;

        public abstract void AppendNew();

        #region Moving
        public void Forward(int count = 1)
        {
            index += count;
            while(index >= list.Count)
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
            return list[index];
        }
        #endregion
    }
}