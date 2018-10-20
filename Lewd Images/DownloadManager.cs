using System;
using Android.Content;
using Android.OS;
using Java.IO;
using Java.Lang;

namespace Lewd_Images
{
    public class DownloadManager : AsyncTask<string, string, string>
    {
        private readonly Context context;
        private readonly InputStream source;
        private readonly long size;

        public DownloadManager(Context context, InputStream source, long size)
        {
            this.context = context;
            this.source = source;
            this.size = size;
        }

        public static string DownloadPath => Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;

        protected override string RunInBackground(params string[] @params)
        {
            string filePath = System.IO.Path.Combine(DownloadPath, @params[0]);

            int count;
            if(!System.IO.File.Exists(filePath))
                System.IO.File.Create(filePath);
            OutputStream output = new FileOutputStream(filePath);

            byte[] data = new byte[1024];
            long total = 0;
            while ((count = source.Read(data)) != -1)
            {
                total += count;
                PublishProgress("" + (int)((total / 100 / size)));
                output.Write(data, 0, count);
            }
            output.Flush();
            output.Close();
            source.Close();
            if(!System.IO.File.Exists(filePath))
            {
                throw new System.Exception();
            }
            return null;
        }

        protected override void OnPostExecute(string result)
        {
            MainActivity.Instance.RunOnUiThread(() =>
            {
                MainActivity.Instance.CreateNotification("Download Completed!", MainActivity.Instance.ImageName);
            });
        }
    }
}