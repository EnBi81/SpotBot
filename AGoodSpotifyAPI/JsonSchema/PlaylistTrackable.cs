using AGoodSpotifyAPI.InterFaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class PlaylistTrackable : IPlaylistTrackable
    {
        public string Href { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }

    }
}
