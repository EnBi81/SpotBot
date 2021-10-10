using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.InterFaces
{
    internal interface IPlaylistTrackable
    {
        public string Href { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
    }
}
