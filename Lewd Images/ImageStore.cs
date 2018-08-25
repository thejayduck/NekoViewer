using System.Collections.Generic;
using Android.Graphics;

namespace Lewd_Images
{
    public abstract class ImageStore
    {
        protected readonly List<string> list = new List<string>();
        protected int index = -1; // -1 is a 0 size list, 0 is a 1 size array, etc

        Bitmap current = null;

        public abstract void AppendNew();

        #region Moving
        public void Forward(int count = 1)
        {
            current = null;
            index += count;
            while(index > list.Count-1)
            {
                AppendNew();
            }
        }
        public void Back(int count = 1)
        {
            current = null;
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