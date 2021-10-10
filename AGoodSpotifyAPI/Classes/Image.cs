using AGoodSpotifyAPI.JsonSchema;
using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Classes
{
    public class Image
    {
        /// <summary>
        /// The image height in pixels. If unknown: null or not returned.
        /// </summary>
        public int? Height { get; }
        /// <summary>
        /// The image width in pixels. If unknown: null or not returned.
        /// </summary>
        public int? Width { get; }
        /// <summary>
        /// The source URL of the image.
        /// </summary>
        public string Url { get; }

        internal Image(ImageJSON image)
        {
            Height = image.Height;
            Width = image.Width;
            Url = image.Url;
        }
    }
}
