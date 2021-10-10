namespace AGoodSpotifyAPI.JsonSchema
{
    internal class TimeIntervalJSON
    {
        /// <summary>
        /// The starting point (in seconds) of the time interval.
        /// </summary>
        public float Start { get; set; }
        /// <summary>
        /// The duration (in seconds) of the time interval.
        /// </summary>
        public float Duration { get; set; }
        /// <summary>
        /// The confidence, from 0.0 to 1.0, of the reliability of the interval.
        /// </summary>
        public float Confidence { get; set; }
    }
}