namespace AGoodSpotifyAPI.JsonSchema
{
    internal class AlbumFullJSON : AlbumSimpJSON
    {
        /// <summary>
        /// The copyright statements of the album.
        /// </summary>
        public CopyRightJSON[] CopyRights { get; set; }
        /// <summary>
        /// Known external IDs for the album.
        /// </summary>
        public ExternalIdsJSON External_ids { get; set; }
        /// <summary>
        /// A list of the genres used to classify the album. For example: "Prog Rock" , "Post-Grunge". (If not yet classified, the array is empty.)
        /// </summary>
        public string[] Genres { get; set; }
        /// <summary>
        /// The label for the album.
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// The popularity of the album. The value will be between 0 and 100, with 100 being the most popular. The popularity is calculated from the popularity of the album’s individual tracks.
        /// </summary>
        public int? Popularity { get; set; }
        /// <summary>
        /// The tracks of the album.
        /// </summary>
        public PagingJSON<TrackSimpJSON> Tracks { get; set; }
    }
}
