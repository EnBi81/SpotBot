namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ErrorJSON
    {
        /// <summary>
        /// The HTTP status code (also returned in the response header; see Response Status Codes for more information).
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// A short description of the cause of the error.
        /// </summary>
        public string Message { get; set; }
    }
}
