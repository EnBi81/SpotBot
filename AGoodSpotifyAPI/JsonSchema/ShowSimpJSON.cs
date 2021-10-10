namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ShowSimpJSON
    {
        /// <summary>
        /// A list of the countries in which the show can be played, identified by their ISO 3166-1 alpha-2 code.
        /// </summary>
        public string[] Available_markets { get; set; }
        /// <summary>
        /// The copyright statements of the show.
        /// </summary>
        public CopyRightJSON[] Copyrights { get; set; }
        /// <summary>
        /// A description of the show.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Whether or not the show has explicit content (true = yes it does; false = no it does not OR unknown).
        /// </summary>
        public bool? Explicit { get; set; }
        /// <summary>
        /// Known external URLs for this show.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the show.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The Spotify ID for the show.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The cover art for the show in various sizes, widest first.
        /// </summary>
        public ImageJSON[] Images { get; set; }
        /// <summary>
        /// True if all of the show’s episodes are hosted outside of Spotify’s CDN. This field might be null in some cases.
        /// </summary>
        public bool? Is_externally_hosted { get; set; }
        /// <summary>
        /// A list of the languages used in the show, identified by their ISO 639 code.
        /// </summary>
        public string[] Languages { get; set; }
        /// <summary>
        /// The media type of the show.
        /// </summary>
        public string Media_type { get; set; }
        /// <summary>
        /// The name of the show.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The publisher of the show.
        /// </summary>
        public string Publisher { get; set; }
        /// <summary>
        /// The object type: “show”.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Spotify URI for the show.
        /// </summary>
        public string Uri { get; set; }
    }
}