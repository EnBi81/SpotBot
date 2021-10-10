namespace AGoodSpotifyAPI.JsonSchema
{
    internal class RecommendationJSON
    {
        /// <summary>
        /// An array of recommendation seed objects.
        /// </summary>
        public SeedJSON[] Seeds { get; set; }
        /// <summary>
        /// An array of track object (simplified) ordered according to the parameters supplied.
        /// </summary>
        public TrackSimpJSON[] Tracks { get; set; }
    }
}
