using AGoodSpotifyAPI.InterFaces;
using AGoodSpotifyAPI.JsonSchema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using AGoodSpotifyAPI.Exceptions.WebExceptions;
using System.Runtime.CompilerServices;

namespace AGoodSpotifyAPI.Classes
{
    public class PlaylistTrack : ITrack, IPlaylistItem
    {
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

        public DateTime? AddedAt { get; }
        public User AddedBy { get; }
        public bool IsLocal { get; }

        internal PlaylistTrack(PlaylistTrackJSON<TrackFullJSON> track)
        {
            AddedAt = track.Added_at;
            AddedBy = new User(track.Added_by);
            IsLocal = track.Is_local;

            if(track.Track is null)
                throw new NotFoundException("nincs ilyen");

            var t = track.Track;
            if (track.Track.Artists is null) Artists = new List<string>(0);
            else
                Artists = from ar in t.Artists select ar.Id;

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
        public async Task<Artist[]> GetArtistsAsync(string token) => await Artist.GetSeveralArtistsAsync(artistIds: Artists, token: token);

        public bool Equals([AllowNull] ITrack other) => Id == other.Id;
    }
}
