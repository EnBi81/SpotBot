namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ListOfFeaturedPlaylistsJSON
    {
        public string Message { get; set; }
        public PagingJSON<PlaylistSimp2JSON> Playlists { get; set; }
    }
}
