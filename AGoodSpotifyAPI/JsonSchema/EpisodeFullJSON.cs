using AGoodSpotifyAPI.InterFaces;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class EpisodeFullJSON : EpisodeSimpJSON, IPlaylistTrackable
    {
        /// <summary>
        /// The show on which the episode belongs.
        /// </summary>
        public ShowSimpJSON Show { get; set; }
    }
}
