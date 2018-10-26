using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Lewd_Images
{
    public abstract class ImageStore
    {
        /// <summary>
        /// List of image urls
        /// </summary>
        protected readonly List<string> list = new List<string>();

        private int m_index = -1;
        /// <summary>
        /// Index of the list, -1 if no images exist
        /// </summary>
        public int Index {
            get => m_index;
            protected set {
                if(value >= list.Count)
                    throw new IndexOutOfRangeException();
                m_index = value;
            }
        }

        /// <summary>
        /// Gets a new url to the ImageStore internal array
        /// </summary>
        /// <returns>Image url</returns>
        protected abstract string GetNew();

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
        /// Moves the <see cref="Index"/> forward and calls <see cref="AppendNew"/> if no urls are available
        /// </summary>
        /// <param name="count"></param>
        public void Forward(int count = 1)
        {
            while((Index + count) > list.Count-1)
            {
                list.Add(GetNew());
            }
            Index += count;
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
            list.Clear();
            Index = -1;
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
            string url = GetLink();

            return new DownloadImageTask(null, null).Execute(url).GetResult();
        }

        /// <summary>
        /// Local path to where cached images should be stored
        /// </summary>
        public static string CacheStorageFolder => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "images");
        static string GetImageCacheLocation(string id) => System.IO.Path.Combine(CacheStorageFolder, id);
        static bool ExistsInCache(string id) => System.IO.File.Exists(System.IO.Path.Combine(CacheStorageFolder, GetUrlId(id)));
        static string GetUrlId(string url) => new FileInfo(url).Name;

        static Bitmap GetCached(string url)
        {
            string id = GetUrlId(url);
            return ExistsInCache(id) ? BitmapFactory.DecodeFile(GetImageCacheLocation(id)) : null;
        }

        public void SetImage(ImageView imageView, Action post)
        {
            string url = GetLink();
            Bitmap image = GetCached(url);
            if (image == null)
                new DownloadImageTask(imageView, post).Execute(url);
            else
            {
                imageView.SetImageBitmap(image);
                post?.Invoke();
            }
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

            MemoryStream CopyStreamToMemory(Stream inputStream)
            {
                MemoryStream ret = new MemoryStream();
                const int BUFFER_SIZE = 4096;
                byte[] buf = new byte[BUFFER_SIZE];

                int bytesread = 0;
                while ((bytesread = inputStream.Read(buf, 0, BUFFER_SIZE)) > 0)
                    ret.Write(buf, 0, bytesread);

                ret.Position = 0;
                return ret;
            }

            protected override Bitmap RunInBackground(params string[] urls)
            {
                using (Stream stream = new Java.Net.URL(urls[0]).OpenStream())
                {
                    string folder = GetImageCacheLocation("");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    
                    using (MemoryStream streamCopy = CopyStreamToMemory(stream))    //Since you cant seek in url stream, you need to copy it to memory
                    {
                        //TODO Make file copying async
                        using (FileStream file = System.IO.File.OpenWrite(GetImageCacheLocation(GetUrlId(urls[0]))))
                        {
                            streamCopy.CopyTo(file);
                        }
                        streamCopy.Position = 0;
                        return BitmapFactory.DecodeStream(streamCopy);
                    }
                }
            }
            protected override void OnPostExecute(Bitmap result)
            {
                bmImage?.SetImageBitmap(result);
                post?.Invoke();
            }
            protected override void OnPreExecute()
            {
                
            }
        }

        #endregion

        /// <summary>
        /// Returns link to the current image
        /// </summary>
        /// <returns>Link to current image</returns>
        public string GetLink()
        {
            if (Index < 0)
                throw new ImageStoreEmptyException();
            return list[Index];
        }

        public void DeleteCache()
        {
            Directory.Delete(CacheStorageFolder);
        }

        public ImageStore()
        {
            DeleteCache();
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

    public class ImageStoreEmptyException : Exception
    {
        public override string Message => "Tried to get image while ImageStore contained no images";
    }
}
