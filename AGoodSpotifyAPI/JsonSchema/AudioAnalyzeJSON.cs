namespace AGoodSpotifyAPI.JsonSchema
{
    internal class AudioAnalyzeJSON
    {
        /// <summary>
        /// The time intervals of the bars throughout the track. A bar (or measure) is a segment of time defined as a given number of beats. Bar offsets also indicate downbeats, the first beat of the measure.
        /// </summary>
        public TimeIntervalJSON[] Bars { get; set; }
        /// <summary>
        /// The time intervals of beats throughout the track. A beat is the basic time unit of a piece of music; for example, each tick of a metronome. Beats are typically multiples of tatums.
        /// </summary>
        public TimeIntervalJSON[] Beats { get; set; }
        /// <summary>
        /// Sections are defined by large variations in rhythm or timbre, e.g. chorus, verse, bridge, guitar solo, etc. Each section contains its own descriptions of tempo, key, mode, time_signature, and loudness.
        /// </summary>
        public SectionJSON[] Sections { get; set; }
        /// <summary>
        /// Audio segments attempts to subdivide a song into many segments, with each segment containing a roughly consistent sound throughout its duration.
        /// </summary>
        public SegmentJSON[] Segments { get; set; }
        /// <summary>
        /// A tatum represents the lowest regular pulse train that a listener intuitively infers from the timing of perceived musical events (segments). For more information about tatums, see Rhythm (below).
        /// </summary>
        public TimeIntervalJSON[] Tatums { get; set; }
    }
}
