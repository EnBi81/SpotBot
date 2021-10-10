using System;

namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// Contains Timestamp
    /// </summary>
    internal class SavedShowJSON
    {
        /// <summary>
        /// The date and time the show was saved.
        /// </summary>
        public DateTime Added_at { get; set; }
        /// <summary>
        /// Information about the show.
        /// </summary>
        public ShowFullJSON Show { get; set; }
    }
}
