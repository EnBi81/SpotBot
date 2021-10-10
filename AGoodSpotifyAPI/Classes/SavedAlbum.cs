using AGoodSpotifyAPI.InterFaces;
using AGoodSpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Classes
{
    public class SavedAlbum : IAlbum, ISavedItem
    {
        public DateTime SavedAt { get; }

        public AlbumType AlbumType { get; }
        private IEnumerable<string> Artists { get; }
        public Markets[] AvailableMarkets { get; }
        public CopyRight[] CopyRights { get; }
        public string ExternalUrl { get; }
        public string[] Genres { get; }
        public string Href { get; }
        public string Id { get; }
        public Image[] Images { get; }
        public string Label { get; }
        public string Name { get; }
        public int Popularity { get; }
        public string ReleaseDate { get; }
        public ReleaseDatePrecision ReleaseDatePrecision { get; }
        public string Uri { get; }

        public async Task<List<Track>> GetTracksAsync(string token) => await Album.GetAlbumTracksAsync(albumId: Id, token: token);
        public async Task<Artist[]> GetArtists(string token) => await Artist.GetSeveralArtistsAsync(Artists, token);

#pragma warning disable
        public async Task UnSave(string token)
        {
            
        }
#pragma warning restore
    }
}
