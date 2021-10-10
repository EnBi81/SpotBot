using System;
using System.Collections.Generic;
using System.Text;
using Victoria;

namespace SpotBot.Spotify
{
    public class LavaTrackHelper
    {
        public string Author { get; set; }
        public bool CanSeek { get; set; }
        public long Duration { get; set; }
        public string Hash { get; set; }
        public string Id { get; set; }
        public bool IsStream { get; set; }
        public TimeSpan Position { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }








        public static implicit operator LavaTrack(LavaTrackHelper helper)
        {
            LavaTrack track = new LavaTrack(hash: helper.Hash, id: helper.Id, title: helper.Title,
                author: helper.Author, url: helper.Url, position: TimeSpan.Zero,
                duration: helper.Duration, canSeek: helper.CanSeek, isStream: helper.IsStream);

            return track;
        }
        public static implicit operator LavaTrackHelper(LavaTrack track)
        {
            LavaTrackHelper helper = new LavaTrackHelper()
            {
                Author = track.Author,
                CanSeek = track.CanSeek,
                Duration = (long)track.Duration.TotalMilliseconds,
                Hash = track.Hash,
                Id = track.Id,
                IsStream = track.IsStream,
                Position = track.Position,
                Title = track.Title,
                Url = track.Url
            };

            return helper;
        }

    }
}
