using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Net;

namespace Lewd_Images
{
    public class DownloadManager : AsyncTask<string, string, string>
    {
        private ProgressDialog pDialog;
        private ImageView imgView;
        private Context context;
        private string imageName;

        public DownloadManager(Context context, ImageView imgView, string imageName)
        {
            this.context = context;
            this.imgView = imgView;
            this.imageName = imageName;
        }

        protected override void OnPreExecute()
        {
            pDialog = new ProgressDialog(context);
            pDialog.SetMessage("Downloading File Please Wait...");
            pDialog.Indeterminate = false;
            pDialog.Max = 100;
            pDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            pDialog.SetCancelable(true);
            pDialog.Show();
            base.OnPreExecute();
        }

        protected override void OnProgressUpdate(params string[] values)
        {
            base.OnProgressUpdate(values);
            pDialog.SetProgressNumberFormat(values[0]);
            pDialog.Progress = int.Parse(values[0]);
        }

        protected override void OnPostExecute(string result)
        {
            //string storagePath = Android.OS.Environment.ExternalStorageDirectory.Path;
            //string filePath = System.IO.Path.Combine(storagePath, "download.jpg");

            //imgView.SetImageDrawable(Drawable.CreateFromPath(filePath));
            pDialog.Dismiss();
        }

        static string downloadPath => Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath;

        protected override string RunInBackground(params string[] @params)
        {
            string filePath = System.IO.Path.Combine(downloadPath, imageName);

            int count;

            URL url = new URL(@params[0]);
            URLConnection connection = url.OpenConnection();
            connection.Connect();
            int LenghtOfFile = connection.ContentLength;
            InputStream input = new BufferedInputStream(url.OpenStream(), LenghtOfFile);
            if(!System.IO.File.Exists(filePath))
                System.IO.File.Create(filePath);
            OutputStream output = new FileOutputStream(filePath);

            byte[] data = new byte[1024];
            long total = 0;
            while ((count = input.Read(data)) != -1)
            {
                total += count;
                PublishProgress("" + (int)((total / 100 / LenghtOfFile)));
                output.Write(data, 0, count);
            }
            output.Flush();
            output.Close();
            input.Close();
            if(!System.IO.File.Exists(filePath))
            {
                throw new Exception();
            }
            return null;
        }
    }
}