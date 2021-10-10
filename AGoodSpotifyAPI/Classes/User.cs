using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using AGoodSpotifyAPI.InterFaces;

namespace AGoodSpotifyAPI.Classes
{
    /// <summary>
    /// A Spotify User
    /// </summary>
    public class User : IUser
    {
        public string DisplayName { get; }
        public string ExternalUrl { get; }
        public int Followers { get; }
        public string Href { get; }
        public string Id { get; }
        public Image[] Images { get; }
        public string Uri { get; }

        internal User(UserPublicJSON user)
        {
            DisplayName = user.Display_name;
            ExternalUrl = user.External_urls.Spotify;
            Followers = user.Followers is null ? 0 : (user.Followers.Total ?? 0);
            Href = user.Href;
            Id = user.Id;
            Images = user.Images is null ? new Image[0] : (from i in user.Images select new Image(i)).ToArray();
            Uri = user.Uri;
        }

        public static async Task<User> GetUser(string token, string userId)
        {
            var res = await WebHelper.GetUserPublicHelper(token, userId).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);

            return new User(res.Result);
        }

        public virtual async Task<PlayList[]> GetPlaylists(string token) => await PlayList.GetUserPlaylistsAsync(token, Id);

        public override bool Equals(object obj) => obj is User && Id == (obj as User).Id;
        public bool Equals(IUser user) => Id == user.Id;
        /// <summary>
        /// This object as string
        /// </summary>
        /// <returns>DisplayName [Id]</returns>
        public override string ToString() => $"{DisplayName} [{Id}]";
        public override int GetHashCode() => HashCode.Combine(this);

        public static bool operator ==(User left, User right) => left.Equals(right);
        public static bool operator !=(User left, User right) => !(left == right);

    }
}
