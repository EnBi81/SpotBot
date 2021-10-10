namespace AGoodSpotifyAPI.JsonSchema
{
    internal class PlayerErrorJSON
    {
        /// <summary>
        /// The HTTP status code. Either 404 NOT FOUND or 403 FORBIDDEN. Also returned in the response header.
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// A short description of the cause of the error.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// One of the predefined player error reasons.
        /// </summary>
        public string Reason { get; set; }
    }
}
