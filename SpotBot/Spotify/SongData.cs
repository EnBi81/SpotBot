using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace SpotBot.Spotify
{
    public class SongData
    {
        public string SpotId { get; set; }
        public string YTUrl { get; set; }
        public string JSON { get; set; }
        public LavaTrack Track { 
            get
            {
                if (JSON is null) return null;
                LavaTrackHelper track = JsonConvert.DeserializeObject<LavaTrackHelper>(JSON);
                return track;
            }
            set
            {
                if (value is null) return;
                LavaTrackHelper helper = value;
                helper.Title = helper.Title.Replace("\'", "").Replace("\"", "");
                JSON = JsonConvert.SerializeObject(helper);
            }
        }


        public async Task Save() => await AddSongData(this);

        public static async Task<SongData> GetSongData(string spotId = null, string ytUrl = null)
            => await DataHelper.GetSongData(spotId, ytUrl);

        public static async Task<int> AddSongData(SongData data, bool checkIfExist = true)
            => await DataHelper.AddSongData(data, checkIfExist);

    }
}
