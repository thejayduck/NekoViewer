using System.IO;
using System.Net;

namespace Lewd_Images
{
    class LewdImageStore : ImageStore
    {
        public string Tag { get; set; }
        public override void AppendNew()
        {
            string apiResponse = "";
            using (HttpWebResponse response = NekosLife.Request(Tag))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                apiResponse = reader.ReadToEnd();
            }

            var json = new Org.Json.JSONObject(apiResponse);
            list.Add(json.GetString("url"));
        }

        //wtf, fixes animations for some reason
        public void Fix()
        {
            var _ = WebRequest.Create(NekosLife.APIUri + "neko").GetResponse();
        }
        public LewdImageStore(string tag = "select_default")
        {
            if (tag == "select_default")
                Tag = NekosLife.DefaultTag;
            else
                Tag = tag;
        }
    }
}