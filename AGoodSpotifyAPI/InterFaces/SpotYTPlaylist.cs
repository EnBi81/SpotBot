using AGoodSpotifyAPI.Classes;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface ISpotYTPlaylist
    {
        public string Name { get; }
        public int TrackCount { get; }
        public User Owner { get; }
    }
}
