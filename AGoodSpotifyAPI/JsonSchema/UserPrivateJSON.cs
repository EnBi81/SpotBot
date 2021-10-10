namespace AGoodSpotifyAPI.JsonSchema
{
    internal class UserPrivateJSON : UserPublicJSON
    {
        /// <summary>
        /// The country of the user, as set in the user’s account profile. An ISO 3166-1 alpha-2 country code. This field is only available when the current user has granted access to the user-read-private scope.
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// The user’s email address, as entered by the user when creating their account. Important! This email address is unverified; there is no proof that it actually belongs to the user. This field is only available when the current user has granted access to the user-read-email scope.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// The user’s Spotify subscription level: “premium”, “free”, etc. (The subscription level “open” can be considered the same as “free”.) This field is only available when the current user has granted access to the user-read-private scope.
        /// </summary>
        public string Product { get; set; }
    }
}
