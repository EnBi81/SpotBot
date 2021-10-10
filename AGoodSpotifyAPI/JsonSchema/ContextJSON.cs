namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ContextJSON
    {
        /// <summary>
        /// The object type, e.g. “artist”, “playlist”, “album”.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the track.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// External URLs for this context.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// The Spotify URI for the context.
        /// </summary>
        public string Uri { get; set; }
    }
}
