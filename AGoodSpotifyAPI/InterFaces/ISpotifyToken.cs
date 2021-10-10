using System;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface ISpotifyToken
    {
        public string AccessToken { get; }
        public DateTime ExpiresAt { get; }
        public bool Expired { get; }
        public string TokenType { get; }

        /// <summary>
        /// Automaticly refresh the token if it has already expired
        /// </summary>
        /// <returns></returns>
        public Task<string> GetAccessToken();
    }

    public interface IRefreshToken : ISpotifyToken
    {
        public AuthScopes[] Scopes { get; }
        public string RefreshToken { get; }
        public Task Refresh();
    }
}
