using AGoodSpotifyAPI.InterFaces;
using Victoria;
using System;
using System.Linq;
using System.Threading.Tasks;
using SpotBot.Helpers;
using SpotBot.Exceptions;

namespace SpotBot
{
    public class STrack
    {
        private LavaTrack LavaTrack { get; set; }

        private bool IsSpotifyTrack { get; }

        public STrack(ITrack spotTrack)
        {
            if (spotTrack is null) throw new ArgumentNullException(nameof(spotTrack));
            Url = "https://open.spotify.com/track/" + spotTrack.Id;
            if (spotTrack.ArtistNames.Any()) Title = spotTrack.ArtistNames.First() + " - " + spotTrack.Name;
            else Title = spotTrack.Name;
            Duration = TimeSpan.FromMilliseconds(spotTrack.Duration);
            IsSpotifyTrack = true;
            
        }

        public STrack(YoutubeExplode.Videos.Video ytVideo)
        {
            if (ytVideo is null) throw new ArgumentNullException(nameof(ytVideo));
            Url = ytVideo.Url;
            Title = ytVideo.Title;
            Duration = ytVideo.Duration;
            IsSpotifyTrack = false;
        }

        public STrack(LavaTrack track)
        {
            if (track is null) throw new ArgumentNullException(nameof(track));
            LavaTrack = track;
            Url = track.Url;
            Title = track.Title;
            Duration = track.Duration;
            IsSpotifyTrack = false;
        }

        public async Task<LavaTrack> GetLavaTrack()
        {
            if (LavaTrack is not null) return LavaTrack;
            var query = IsSpotifyTrack ? Url : Title;
            var songs = await Services.LavaNodeService.FirstNode.SearchSongs(query);

            if (!songs.Any()) throw new LavaTrackNotFoundException();

            LavaTrack = songs.First();
            return LavaTrack;
        }
        public string Url { get; }
        public string Title { get; }
        public TimeSpan Duration { get; }



        public static implicit operator STrack(AGoodSpotifyAPI.Classes.Track track) => new(track);
        public static implicit operator STrack(AGoodSpotifyAPI.Classes.SavedTrack track) => new(track);
        public static implicit operator STrack(AGoodSpotifyAPI.Classes.PlaylistTrack track) => new(track);
        public static implicit operator STrack(YoutubeExplode.Videos.Video video) => new(video);
        public static implicit operator STrack(LavaTrack track) => new(track);

    }
}
