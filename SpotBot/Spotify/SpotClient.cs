using System;
using System.Threading.Tasks;
using AGoodSpotifyAPI.Auth;
using Newtonsoft.Json;
using static AGoodSpotifyAPI.Auth.Authorization;

namespace SpotBot.Spotify
{
    public class SpotClient
    {
        private static string ClientId { get; } = SpotBotClient.Config.Clientid;
        private static string ClientSecret { get; } = SpotBotClient.Config.Clientsecret;

        private static CCToken Token { get; set; } = null;
        private static bool Expired => Token.ExpiresAt <= DateTime.Now.AddMinutes(1);

        public static async Task<string> GetTokenAsync()
        {
            if (Token is null) Token = await ClientCredentials.GetTokenAsync(ClientId, ClientSecret);
            if (Expired) await Token.Refresh();

            return Token.AccessToken;
        }

    }
}
