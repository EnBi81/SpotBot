using AGoodSpotifyAPI.InterFaces;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class PlaylistFullJSON<T> : PlaylistSimpJSON<T> 
    {
        public FollowersJSON Followers { get; set; }
    }
}
