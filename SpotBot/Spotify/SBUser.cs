using AGoodSpotifyAPI.Auth;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

namespace SpotBot.Spotify
{
    public class SBUser
    {
        public ulong DiscordId { get; private set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public DateTime ExpiresOn { get; private set; }
        public bool Expired { get => ExpiresOn <= DateTime.Now.AddMinutes(1); }
        public bool Premium { get; private set; }
        public string UserName { get; private set; }

        public SBUser()
        {
             
        }
        public SBUser(ulong id, string ac, string rf, DateTime exp, bool prm, string userName)
            => (DiscordId, AccessToken, RefreshToken, ExpiresOn, Premium, UserName) = (id, ac, rf, exp, prm, userName);

        public async Task<string> GetAccessToken()
        {
            if (!Expired) return AccessToken;
            PKCEToken t;
            try
            {
                t = await Authorization.PKCE.RefreshTokenAsync(DataHelper.ClientId, RefreshToken);
                (AccessToken, RefreshToken, ExpiresOn) = (t.AccessToken, t.RefreshToken, t.ExpiresAt);
                await ModifyUser(DiscordId, newAccess: AccessToken, newRefresh: RefreshToken, newDate: ExpiresOn);
            }
            catch { throw; }
            

            return AccessToken;
        }

        public override bool Equals(object obj) => obj is SBUser && (obj as SBUser).DiscordId == DiscordId;
        public override int GetHashCode() => HashCode.Combine(this);

        public static async Task<SBUser> GetUser(ulong id, bool returnLoggedOut = false)
        {
            if(AccessTokens.ContainsKey(id))
            {
                return AccessTokens[id];
            }

            var user = await DataHelper.GetUser(id);
            if (user is null) return null;
            user.DiscordId = id;

            if (user.AccessToken == "Out")
            {
                if (!returnLoggedOut)
                    return null;
            }
            else
            {
                try
                {
                    AccessTokens.Add(id, user);
                }
                catch { }
            }

            
            return user;
        }
        public static async Task<int> DeleteUser(ulong id) => await DataHelper.DeleteUser(id);
        public static async Task<int> AddUser(SBUser user) => await DataHelper.AddUser(user);
        public static async Task<int> ModifyUser(ulong id, ulong? newId = null, string newAccess = null, string newRefresh = null, DateTime? newDate = null, bool? newPremium = null)
            => await DataHelper.ModifyUser(id, newId, newAccess, newRefresh, newDate, newPremium);

        public static async Task<int> LogoutUser(ulong id)
        {
            AccessTokens.Remove(id);
            return await ModifyUser(id, newAccess: "Out", newRefresh: "");
        }

        public static bool operator ==(SBUser left, SBUser right) => left.Equals(right);
        public static bool operator !=(SBUser left, SBUser right) => !(left == right);


        private static Dictionary<ulong, SBUser> AccessTokens { get; } = new Dictionary<ulong, SBUser>();

        private static Timer TimerExecute { get; } = new Timer(1000 * 5) { AutoReset = true, Enabled = true };
        internal static void SetTimer()
        {
            TimerExecute.Elapsed += (sen, e) =>
            {
                var users = from u in AccessTokens where u.Value.Expired select u.Key;
                foreach (var u in users)
                {
                    AccessTokens.Remove(u);
                }
            };
        }
    }
}
