using AGoodSpotifyAPI.InterFaces;
using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Classes
{
    public class SavedTrack : ITrack, ISavedItem
    {
        public DateTime SavedAt { get; }
        private string Album { get; }
        private IEnumerable<string> Artists { get; }
        public string[] ArtistNames { get; }
        public Markets[] AvailableMarkets { get; }
        public int DiscNumber { get; }
        public int Duration { get; }
        public bool? Explicit { get; }
        public string ExternalURL { get; }
        public string Href { get; }
        public string Id { get; }
        public bool IsPlayale { get; }
        public Restriction Restriction { get; }
        public string Name { get; }
        public int Popularity { get; }
        public string PreviewUrl { get; }
        public int TrackNumber { get; }
        public string Uri { get; }

        internal SavedTrack(SavedTrackJSON track)
        {
            var t = track.Track;
            Artists = from ar in track.Track.Artists select ar.Id;
            ArtistNames = (from a in track.Track.Artists select a.Name).ToArray();
            Album = t.Album.Id;
            AvailableMarkets = Converting.StringToMarkets(t.Available_Markets);
            DiscNumber = t.Disk_number ?? 1;
            Duration = t.Duration_ms ?? 0;
            Explicit = t.Explicit;
            Href = t.Href;
            Id = t.Id;
            IsPlayale = t.Is_playable ?? true;
            Name = t.Name;
            Popularity = t.Popularity ?? 0;
            PreviewUrl = t.Preview_url;
            Restriction = new Restriction(t.Restrictions);
            TrackNumber = t.Track_number ?? 1;
            Uri = t.Uri;
        }

        public async Task<Album> GetAlbumAsync(string token) => await Classes.Album.GetAlbumAsync(Album, token: token);
        public async Task<Artist[]> GetArtistsAsync(string token) => await Artist.GetSeveralArtistsAsync(Artists, token);

        public async Task UnSave(string token) => await UnSaveTrack(Id, token);



        #region Statikus
        public static async Task UnSaveTracks(IEnumerable<string> trackIds, string token) => await WebHelper.RemoveSavedTracks(trackIds, token);
        public static async Task UnSaveTrack(string trackId, string token) => await UnSaveTracks(new string[] { trackId }, token);

        public static async Task<bool[]> CheckSavedTracks(string token, IEnumerable<string> trackIds) => await WebHelper.CheckSavedTracks(trackIds, token);
        public static async Task<bool> CheckSavedTrack(string token, string trackId) => (await CheckSavedTracks(token, new string[] { trackId })).FirstOrDefault();

        public static async Task<TrackList<SavedTrack>> GetSavedTracks(string token)
        {
            var paging = (await WebHelper.GetSavedTracks(token).GetResultAsync()).Result;

            var list = await Converting.GetPagingItems(paging, token);

            return new TrackList<SavedTrack>(from t in list select new SavedTrack(t));
        }
        #endregion

        #region Overrides
        public bool Equals([AllowNull] ITrack other) => Id == other.Id;
        #endregion
    }
}
