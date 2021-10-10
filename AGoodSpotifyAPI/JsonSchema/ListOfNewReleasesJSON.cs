namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ListOfNewReleasesJSON
    {
        public string Message { get; set; }
        public PagingJSON<AlbumSimpJSON> Albums { get; set; }
    }
}
