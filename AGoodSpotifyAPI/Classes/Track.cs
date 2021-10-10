using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;
using AGoodSpotifyAPI.InterFaces;
using AGoodSpotifyAPI.Exceptions.WebExceptions;

namespace AGoodSpotifyAPI.Classes
{
#pragma warning disable CS0661, CS0659
    /// <summary>
    /// A Spotify Track
    /// </summary>
    public class Track : IEquatable<Track>, ITrack
#pragma warning restore CS0661, CS0659
    {
        public string[] ArtistNames { get; }
        private string Album { get; }
        private IEnumerable<string> Artists { get; }
        public IEnumerable<ArtistSimple> ArtistSimple { get; }
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

        internal Track(TrackFullJSON track, string albumId, IEnumerable<string> artistIds, IEnumerable<ArtistSimpJSON> artistNames)
        {
            var t = track;
            Artists = artistIds;
            ArtistNames = (from a in artistNames select a.Name).ToArray();
            ArtistSimple = from a in artistNames select (Classes.ArtistSimple)a;
            Album = albumId;
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
        internal Track(TrackFullJSON track) : this(track, track.Album.Id, from ar in track.Artists select ar.Id, track.Artists) { }

        public async Task<Album> GetAlbumAsync(string token)
        {
            return await Classes.Album.GetAlbumAsync(Album, token: token);
        }
        public async Task<Artist[]> GetArtistsAsync(string token)
        {
            return await Artist.GetSeveralArtistsAsync(Artists, token);
        }

        public async Task SaveTrack(string token) => await WebHelper.SaveTracks(new string[] { Id }, token);

        /// <summary>
        /// Gets a track by Id
        /// </summary>
        /// <param name="token">The token (can be from any Authorization flow)</param>
        /// <param name="id">The track's Id</param>
        /// <returns></returns>
        public static async Task<Track> GetTrackAsync(string token, string id)
        {
            var result = await WebHelper.GetTrackFullHelper(token, id).GetResultAsync();
            if (result.IsError) throw SpotWebException.GetWebException(result.Error.Status, result.Error.Message);

            return await InitializeAsync(token, result.Result);
        }

        public override bool Equals(object obj)
        {
            if (obj is Track) return this == (obj as Track);
            return false;
        }
        public bool Equals([AllowNull] Track other)
        {
            return this == other;
        }

        #region Operators
        public static bool operator ==(Track left, ITrack right) => left.Id == right.Id;
        public static bool operator !=(Track left, ITrack right) => !(left == right);

        /// <summary>
        /// I really like this idea, so enjoy it.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static TrackList<Track> operator +(Track one, Track two)
        {
            var list = new List<Track>() { one, two };
            return new TrackList<Track>(list);
        }

        public bool Equals([AllowNull] ITrack other) => Id == other.Id;
        #endregion

        #region Internal Helper
        internal static async Task<Track> InitializeAsync(string token, TrackSimpJSON track)
        {
            var full = await track.GetFullTrackJSON(token);

            var t = new Track(full, full.Album.Id, from a in track.Artists select a.Id, track.Artists);

            return t;
        }

        #endregion
    }
}
