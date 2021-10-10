using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using Newtonsoft.Json;

namespace AGoodSpotifyAPI.Classes
{
    public class Artist
    {
        /// <summary>
        /// Known external URL for this artist.
        /// </summary>
        public string ExternalUrls { get; }
        /// <summary>
        /// The total number of followers.
        /// </summary>
        public int Followers { get; }
        /// <summary>
        /// A list of the genres the artist is associated with. For example: "Prog Rock" , "Post-Grunge". (If not yet classified, the array is empty.)
        /// </summary>
        public string[] Genres { get; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the artist.
        /// </summary>
        public string Href { get; }
        /// <summary>
        /// The Spotify ID for the artist.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Images of the artist in various sizes, widest first.
        /// </summary>
        public Image[] Images { get; }
        /// <summary>
        /// The name of the artist
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The popularity of the artist. The value will be between 0 and 100, with 100 being the most popular. The artist’s popularity is calculated from the popularity of all the artist’s tracks.
        /// </summary>
        public int Popularity { get; }
        /// <summary>
        /// The Spotify URI for the artist.
        /// </summary>
        public string Uri { get; }

        internal Artist(ArtistFullJSON artist)
        {
            var a = artist;
            ExternalUrls = a.External_urls.Spotify;
            Followers = a.Followers.Total ?? 0;
            Genres = a.Genres;
            
            Href = a.Href;
            Id = a.Id;
            Images = (from i in a.Images select new Image(i)).ToArray();
            Name = a.Name;
            Popularity = a.Popularity ?? 0;
            Uri = a.Uri;
        }


        /// <summary>
        /// Get Spotify catalog information about an artist’s albums.
        /// </summary>
        /// <param name="token">A valid access token from the Spotify Accounts service</param>
        /// <returns></returns>
        public async Task<Album[]> GetAlbumsAsync(string token) => await GetArtistAlbumsAsync(token, Id);

        public async Task<Artist[]> GetRelatedArtistsAsync(string token) => await Artist.GetRelatedArtistsAsync(token, Id);


        #region Statikus regio
        /// <summary>
        /// Get Spotify catalog information for a single artist identified by their unique Spotify ID.
        /// </summary>
        /// <param name="token">A valid access token from the Spotify Accounts service</param>
        /// <param name="artistId">The Spotify ID for the artist.</param>
        /// <returns></returns>
        public static async Task<Artist> GetArtistAsync(string token, string artistId)
        {
            var result = await WebHelper.GetArtistHelper(token: token, artistId: artistId).GetResultAsync();

            if (result.IsError) throw new Exception(result.Error.Message);

            return new Artist(result.Result);
        }

        /// <summary>
        /// Get Spotify catalog information about an artist’s albums.
        /// </summary>
        /// <param name="token">A valid access token from the Spotify Accounts service</param>
        /// <param name="artistId">The Spotify ID for the artist.</param>
        /// <param name="includeGroups">Alist of keywords that will be used to filter the response. If not supplied, all album types will be returned.</param>
        /// <param name="market">Supply this parameter to limit the response to one particular geographical market.</param>
        /// <param name="limit">The number of album objects to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first album to return. Default: 0 (i.e., the first album). Use with limit to get the next set of albums.</param>
        /// <returns></returns>
        public static async Task<Album[]> GetArtistAlbumsAsync(string token, string artistId, AlbumGroups[] includeGroups = null, Markets? market = null, int? limit = null, int? offset = null)
        {
            var result = await WebHelper.GetArtistsAlbumHelper(token, artistId, includeGroups, market, limit, offset).GetResultAsync();

            if (result.IsError) throw new Exception(result.Error.Message);

            var paging = result.Result;

            List<AlbumSimpJSON> list = new List<AlbumSimpJSON>();
            list.AddRange(paging.Items);
            while (!(paging.Next is null))
            {
                paging = (await new WebHelper<PagingJSON<AlbumSimpJSON>>(token, paging.Next, Method.Get).GetResultAsync()).Result;
                list.AddRange(paging.Items);
            }

            var seged = new List<Album>();

            var groups = Converting.Cutting(list, 50);

            foreach (var item in groups)
            {
                using var res = await WebHelper.GetAlbumsHelper(token, from a in item select a.Id).GetResultAsync();
                var albums = res.Result.Albums;

                foreach (var a in albums)
                {
                    seged.Add(Album.InitializeAsync(a));
                }
            }


            list = null;

            return seged.ToArray();

        }

        /// <summary>
        /// Get Spotify catalog information about an artist’s top tracks by country
        /// </summary>
        /// <param name="token">A valid access token from the Spotify Accounts service</param>
        /// <param name="artistId">The Spotify ID for the artist.</param>
        /// <param name="market"></param>
        /// <returns></returns>
        public static async Task<Track[]> GetArtistTopTracksAsync(string token, string artistId, Markets? market = null)
        {
            using var result = await WebHelper.GetArtistsTopTracksHelper(token, artistId, market).GetResultAsync();

            if (result.IsError) throw new Exception(result.Error.Message);

            var tracks = result.Result.Tracks;

            var list = new List<Track>();

            foreach (var item in tracks)
            {
                list.Add(await Track.InitializeAsync(token, item));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Get Spotify catalog information for several artists based on their Spotify IDs.
        /// </summary>
        /// <param name="artistIds">List of the Spotify IDs for the artists. Maximum: 50 IDs.</param>
        /// <param name="token">A valid access token from the Spotify Accounts service</param>
        /// <returns></returns>
        public static async Task<Artist[]> GetSeveralArtistsAsync(IEnumerable<string> artistIds, string token)
        {
            var res = await WebHelper.GetSeveralArtists(token, artistIds.ToArray()).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);
            var artists = res.Result.Artists;

            return (from a in artists select new Artist(a)).ToArray();

        }
        /// <summary>
        /// Get Spotify catalog information for several artists based on their Spotify IDs.
        /// </summary>
        /// <param name="token">A valid access token from the Spotify Accounts service</param>
        /// <param name="artistIds">List of the Spotify IDs for the artists. Maximum: 50 IDs.</param>
        /// <returns></returns>
        public static async Task<Artist[]> GetSeveralArtistsAsync(string token, params string[] artistIds) => await GetSeveralArtistsAsync(artistIds, token);

        /// <summary>
        /// Get Spotify catalog information about artists similar to a given artist. Similarity is based on analysis of the Spotify community’s listening history.
        /// </summary>
        /// <param name="token">A valid access token from the Spotify Accounts service</param>
        /// <param name="artistId">The Spotify ID for the artist.</param>
        /// <returns>An array of up to 20 artists</returns>
        public static async Task<Artist[]> GetRelatedArtistsAsync(string token, string artistId)
        {
            var res = await WebHelper.GetArtistsRelatedArtistsHelper(token: token, artistId: artistId).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);

            var artists = res.Result.Artists;

            return (from a in artists select new Artist(a)).ToArray();
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj) => obj is Artist && (obj as Artist).Id == Id;
        public override int GetHashCode() => HashCode.Combine(this);
        public override string ToString() => $"{Name} [{Id}]";
        #endregion

        #region Operators
        public static bool operator ==(Artist left, Artist right) => left.Equals(right);
        public static bool operator !=(Artist left, Artist right) => !(left == right);
        #endregion
    }
}
