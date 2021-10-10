namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ArtistFullJSON : ArtistSimpJSON
    {
        /// <summary>
        /// Information about the followers of the artist.
        /// </summary>
        public FollowersJSON Followers { get; set; }
        /// <summary>
        /// A list of the genres the artist is associated with. For example: "Prog Rock" , "Post-Grunge". (If not yet classified, the array is empty.)
        /// </summary>
        public string[] Genres { get; set; }
        /// <summary>
        /// Images of the artist in various sizes, widest first.
        /// </summary>
        public ImageJSON[] Images { get; set; }
        /// <summary>
        /// The popularity of the artist. The value will be between 0 and 100, with 100 being the most popular. The artist’s popularity is calculated from the popularity of all the artist’s tracks.
        /// </summary>
        public int? Popularity { get; set; }
    }
}
