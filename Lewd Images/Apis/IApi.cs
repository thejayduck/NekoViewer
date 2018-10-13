namespace Lewd_Images
{
    public interface IApi
    {
        /// <summary>
        /// Available tags
        /// </summary>
        string[] Tags { get; }
        /// <summary>
        /// Default tag to use
        /// </summary>
        string DefaultTag { get; }
        /// <summary>
        /// Returns a image url using the tag
        /// </summary>
        /// <param name="tag">Tag to search with</param>
        /// <returns>Image url</returns>
        string GetImageUrl(string tag);
    }
}