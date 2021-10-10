using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Auth.JSON
{
    internal class PKCETokenJSON
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public int Expires_in { get; set; }
        public string Refresh_token { get; set; }
        public string Scope { get; set; }
    }

    internal class CCTokenJSON
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public int Expires_in { get; set; }
    }
}
