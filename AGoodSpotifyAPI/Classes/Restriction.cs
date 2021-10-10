using AGoodSpotifyAPI.JsonSchema;
using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Classes
{
    public class Restriction
    {
        public string Reason { get; }
        

        internal Restriction(RestrictionJSON rest)
        {
            Reason = rest is null ? null : rest.Reason ?? null;
        }
    }
}
