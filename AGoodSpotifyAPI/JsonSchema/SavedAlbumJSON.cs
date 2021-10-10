using System;

namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// Contains Timestamp
    /// </summary>
    internal class SavedAlbumJSON
    {
        /// <summary>
        /// The date and time the album was saved.
        /// </summary>
        public DateTime Added_at { get; set; }
        /// <summary>
        /// Information about the album.
        /// </summary>
        public AlbumFullJSON Album { get; set; }
    }
}
