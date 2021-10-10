using AGoodSpotifyAPI.Classes;
using AGoodSpotifyAPI.InterFaces;
using System;
using System.Linq;

namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// Part of the IPlaylistTrackable set
    /// </summary>
    internal class TrackFullJSON : TrackSimpJSON, IPlaylistTrackable
    {
        /// <summary>
        /// The album on which the track appears. The album object includes a link in href to full information about the album.
        /// </summary>
        public AlbumSimpJSON Album { get; set; }
        /// <summary>
        /// Known external IDs for the track.
        /// </summary>
        public ExternalIdsJSON External_ids { get; set; }
        /// <summary>
        /// The popularity of the track. The value will be between 0 and 100, with 100 being the most popular.
        /// The popularity of a track is a value between 0 and 100, with 100 being the most popular.The popularity is calculated by algorithm and is based, in the most part, on the total number of plays the track has had and how recent those plays are.
        /// Generally speaking, songs that are being played a lot now will have a higher popularity than songs that were played a lot in the past. Duplicate tracks (e.g.the same track from a single and an album) are rated independently.Artist and album popularity is derived mathematically from track popularity.Note that the popularity value may lag actual popularity by a few days: the value is not updated in real time.
        /// </summary>
        public int? Popularity { get; set; }

    }
}
