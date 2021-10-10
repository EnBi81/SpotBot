using AGoodSpotifyAPI.Classes;
using AGoodSpotifyAPI.Web;
using System;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class TrackSimpJSON
    {
        /// <summary>
        /// The artists who performed the track. Each artist object includes a link in href to more detailed information about the artist.
        /// </summary>
        public ArtistSimpJSON[] Artists { get; set; }
        /// <summary>
        /// A list of the countries in which the track can be played, identified by their ISO 3166-1 alpha-2 code.
        /// </summary>
        public string[] Available_Markets { get; set; }
        /// <summary>
        /// The disc number (usually 1 unless the album consists of more than one disc).
        /// </summary>
        public int? Disk_number { get; set; }
        /// <summary>
        /// The track length in milliseconds.
        /// </summary>
        public int? Duration_ms { get; set; }
        /// <summary>
        /// Whether or not the track has explicit lyrics ( true = yes it does; false = no it does not OR unknown).
        /// </summary>
        public bool? Explicit { get; set; }
        /// <summary>
        /// Known external URLs for this track.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the track.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The Spotify ID for the track.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Part of the response when Track Relinking is applied. If true , the track is playable in the given market. Otherwise false.
        /// </summary>
        public bool? Is_playable { get; set; }
        /// <summary>
        /// Part of the response when Track Relinking is applied, and the requested track has been replaced with different track. The track in the linked_from object contains information about the originally requested track.
        /// </summary>
        public TrackLinkJSON Linked_from { get; set; }
        /// <summary>
        /// Part of the response when Track Relinking is applied, the original track is not available in the given market, and Spotify did not have any tracks to relink it with. The track response will still contain metadata for the original track, and a restrictions object containing the reason why the track is not available: "restrictions" : {"reason" : "market"}
        /// </summary>
        public RestrictionJSON Restrictions { get; set; }
        /// <summary>
        /// The name of the track.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A link to a 30 second preview (MP3 format) of the track. Can be null
        /// </summary>
        public string Preview_url { get; set; }
        /// <summary>
        /// The number of the track. If an album has several discs, the track number is the number on the specified disc.
        /// </summary>
        public int? Track_number { get; set; }
        /// <summary>
        /// The object type: “track”.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Spotify URI for the track.
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// Whether or not the track is from a local file.
        /// </summary>
        public bool? Is_local { get; set; }


        public async System.Threading.Tasks.Task<TrackFullJSON> GetFullTrackJSON(string token)
        {
            if (this is TrackFullJSON) return this as TrackFullJSON;

            return (await WebHelper.GetTrackFullHelper(token, Id).GetResultAsync()).Result;
        }
    }
}