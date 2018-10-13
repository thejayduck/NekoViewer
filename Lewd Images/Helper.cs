using System.Net;
using Android.Graphics;

namespace Lewd_Images
{
    static class Helper
    {
        /// <summary>
        /// Downloads image from url
        /// </summary>
        /// <param name="url">Url to download</param>
        /// <returns>Image downloaded in Bitmap form</returns>
        public static Bitmap GetImageBitmapFromUrl(string url)
        {
            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                    return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            }
            return null;
        }
    }
}