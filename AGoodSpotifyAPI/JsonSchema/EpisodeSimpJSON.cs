namespace AGoodSpotifyAPI.JsonSchema
{
    internal class EpisodeSimpJSON
    {
        /// <summary>
        /// A URL to a 30 second preview (MP3 format) of the episode. null if not available.
        /// </summary>
        public string Audio_preview_url { get; set; }
        /// <summary>
        /// A description of the episode.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The episode length in milliseconds.
        /// </summary>
        public int Duration_ms { get; set; }
        /// <summary>
        /// Whether or not the episode has explicit content (true = yes it does; false = no it does not OR unknown).
        /// </summary>
        public bool? Explicit { get; set; }
        /// <summary>
        /// External URLs for this episode.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the episode.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The Spotify ID for the episode.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The cover art for the episode in various sizes, widest first.
        /// </summary>
        public ImageJSON[] Images { get; set; }
        /// <summary>
        /// True if the episode is hosted outside of Spotify’s CDN.
        /// </summary>
        public bool? Is_externally_hosted { get; set; }
        /// <summary>
        /// True if the episode is playable in the given market. Otherwise false.
        /// </summary>
        public bool? Is_playable { get; set; }
        /// <summary>
        /// Note: This field is deprecated and might be removed in the future. Please use the languages field instead. The language used in the episode, identified by a ISO 639 code.
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// A list of the languages used in the episode, identified by their ISO 639 code.
        /// </summary>
        public string[] Languages { get; set; }
        /// <summary>
        /// The name of the episode.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The date the episode was first released, for example "1981-12-15". Depending on the precision, it might be shown as "1981" or "1981-12".
        /// </summary>
        public string Release_date { get; set; }
        /// <summary>
        /// The precision with which release_date value is known: "year", "month", or "day".
        /// </summary>
        public string Release_date_precision { get; set; }
        /// <summary>
        /// The user’s most recent position in the episode. Set if the supplied access token is a user token and has the scope user-read-playback-position.
        /// </summary>
        public ResumepointJSON Resume_point { get; set; }
        /// <summary>
        /// The object type: "episode".
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Spotify URI for the episode.
        /// </summary>
        public string Uri { get; set; }
    }
}
