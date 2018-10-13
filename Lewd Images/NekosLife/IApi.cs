namespace Lewd_Images
{
    public interface IApi
    {
        string[] Tags { get; }
        string DefaultTag { get; }
        string GetImageUrl(string tag);
    }
}