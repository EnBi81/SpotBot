using System;

namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// Contains Timestamp
    /// </summary>
    internal class SavedTrackJSON
    {
        /// <summary>
        /// The date and time the track was saved.
        /// </summary>
        public DateTime Added_at { get; set; }
        /// <summary>
        /// Information about the track.
        /// </summary>
        public TrackFullJSON Track { get; set; }
    }
}
