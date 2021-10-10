using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Exceptions
{
    interface IPlayerError
    {
        public int ErrorCode { get; }
        public string Reason { get; }
        public string Description { get; }
    }
}
