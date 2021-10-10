using System;
using System.Threading.Tasks;
using AGoodSpotifyAPI.Classes;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface ITrack : IEquatable<ITrack>
    {
        public string[] ArtistNames { get; }
        /// <summary>
        /// A list of the countries in which the track can be played, identified by their ISO 3166-1 alpha-2 code.
        /// </summary>
        public Markets[] AvailableMarkets { get; }
        /// <summary>
        /// The disc number (usually 1 unless the album consists of more than one disc).
        /// </summary>
        public int DiscNumber { get; }
        /// <summary>
        /// The track length in milliseconds.
        /// </summary>
        public int Duration { get; }
        /// <summary>
        /// Whether or not the track has explicit lyrics ( true = yes it does; false = no it does not OR unknown).
        /// </summary>
        public bool? Explicit { get; }
        /// <summary>
        /// Known external URLs for this track.
        /// </summary>
        public string ExternalURL { get; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the track.
        /// </summary>
        public string Href { get; }
        /// <summary>
        /// The Spotify ID for the track.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Part of the response when Track Relinking is applied. If true , the track is playable in the given market. Otherwise false.
        /// </summary>
        public bool IsPlayale { get; }
        /// <summary>
        /// Part of the response when Track Relinking is applied, the original track is not available in the given market, and Spotify did not have any tracks to relink it with. The track response will still contain metadata for the original track, and a restrictions object containing the reason why the track is not available
        /// </summary>
        public Restriction Restriction { get; }
        /// <summary>
        /// The name of the track.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// <para>The popularity of the track. The value will be between 0 and 100, with 100 being the most popular.</para>
        /// <para>The popularity of a track is a value between 0 and 100, with 100 being the most popular.The popularity is calculated by algorithm and is based, in the most part, on the total number of plays the track has had and how recent those plays are.</para>
        /// Generally speaking, songs that are being played a lot now will have a higher popularity than songs that were played a lot in the past. Duplicate tracks (e.g.the same track from a single and an album) are rated independently. Artist and album popularity is derived mathematically from track popularity. Note that the popularity value may lag actual popularity by a few days: the value is not updated in real time.
        /// </summary>
        public int Popularity { get; }
        /// <summary>
        /// A link to a 30 second preview (MP3 format) of the track. Can be null
        /// </summary>
        public string PreviewUrl { get; }
        /// <summary>
        /// The number of the track. If an album has several discs, the track number is the number on the specified disc.
        /// </summary>
        public int TrackNumber { get; }
        /// <summary>
        /// The Spotify URI for the track.
        /// </summary>
        public string Uri { get; }

        public Task<Album> GetAlbumAsync(string token);
        public Task<Artist[]> GetArtistsAsync(string token);
    }
}