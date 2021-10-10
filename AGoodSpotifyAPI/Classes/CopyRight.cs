using AGoodSpotifyAPI.JsonSchema;
using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Classes
{
    /// <summary>
    /// CopyRight object
    /// </summary>
    public class CopyRight
    {
        /// <summary>
        /// The copyright text for this album.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// The type of copyright: C = the copyright, P = the sound recording (performance) copyright.
        /// </summary>
        public string Type { get; }

        internal CopyRight(CopyRightJSON copy)
        {
            Text = copy.Text;
            Type = copy.Type;
        }
    }
}
