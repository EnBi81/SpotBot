namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// The identifier type
    /// </summary>
    internal class ExternalIdsJSON
    {
        /// <summary>
        /// International Standard Recording Code
        /// </summary>
        public string Iscr { get; set; }
        /// <summary>
        /// International Article Number
        /// </summary>
        public string Ean { get; set; }
        /// <summary>
        /// Universal Product Code
        /// </summary>
        public string Upc { get; set; }
    }
}