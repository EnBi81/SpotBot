namespace AGoodSpotifyAPI.JsonSchema
{
    internal class CategoryJSON
    {
        /// <summary>
        /// A link to the Web API endpoint returning full details of the category.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The category icon, in various sizes.
        /// </summary>
        public ImageJSON[] Images { get; set; }
        /// <summary>
        /// The Spotify category ID of the category.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The name of the category.
        /// </summary>
        public string Name { get; set; }
    }
}
