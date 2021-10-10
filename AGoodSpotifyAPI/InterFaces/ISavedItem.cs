using System;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface ISavedItem
    {
        /// <summary>
        /// The date and time the item was saved.
        /// </summary>
        public DateTime SavedAt { get; }

        public Task UnSave(string token);
    }
}
