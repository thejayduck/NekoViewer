using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace Lewd_Images
{
    public abstract class ImageStore
    {
        /// <summary>
        /// List of image urls
        /// </summary>
        protected readonly List<string> list = new List<string>();

        /// <summary>
        /// Adds a custom link to internal list
        /// </summary>
        /// <param name="url">Custom link</param>
        public void AddLink(string url)
        {
            list.Add(url);
            Index++;
        }

        /// <summary>
        /// Index of the list, -1 if no images exist
        /// </summary>
        public int Index { get; protected set; } = -1;

        /// <summary>
        /// Gets a new url to the ImageStore internal array
        /// </summary>
        /// <returns>Image url</returns>
        protected abstract string GetNew();

        /// <summary>
        /// Moves the <see cref="Index"/> forward and calls <see cref="AppendNew"/> if no urls are available
        /// </summary>
        /// <param name="count"></param>
        public void Forward(int count = 1)
        {
            Index += count;
            while(Index > list.Count-1)
            {
                list.Add(GetNew());
            }
        }
        /// <summary>
        /// Moves the <see cref="Index"/> back
        /// </summary>
        /// <param name="count"></param>
        public void Back(int count = 1)
        {
            Index -= count;
            if (Index < 0) Index = 0;
        }
        /// <summary>
        /// Goes to the last image available
        /// </summary>
        public void GotoLast()
        {
            while(!IsLast)
            {
                Forward();
            }
        }

        /// <summary>
        /// Removes all urls stored and sets <see cref="Index"/> to -1
        /// </summary>
        public void Reset()
        {
            Index = -1;
            list.Clear();
        }

        // Sample Size: 10
        // Average: 0.821
        // Highest: 1.25
        // Lowest: 0.4
        #region Image Downloading

        /// <summary>
        /// Returns the cached image, If internal image cache is null, download the image and set the cache
        /// </summary>
        /// <returns>Current image</returns>
        public Bitmap GetImage()
        {
            return new DownloadImageTask(null, null).Execute(GetLink()).GetResult();
        }

        public void SetImage(ImageView imageView, Action post)
        {
            new DownloadImageTask(imageView, post).Execute(GetLink());
        }

        private class DownloadImageTask : AsyncTask<string, string, Bitmap> {
            readonly ImageView bmImage;
            readonly bool animate;
            readonly Action post;

            public DownloadImageTask(ImageView bmImage, Action post, bool animate = true)
            {
                this.bmImage = bmImage;
                this.animate = animate;
                this.post = post;
            }

            protected override Bitmap RunInBackground(params string[] urls)
            {
                using (Stream stream = new Java.Net.URL(urls[0]).OpenStream())
                {
                    return BitmapFactory.DecodeStream(stream);
                }
            }
            protected override void OnPostExecute(Bitmap result)
            {
                bmImage?.SetImageBitmap(result);
                post?.Invoke();
            }
        }

        #endregion

        /// <summary>
        /// Returns link to the current image
        /// </summary>
        /// <returns>Link to current image</returns>
        public string GetLink()
        {
            return list[Index];
        }

        /// <summary>
        /// If <see cref="Index"/> points to the last image in the list
        /// </summary>
        public bool IsLast => Index == list.Count - 1;
        /// <summary>
        /// If <see cref="Index"/> points to the first image in the list
        /// </summary>
        public bool IsFirst => Index <= 0;
    }
}