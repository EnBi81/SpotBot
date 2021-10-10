using AGoodSpotifyAPI.Classes;
using System.Data;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface IUser
    {
        /// <summary>
        /// The name displayed on the user’s profile. null if not available.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Known public external URLs for this user.
        /// </summary>
        public string ExternalUrl { get; }
        /// <summary>
        /// Information about the followers of this user.
        /// </summary>
        public int Followers { get; }
        /// <summary>
        /// A link to the Web API endpoint for this user.                                                 
        /// </summary>
        public string Href { get; }
        /// <summary>
        /// The Spotify user ID for this user.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// The user’s profile image.
        /// </summary>
        public Image[] Images { get; }
        /// <summary>
        /// The Spotify URI for this user.
        /// </summary>
        public string Uri { get; }

        Task<PlayList[]> GetPlaylists(string token);
        public bool Equals(IUser user);

    }
}