using AGoodSpotifyAPI.InterFaces;
using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Classes
{
    public class Album : IAlbum
    {
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

        private Album(AlbumSimpJSON album, IEnumerable<string> artists)
        {
            var a = album;

            if(album is AlbumFullJSON full)
            {
                CopyRights = (from c in full.CopyRights select new CopyRight(c)).ToArray();
                Genres = full.Genres;
                Label = full.Label;
                Popularity = full.Popularity ?? 0;
            }

            AlbumType = Converting.StringToAlbumType(a.Album_type);
            Artists = artists.ToArray();
            AvailableMarkets = Converting.StringToMarkets(a.Available_markets);
            CopyRights = Array.Empty<CopyRight>();
            ExternalUrl = a.External_urls.Spotify;
            Href = a.Href;
            Id = a.Id;
            Genres = Array.Empty<string>();
            Images = (from i in a.Images select new Image(i)).ToArray();
            Label = string.Empty;
            Name = a.Name;
            Popularity = 0;
            ReleaseDate = a.Release_date;
            ReleaseDatePrecision = Converting.StringToRDP(a.Release_date_precision);
            Uri = a.Uri;
        }

        public async Task<List<Track>> GetTracksAsync(string token) => await GetAlbumTracksAsync(albumId: Id, token: token);

        public async Task<Artist[]> GetArtists(string token)
        {
            var res = (await WebHelper.GetSeveralArtists(token, Artists).GetResultAsync());
            if (res.IsError) throw new Exception(res.Error.Message);

            var list = new List<Artist>();
            foreach (var item in res.Result.Artists)
            {
                list.Add(new Artist(item));
            }


            return list.ToArray();
        }

        #region  Statikus
        public static async Task<Album> GetAlbumAsync(string albumId, string token)
        {
            var result = await WebHelper.GetAlbumHelper(token: token, albumId: albumId).GetResultAsync();
            if (result.IsError) throw new Exception(result.Error.Message);

            var album = InitializeAsync(result.Result);

            return album;

        }
        public static async Task<List<Track>> GetAlbumTracksAsync(string albumId, string token)
        {
            using var result = await WebHelper.GetAlbumsTrackHelper(token, albumId, null).GetResultAsync();
            var paging = result.Result;
            List<TrackSimpJSON> list = new List<TrackSimpJSON>();
            list.AddRange(paging.Items);
            while (!(paging.Next is null))
            {
                paging = (await new WebHelper<PagingJSON<TrackSimpJSON>>(token, paging.Next, Method.Get).GetResultAsync()).Result;
                list.AddRange(paging.Items);
            }

            var seged = Converting.Cutting(list);

            var tracks = new List<Track>();

            foreach (var item in seged)
            {
                var res = (await WebHelper.GetTrackMultiHelper(token: token, trackIds: from t in item select t.Id).GetResultAsync()).Result.Tracks;
                foreach (var i in res)
                {
                    tracks.Add(await Track.InitializeAsync(token, i));
                }
            }

            list = null;
            seged = null;
            paging = null;

            return tracks;

        }

        #endregion

        #region Overrides
        public override bool Equals(object obj) => obj is Album && (obj as Album).Id == Id;
        public override int GetHashCode() => HashCode.Combine(this);
        public override string ToString() => $"{Name} [{Id}]";
        #endregion

        #region Operators
        public static bool operator ==(Album left, Album right) => left.Equals(right);
        public static bool operator !=(Album left, Album right) => !(left == right);
        #endregion

        #region Internal Helper
        internal static Album InitializeAsync(AlbumSimpJSON album) => new Album(album, from a in album.Artists select a.Id);

        #endregion
    }
}
