using AGoodSpotifyAPI.Classes;
using AGoodSpotifyAPI.InterFaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class SearchJSON
    {
        public PagingJSON<TrackFullJSON> Tracks { get; set; }
        public PagingJSON<AlbumSimpJSON> Albums { get; set; }

        public PagingJSON<ArtistFullJSON> Artists { get; set; }
        public PagingJSON<PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>> Playlists { get; set; }

        public async Task<Album[]> GetAlbums(string token)
        {
            if (Albums is null) return null;

            var cucc = await Converting.GetPagingItems(Albums, token);
            var albums = new List<Album>();

            cucc.ForEach(a =>
            {
                try
                {
                    albums.Add(Album.InitializeAsync(a));
                }
                catch { }
            });

            return albums.ToArray();
        }

        public async Task<Track[]> GetTracks(string token)
        {
            if (Tracks is null) return null;

            var cucc = await Converting.GetPagingItems(Tracks, token);
            var seged = new List<Track>();

            cucc.ForEach(async t => 
            {
                try
                {
                    seged.Add(await Track.InitializeAsync(token, t));
                }
                catch { }
            });

            return seged.ToArray();

        }
    }
}
