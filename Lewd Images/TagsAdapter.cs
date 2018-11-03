using Android.Widget;
using Android.Content;

namespace Lewd_Images
{
    public class TagsAdapter : ArrayAdapter
    {
        public TagsAdapter(Context context, int textViewResourceId, IApi api) : base(context, textViewResourceId)
        {
            this.api = api;
        }

        readonly IApi api;

        public override int Count => api.Tags.Length;

        public override Java.Lang.Object GetItem(int position)
        {
            return api.Tags[position];
        }
    }
}