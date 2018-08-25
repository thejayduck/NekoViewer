using System;
using Android.Content;
using Android.OS;
using Java.IO;

namespace Lewd_Images
{
    public class DownloadManager : AsyncTask<string, string, string>
    {
        //private ProgressDialog pDialog;
        private readonly Context context;
        private readonly InputStream source;
        private readonly long size;

        public DownloadManager(Context context, InputStream source, long size)
        {
            this.context = context;
            this.source = source;
            this.size = size;
        }

        protected override void OnPreExecute()
        {
            //pDialog = new ProgressDialog(context);
            //pDialog.SetMessage("Downloading File Please Wait...");
            //pDialog.Indeterminate = false;
            //pDialog.Max = 100;
            //pDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            //pDialog.SetCancelable(true);
            //pDialog.Show();
            base.OnPreExecute();
        }

        protected override void OnProgressUpdate(params string[] values)
        {
            base.OnProgressUpdate(values);
            //pDialog.SetProgressNumberFormat(values[0]);
            //pDialog.Progress = int.Parse(values[0]);
        }

        protected override void OnPostExecute(string result)
        {
            //string storagePath = Android.OS.Environment.ExternalStorageDirectory.Path;
            //string filePath = System.IO.Path.Combine(storagePath, "download.jpg");

            //imgView.SetImageDrawable(Drawable.CreateFromPath(filePath));
            //pDialog.Dismiss();
        }

        static string DownloadPath => Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;

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
                throw new Exception();
            }
            return null;
        }
    }
}