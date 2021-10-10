using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Specialized;
using System.Web;
using System.Security.Cryptography;
using System.Net.Http;
using AGoodSpotifyAPI.Auth.JSON;
using IdentityModel;

namespace AGoodSpotifyAPI.Auth
{
    public sealed class Authorization
    {
        private static readonly Encoding _encoding = Encoding.ASCII;

        /// <summary>
        /// Encode the parameter with base64 encoding
        /// </summary>
        /// <param name="plainText">This is the input text</param>
        /// <returns>The encoded text</returns>
        private static string Base64UrlEncode(byte[] text)
        {
            return Base64Url.Encode(text);
        }
        private static string Base64Encode(string text)
        {
            byte[] bytes = _encoding.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        
        /// <summary>
        /// Authorization Code Flow with Proof Key for Code Exchange (PKCE)
        /// <para>Step 1: Create the code verifier and challenge <see cref="PKCE.CodeVerifier(int, string)"/> and <see cref="PKCE.CodeChallenge(string)"/></para>
        /// <para>Step 2: Construct the authorization URI <see cref="PKCE.GetUri(string, string, string, string, AuthScopes[])"/></para>
        /// <para>Step 3: Your app redirects the user to the authorization URI (you can get the code via <see cref="PKCE.GetCode(string, string)"/></para>
        /// <para>Step 4: Your app exchanges the code for an access token <see cref="PKCE.GetTokenAsync(string, string, string, string)"/></para>
        /// <para>Step 5: Use the access token to access the Spotify Web API</para>
        /// <para>Step 6: Requesting a refreshed access token <see cref="PKCE.RefreshTokenAsync(string, string)"/></para>
        /// </summary>
        public class PKCE
        {
            public string ClientId { get; set; }
            public string Verifier { get; } = CodeVerifier();
            public string Challenge => CodeChallenge(Verifier);
            public List<AuthScopes> Scopes { get; set; } = new List<AuthScopes>();
            public string RedirectUri { get; set; }

            public string GetLoginLink(string redirectUri, string state = null) => GetUri(ClientId, RedirectUri = redirectUri, Challenge, state, Scopes.ToArray());

            public async Task<PKCEToken> GetToken(string code) => await GetTokenAsync(ClientId, code, RedirectUri, Verifier);


            public PKCE(string clientId) => ClientId = clientId;



            #region First step

            /// <summary>
            /// The code verifier is a cryptographically random string between 43 and 128 characters in length. It can contain letters, digits, underscores, periods, hyphens, or tildes.
            /// </summary>
            /// <param name="length">Between 43 and 128</param>
            /// <param name="allowedChars">Nothing to do with this</param>
            /// <returns>The Generated Verifier</returns>
            public static string CodeVerifier(int length = 100, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
            {
                if (length < 0 || length > 128) throw new ArgumentOutOfRangeException("length", "length can only be between 43 and 128.");
                if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

                const int byteSize = 0x100;
                var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
                if (byteSize < allowedCharSet.Length) throw new ArgumentException(string.Format("allowedChars may contain no more than {0} characters.", byteSize));

                using var rng = new RNGCryptoServiceProvider();
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
            /// <summary>
            /// In order to generate the code challenge, your app should hash the code verifier using the SHA256 algorithm. Then, base64url encode the hash that you generated.
            /// </summary>
            /// <param name="verifier"></param>
            /// <returns></returns>
            public static string CodeChallenge(string verifier)
            {
                using var sha256 = SHA256.Create();

                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(verifier));
                return Base64UrlEncode(challengeBytes);
            }
            #endregion

            #region Second step
            /// <summary>
            /// Creates the link with the given parameters
            /// </summary>
            /// <param name="clientId"></param>
            /// <param name="redirectUri"></param>
            /// <param name="codeChallenge"></param>
            /// <param name="state"></param>
            /// <param name="scopes"></param>
            /// <returns>The generated link</returns>
            public static string GetUri(string clientId, string redirectUri, string codeChallenge, string state = null, AuthScopes[] scopes = null)
            {
                var b = new StringBuilder("https://accounts.spotify.com/authorize?")
                   .Append("client_id=" + clientId)
                   .Append("&response_type=code")
                   .Append("&redirect_uri=" + redirectUri)
                   .Append("&code_challenge_method=S256")
                   .Append("&code_challenge=" + codeChallenge);
                
                if(!string.IsNullOrWhiteSpace(state))
                {
                    b = b.Append("&state=" + state);
                }
                if(!(scopes is null))
                {
                    if(scopes.Length > 0)
                    {
                        b = b.Append("&scope=");
                        scopes.ToList().ForEach(s => b = b.Append(s.ToString().ToLower().Replace('_','-') + " "));
                        return b.ToString()[..^1];
                    }
                }

                return b.ToString();
            }
            #endregion

            #region Third
            /// <summary>
            /// This returns the code from the query. It also checks if the state parameters match, if not then an exception will be thrown. If this is an error object, it throws a WebException with the message inside.
            /// </summary>
            /// <param name="parameters">The parameters after the question mark.</param>
            /// <param name="state">It checks that the 2 state parameters are the same. If not, then an exception will be thrown</param>
            /// <returns>The code from the query.</returns>
            /// <exception cref="WebException">If it's an error object, then a webexception will be throw with the message of it</exception>
            /// <exception cref="Exception">If the 2 state objects don't match</exception>
            public static string GetCode(string parameters, string state = null)
            {
                NameValueCollection collection = HttpUtility.ParseQueryString(parameters);

                if (!(state is null))
                    if (collection["state"] != state)
                        throw new Exception($"The 2 states don't matches{(collection.AllKeys.Contains("error") ? " (Also another error has occured)" : "")}. from query: {collection["state"]}  from method parameter: {state}");

                if (collection.AllKeys.Contains("error"))
                    throw new WebException(collection["error"]);

                return collection["code"];
            }
            #endregion

            #region Fourth
            public static async Task<PKCEToken> GetTokenAsync(string clientId, string code, string redirectUri, string codeVerifier)
            {
                var _client = new HttpClient();

                Dictionary<string, string> dic = new Dictionary<string, string>()
                {
                    ["client_id"] = clientId,
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri,
                    ["code_verifier"] = codeVerifier
                };


                using var content = new FormUrlEncodedContent(dic);

                using var response = await _client.PostAsync("https://accounts.spotify.com/api/token", content);
                string text = await response.Content.ReadAsStringAsync();
                PKCEToken t = new PKCEToken(JsonConvert.DeserializeObject<PKCETokenJSON>(text), clientId);

                return t;
            }

            #endregion

            #region Sixth

            public static async Task<PKCEToken> RefreshTokenAsync(string clientId, string refreshToken)
            {
                var _client = new HttpClient();

                var dic = new Dictionary<string, string>()
                {
                    ["client_id"] = clientId,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken
                };

                using var cont = new FormUrlEncodedContent(dic);

                using var response = await _client.PostAsync("https://accounts.spotify.com/api/token", cont);
                string text = await response.Content.ReadAsStringAsync();

                PKCEToken t = new PKCEToken(JsonConvert.DeserializeObject<PKCETokenJSON>(text), clientId);

                return t;
            }
            #endregion
        }

        public class ClientCredentials
        {
            public static async Task<CCToken> GetTokenAsync(string clientId, string clientSecret)
            {
                var dic = new Dictionary<string, string>() { ["grant_type"] = "client_credentials" };
                var cont = new FormUrlEncodedContent(dic);

                using var client = new HttpClient();
                string encoded = $"{clientId}:{clientSecret}";
                encoded = Base64Encode(encoded);

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);

                var res = await client.PostAsync("https://accounts.spotify.com/api/token", cont);
                var text = await res.Content.ReadAsStringAsync();

                return new CCToken(clientId, clientSecret, JsonConvert.DeserializeObject<CCTokenJSON>(text));

            }

            public static async Task<CCToken> GetNewToken(CCToken token) => await GetTokenAsync(token.ClientId, token.ClientSecret);
        }

        
    }

    
}
                                                                 