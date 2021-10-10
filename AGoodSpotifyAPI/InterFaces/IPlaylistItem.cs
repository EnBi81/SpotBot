using AGoodSpotifyAPI.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface IPlaylistItem
    {
        /// <summary>
        /// The date and time the track or episode was added.
        /// Note that some very old playlists may return null in this field.
        /// </summary>
        public DateTime? AddedAt { get; }
        /// <summary>
        /// The Spotify user who added the track or episode.
        /// Note that some very old playlists may return null in this field.
        /// </summary>
        public User AddedBy { get; }
        /// <summary>
        /// Whether this track or episode is a local file or not.
        /// </summary>
        public bool IsLocal { get; }
    }
}
