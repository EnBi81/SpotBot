using AGoodSpotifyAPI.InterFaces;
using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Classes
{
    public class CurrentUser : IUser
    {
        /// <summary>
        /// The country of the user, as set in the user’s account profile. An ISO 3166-1 alpha-2 country code. This field is only available when the current user has granted access to the user-read-private scope.
        /// </summary>
        public Markets? Country { get; }
        /// <summary>
        /// <para>The user’s email address, as entered by the user when creating their account.</para>
        /// <para>Important! This email address is unverified; there is no proof that it actually belongs to the user.</para>
        /// This field is only available when the current user has granted access to the user-read-email scope.
        /// </summary>
        public string Email { get; }                                                               
        /// <summary>
        /// <para>The user’s Spotify subscription level: “premium”, “free”, etc. (The subscription level “open” can be considered the same as “free”.)</para>
        /// This field is only available when the current user has granted access to the user-read-private scope.
        /// </summary>
        public string Product { get; }

        public string DisplayName { get; }
        public string ExternalUrl { get; }
        public int Followers { get; }
        public string Href { get; }
        public string Id { get; }
        public Image[] Images { get; }
        public string Uri { get; }

        internal CurrentUser(UserPrivateJSON user)
        {
            if (user.Country != null)
                Country = Enum.Parse<Markets>(user.Country);
            else Country = null;
            Email = user.Email;
            Product = user.Product;

            DisplayName = user.Display_name;
            ExternalUrl = user.External_urls.Spotify;
            Followers = user.Followers is null ? 0 : (user.Followers.Total ?? 0);
            Href = user.Href;
            Id = user.Id;
            Images = user.Images is null ? new Image[0] : (from i in user.Images select new Image(i)).ToArray();
            Uri = user.Uri;
        }

        #region Statikus
        /// <summary>
        /// Reading the user’s email address requires the user-read-email scope; reading country and product subscription level requires the user-read-private scope.
        /// </summary>
        /// <param name="token">A valid access token from the Spotify Accounts service. The access token must have been issued on behalf of the current user.</param>
        /// <returns></returns>
        public static async Task<CurrentUser> GetCurrentUser(string token)
        {
            var res = await WebHelper.GetUserPrivateHelper(token).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);

            return new CurrentUser(res.Result);
        }

        #endregion

        public async Task<PlayList[]> GetPlaylists(string token) => await PlayList.GetCurrentUserPlaylistsAsync(token);
        public async Task<TrackList<SavedTrack>> GetSavedTracksAsync(string token) => await SavedTrack.GetSavedTracks(token);

        public bool Equals(IUser user) => Id == user.Id;
    }
}
