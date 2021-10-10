using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class ResumepointJSON
    {
        /// <summary>
        /// Whether or not the episode has been fully played by the user.
        /// </summary>
        public bool? Fully_played { get; set; }
        /// <summary>
        /// The user’s most recent position in the episode in milliseconds.
        /// </summary>
        public int? Resume_position_integer { get; set; }
    }
}
