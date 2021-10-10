using AGoodSpotifyAPI.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface IAlbum
    {
        /// <summary>
        /// The type of the album: one of "album" , "single" , or "compilation".
        /// </summary>
        public AlbumType AlbumType { get; }
        /// <summary>
        /// The markets in which the album is available: ISO 3166-1 alpha-2 country codes. Note that an album is considered available in a market when at least 1 of its tracks is available in that market.
        /// </summary>
        public Markets[] AvailableMarkets { get; }
        /// <summary>
        /// The copyright statements of the album.
        /// </summary>
        public CopyRight[] CopyRights { get; }
        /// <summary>
        /// Known external URLs for this album.
        /// </summary>
        public string ExternalUrl { get; }
        /// <summary>
        /// A list of the genres used to classify the album. For example: "Prog Rock" , "Post-Grunge". (If not yet classified, the array is empty.)
        /// </summary>
        public string[] Genres { get; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the album.
        /// </summary>
        public string Href { get; }
        /// <summary>
        /// The Spotify ID for the album.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// The cover art for the album in various sizes, widest first.
        /// </summary>
        public Image[] Images { get; }
        /// <summary>
        /// The label for the album.
        /// </summary>
        public string Label { get; }
        /// <summary>
        /// The name of the album. In case of an album takedown, the value may be an empty string.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The popularity of the album. The value will be between 0 and 100, with 100 being the most popular. The popularity is calculated from the popularity of the album’s individual tracks.
        /// </summary>
        public int Popularity { get; }
        /// <summary>
        /// The date the album was first released, for example "1981-12-15". Depending on the precision, it might be shown as "1981" or "1981-12".
        /// </summary>
        public string ReleaseDate { get; }
        /// <summary>
        /// The precision with which ReleaseDate value is known
        /// </summary>
        public ReleaseDatePrecision ReleaseDatePrecision { get; }
        /// <summary>
        /// The Spotify URI for the album.
        /// </summary>
        public string Uri { get; }

        bool Equals(object obj);

        /// <summary>
        /// Gets the artists of this album.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Artist[]> GetArtists(string token);
        /// <summary>
        /// Gets the tracks of this album.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<List<Track>> GetTracksAsync(string token);
        string ToString();
    }
}