using System;

namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// Contains Timestamp
    /// </summary>
    internal class PlayHistoryJSON
    {
        /// <summary>
        /// The track the user listened to.
        /// </summary>
        public TrackSimpJSON Track { get; set; }
        /// <summary>
        /// The date and time the track was played. (Format: yyyy-MM-ddTHH:mm:ssZ)
        /// </summary>
        public DateTime Played_at { get; set; }
        /// <summary>
        /// The context the track was played from.
        /// </summary>
        public ContextJSON Context { get; set; }
    }
}
