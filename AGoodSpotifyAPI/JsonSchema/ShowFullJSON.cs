namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ShowFullJSON : ShowSimpJSON
    {
        /// <summary>
        /// A list of the show’s episodes.
        /// </summary>
        public PagingJSON<EpisodeSimpJSON> Episodes { get; set; }
    }
}
