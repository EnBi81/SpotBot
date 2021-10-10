using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Exceptions
{
    public class NoPreviousTrackException : Exception, IPlayerError
    {
        int IPlayerError.ErrorCode { get => 403; }
        string IPlayerError.Reason { get => "NO_PREV_TRACK"; }
        string IPlayerError.Description { get => "The command requires a previous track, but there is none in the context."; }
    }
}
