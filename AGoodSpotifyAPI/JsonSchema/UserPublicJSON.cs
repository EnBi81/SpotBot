namespace AGoodSpotifyAPI.JsonSchema
{
    internal class UserPublicJSON
    {
        /// <summary>
        /// The name displayed on the user’s profile. null if not available.
        /// </summary>
        public string Display_name { get; set; }
        /// <summary>
        /// Known public external URLs for this user.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// Information about the followers of this user.
        /// </summary>
        public FollowersJSON Followers { get; set; }
        /// <summary>
        /// A link to the Web API endpoint for this user.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The Spotify user ID for this user.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The user’s profile image.
        /// </summary>
        public ImageJSON[] Images { get; set; }
        /// <summary>
        /// The object type: “user”
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Spotify URI for this user.
        /// </summary>
        public string Uri { get; set; }
    }
}
