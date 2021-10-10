using Discord;
using Discord.Commands;
using SpotBot.Spotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGoodSpotifyAPI.Auth;
using AGoodSpotifyAPI;
using AGoodSpotifyAPI.Classes;
using static AGoodSpotifyAPI.Auth.Authorization;
using System.Security.Cryptography;

namespace SpotBot.Modules
{
    public class SpotifyLoginModule : ModuleBase<SocketCommandContext>
    {
        public static readonly List<(ulong, string, string)> list = new List<(ulong, string, string)>();

        private static string CodeChallenge(string verifier)
        {
            var bytes = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(verifier));
            return IdentityModel.Base64Url.Encode(bytes);
        }

        [Command("LoginOld")]
        public async Task Login()
        {
            try
            {
                var user = Context.User;

                var scopes = new AuthScopes[]
            {
                AuthScopes.Playlist_Modify_Private, AuthScopes.Playlist_Modify_Public,
                   AuthScopes.Playlist_Read_Private, AuthScopes.User_Library_Read,
                   AuthScopes.Playlist_Read_Collaborative, AuthScopes.User_Read_Private,
                   AuthScopes.User_Modify_Playback_State, AuthScopes.User_Read_Playback_State,
                   AuthScopes.User_Library_Modify
            };
                var state = PKCE.CodeVerifier(10);
                var verifier = PKCE.CodeVerifier();
                var link = PKCE.GetUri(DataHelper.ClientId, "http://kossuthmovar.hu", CodeChallenge(verifier), state, scopes);
                link = link.Replace(" ", "%20");
                var embed = new EmbedBuilder()
                    .WithTitle("Please Login and Copy back the Full link here with the command \"Code\" (for example !sbCode http://kossuthmovar.hu/...)")
                    .WithDescription(link).WithColor(Color.Blue).Build();

                await user.SendMessageAsync(embed: embed);
                list.Add((user.Id, verifier, state));
            }catch
            (Exception e)
            {
                await ReplyAsync(e.Message);
                await ReplyAsync(e.StackTrace);
            }

        }

        [Command("Code")]
        [RequireContext(ContextType.DM)]
        public async Task GetCode(string link)
        {
            try
            {
               
                link = link.Substring(link.IndexOf('?') + 1);
                string state, verifier;
                ulong id;

                var lekerd = from a in list where a.Item1 == Context.User.Id select a;
                if (!lekerd.Any()) return;

                (id, verifier, state) = lekerd.First();
                list.Remove(lekerd.First());

                var code = PKCE.GetCode(link, state);
                PKCEToken token = null;
                                                                                                                                                  
                try
                {
                    token = await PKCE.GetTokenAsync(DataHelper.ClientId, code, "http://kossuthmovar.hu", verifier);
                }
                catch
                {
                    return;
                }
                if (token is null)
                {
                    await ReplyAsync("Something went wrong");
                    return;
                }
                Console.WriteLine(token.ExpiresAt);
                var user = await CurrentUser.GetCurrentUser(token.AccessToken);
                await ReplyAsync("Hello " + user.DisplayName);

                SBUser u = await SBUser.GetUser(Context.User.Id, true);
                if (u is null)
                {
                    u = new SBUser(id, token.AccessToken, token.RefreshToken, token.ExpiresAt, false, Context.User + "");
                    await SBUser.AddUser(u);
                }
                else
                {
                    await SBUser.ModifyUser(u.DiscordId, newAccess: token.AccessToken, newRefresh: token.RefreshToken, newDate: token.ExpiresAt);
                }
            }
            catch(Exception e)
            {
                await ReplyAsync(e.Message);
                await ReplyAsync(e.StackTrace);
            }

        }    

        [Command("Login")]
        public async Task LoginNew()
        {    
            try
            {
                static string RandomText(int count)
                {
                    var randomtext = string.Empty;

                    for (int i = 0; i < count; i++)
                    {
                        randomtext += (char)new Random().Next(65, 89);
                    }

                    return randomtext;
                }

                var id = Context.User.Id;
                var mode = IdentityModel.Base64Url.Encode(Encoding.Default.GetBytes(RandomText(5) + id)); //Convert.ToBase64String(Encoding.Default.GetBytes(id + ""));
                string url = $"http://localhost:5000/login?mode={mode}&token={RandomText(10)}";

                var e = new EmbedBuilder()
                    .WithAuthor("Click here to log in " + Context.User.Username, Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(), null)
                    .WithDescription(url)
                    .Build();

                await Context.User.SendMessageAsync(embed: e);
            }catch(Exception e)
            { Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        //public static Embed GetLoginEmbed(ulong userId)
        //{
        //    static string RandomText(int count)
        //    {
        //        var randomtext = string.Empty;

        //        for (int i = 0; i < count; i++)
        //        {
        //            randomtext += (char)new Random().Next(65, 89);
        //        }

        //        return randomtext;
        //    }

        //    var id = userId;
        //    var mode = IdentityModel.Base64Url.Encode(Encoding.Default.GetBytes(RandomText(5) + id)); //Convert.ToBase64String(Encoding.Default.GetBytes(id + ""));
        //    string url = $"http://localhost:5000/login?mode={mode}&token={RandomText(10)}";

        //    var e = new EmbedBuilder()
        //        .WithAuthor("Click here to log in " + Context.User.Username, Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(), null)
        //        .WithDescription(url)
        //        .Build();
        //}

        [Command("Logout")]
        public async Task Logout()
        {
            var ok = await SBUser.LogoutUser(Context.User.Id) > 0;
            var text = ok ? "Logged out successfully." : "You are not logged in.";
            await ReplyAsync(text);
        }
    }
}
