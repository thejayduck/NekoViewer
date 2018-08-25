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
        protected readonly List<string> list = new List<string>();
        protected int index = -1; // -1 is a 0 size list, 0 is a 1 size array, etc

        public abstract void AppendNew();

        #region Moving
        public void Forward(int count = 1)
        {
            index += count;
            while(index > list.Count-1)
            {
                AppendNew();
            }
        }
        public void Back(int count = 1)
        {
            index -= count;
            if (index < 0) index = 0;
        }
        public void GotoLast()
        {
            index = list.Count - 1;
        }

        public void Reset()
        {
            index = -1;
            list.Clear();
        }

        #endregion

        #region Gets
        Bitmap current = null;
        public Bitmap GetImage()
        {
            return current ?? (current = Helper.GetImageBitmapFromUrl(GetLink()));
        }
        public string GetLink()
        {
            return list[index];
        }
        #endregion

        public bool IsLast => index == list.Count - 1;
        public bool IsFirst => index == 0;
    }
}