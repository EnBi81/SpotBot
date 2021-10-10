using AGoodSpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Classes
{
    public class SpotPlayer
    {
        public static async Task<bool> Play(string token, IEnumerable<string> uris, string deviceId = null)
        {
            var res = await WebHelper.PlayerPlayTracks(token, uris, deviceId);
            if (res.e is null) return res.Success;

            throw res.e;
        }
        public static async Task<bool> QueueTrack(string token, string trackUri)
        {
            var resp = await WebHelper.QueueTrack(token, trackUri);

            if (!(resp.Exception is null)) throw resp.Exception;

            return resp.Success;
        }
    }
}
 