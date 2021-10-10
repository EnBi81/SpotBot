using AGoodSpotifyAPI.Auth.JSON;
using AGoodSpotifyAPI.InterFaces;
using System;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Auth
{
    /// <summary>
    /// Refreshable Token got from the PKCE Auth flow.
    /// </summary>
    public class PKCEToken : IRefreshToken
    {
        public string ClientID { get; }
        public string AccessToken { get; private set; }
        public string TokenType { get; }
        public DateTime ExpiresAt { get; private set; }
        public bool Expired => ExpiresAt <= DateTime.Now;
        public AuthScopes[] Scopes { get; }
        public string RefreshToken { get; private set; }

        internal PKCEToken(PKCETokenJSON token, string clientId)
        {
            var t = token;

            AccessToken = t.Access_token;
            TokenType = t.Token_type;
            ExpiresAt = DateTime.Now.AddSeconds(t.Expires_in);
            Scopes = Converting.GetAuthScopes(t.Scope);
            RefreshToken = t.Refresh_token;

            ClientID = clientId;
        }
        public PKCEToken(string clientId, string accessToken, string refreshToken, DateTime expiresAt)
        {
            ClientID = clientId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            TokenType = "Bearer";

        }

        public async Task Refresh()
        {
            var t = await Authorization.PKCE.RefreshTokenAsync(ClientID, RefreshToken);
            (AccessToken, ExpiresAt, RefreshToken) = (t.AccessToken, t.ExpiresAt, t.RefreshToken);
        }

        
        public async Task<string> GetAccessToken()
        {
            if (!Expired) return AccessToken;

            await Refresh();
            return AccessToken;
        }
    }

    /// <summary>
    /// Client Credentials Token
    /// </summary>
    public class CCToken : ISpotifyToken
    {
        public string ClientId { get; }
        public string ClientSecret { get; }

        public string AccessToken { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool Expired => ExpiresAt <= DateTime.Now;
        public string TokenType { get; }

        internal CCToken(string clientId, string clientSecret, CCTokenJSON t)
            => (ClientId, ClientSecret, AccessToken, ExpiresAt, TokenType) = (clientId, clientSecret, t.Access_token, DateTime.Now.AddSeconds(t.Expires_in), t.Token_type);

        public async Task<string> GetAccessToken()
        {
            if (!Expired) return AccessToken;

            await Refresh();
            return AccessToken;
        }

        public async Task Refresh()
        {
            var t = await Authorization.ClientCredentials.GetNewToken(this);
            (AccessToken, ExpiresAt) = (t.AccessToken, t.ExpiresAt);
        }
        public async Task<CCToken> GetNewToken() => await Authorization.ClientCredentials.GetNewToken(this);
    }

}