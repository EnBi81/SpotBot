using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class TracksObjectJSON
    {
        /// <summary>
        /// A collection containing a link (href) to the Web API endpoint where full details of the playlist’s tracks can be retrieved
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// Total number of tracks in the playlist.
        /// </summary>
        public int Total { get; set; }
    }
}
