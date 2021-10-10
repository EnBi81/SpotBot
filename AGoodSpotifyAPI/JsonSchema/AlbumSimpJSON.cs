using AGoodSpotifyAPI.Web;
using System.Net;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class AlbumSimpJSON
    {
        /// <summary>
        /// The field is present when getting an artist’s albums. Possible values are “album”, “single”, “compilation”, “appears_on”. Compare to album_type this field represents relationship between the artist and the album.
        /// </summary>
        public string Album_group { get;  set; }
        /// <summary>
        /// The type of the album: one of “album”, “single”, or “compilation”.
        /// </summary>
        public string Album_type { get; set; }
        /// <summary>
        /// The artists of the album. Each artist object includes a link in href to more detailed information about the artist.
        /// </summary>
        public ArtistSimpJSON[] Artists { get; set; }
        /// <summary>
        /// The markets in which the album is available: ISO 3166-1 alpha-2 country codes. Note that an album is considered available in a market when at least 1 of its tracks is available in that market.
        /// </summary>
        public string[] Available_markets { get; set; }
        /// <summary>
        /// Known external URLs for this album.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the album.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The Spotify ID for the album.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The cover art for the album in various sizes, widest first.
        /// </summary>
        public ImageJSON[] Images { get; set; }
        /// <summary>
        /// The name of the album. In case of an album takedown, the value may be an empty string.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The date the album was first released, for example 1981. Depending on the precision, it might be shown as 1981-12 or 1981-12-15.
        /// </summary>
        public string Release_date { get; set; }
        /// <summary>
        /// The precision with which release_date value is known: year , month , or day.
        /// </summary>
        public string Release_date_precision { get; set; }
        /// <summary>
        /// Part of the response when Track Relinking is applied, the original track is not available in the given market, and Spotify did not have any tracks to relink it with. The track response will still contain metadata for the original track, and a restrictions object containing the reason why the track is not available: "restrictions" : {"reason" : "market"}
        /// </summary>
        public RestrictionJSON Restriction { get; set; }
        /// <summary>
        /// The object type: “album”
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Spotify URI for the album.
        /// </summary>
        public string Uri { get; set; }



        public async System.Threading.Tasks.Task<AlbumFullJSON> GetFullAlbumAsync(string token)
        {
            if (this is AlbumFullJSON) return this as AlbumFullJSON;

            var res = await WebHelper.GetAlbumHelper(token, Id).GetResultAsync();

            return res.Result;
        }

    }
}
