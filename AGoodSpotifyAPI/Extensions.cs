using AGoodSpotifyAPI.Classes;
using AGoodSpotifyAPI.InterFaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI
{
    public static class Extensions
    {
        public static TrackList<ITrack> ToTracklist(this IEnumerable<ITrack> collection)
        {
            TrackList<ITrack> list = new TrackList<ITrack>(collection);

            return list;
        }

    }
}
