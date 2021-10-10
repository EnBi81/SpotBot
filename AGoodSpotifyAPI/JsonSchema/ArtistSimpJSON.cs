using AGoodSpotifyAPI.Web;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ArtistSimpJSON
    {
        /// <summary>
        /// Known external URLs for this artist.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the artist.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The Spotify ID for the artist.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The name of the artist.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The object type: "artist"
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Spotify URI for the artist.
        /// </summary>
        public string Uri { get; set; }            

        public async Task<ArtistFullJSON> GetFullArtist(string token)
        {
            if (this is ArtistFullJSON) return this as ArtistFullJSON;  

            return (await WebHelper.GetArtistHelper(token, Id).GetResultAsync()).Result;
        }
    }
}
