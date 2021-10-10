using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Classes
{
    class ArtistSimple
    {
        public string Id { get; }
        public string Name { get; }
        public string Uri { get; }
        public string HRef { get; }

        private ArtistSimple(JsonSchema.ArtistSimpJSON simple)
        {
            Id = simple.Id;
            Name = simple.Name;
            Uri = simple.Uri;
            HRef = simple.Href;
            
        }

        public static explicit operator ArtistSimple(AGoodSpotifyAPI.JsonSchema.ArtistSimpJSON simple) => new ArtistSimple(simple);
    }
}
