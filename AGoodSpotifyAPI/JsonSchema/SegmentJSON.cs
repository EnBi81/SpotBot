namespace AGoodSpotifyAPI.JsonSchema
{
    internal class SegmentJSON : TimeIntervalJSON
    {
        /// <summary>
        /// The onset loudness of the segment in decibels (dB). Combined with loudness_max and loudness_max_time, these components can be used to describe the “attack” of the segment.
        /// </summary>
        public float Loudness_start { get; set; }
        /// <summary>
        /// The peak loudness of the segment in decibels (dB). Combined with loudness_start and loudness_max_time, these components can be used to describe the “attack” of the segment.
        /// </summary>
        public float Loudness_max { get; set; }
        /// <summary>
        /// The segment-relative offset of the segment peak loudness in seconds. Combined with loudness_start and loudness_max, these components can be used to describe the “attack” of the segment.
        /// </summary>
        public float Loudness_max_time { get; set; }
        /// <summary>
        /// The offset loudness of the segment in decibels (dB). This value should be equivalent to the loudness_start of the following segment.
        /// </summary>
        public float Loudness_end { get; set; }
        /// <summary>
        /// A “chroma” vector representing the pitch content of the segment, corresponding to the 12 pitch classes C, C#, D to B, with values ranging from 0 to 1 that describe the relative dominance of every pitch in the chromatic scale. More details about how to interpret this vector can be found below.
        /// </summary>
        public float[] Pitches { get; set; }
        /// <summary>
        /// imbre is the quality of a musical note or sound that distinguishes different types of musical instruments, or voices. Timbre vectors are best used in comparison with each other. More details about how to interpret this vector can be found on the below.
        /// </summary>
        public float[] Timbre { get; set; }
    }
}