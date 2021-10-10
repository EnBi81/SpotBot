namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// If an action is included in the disallows object and set to true, that action is not allowed (disallowed = true equals not allowed).
    /// If an action is not included in the disallows object or set to false or undefined, that action is allowed(disallowed = false equals allowed).
    /// </summary>
    internal class DisallowsJSON
    {
        public bool? Interrupting_playback { get; set; }
        public bool? Pausing { get; set; }
        public bool? Resuming { get; set; }
        public bool? Seeking { get; set; }
        public bool? Skipping_next { get; set; }
        public bool? Skipping_prev { get; set; }
        public bool? Toggling_repeat_context { get; set; }
        public bool? Toggling_shuffle { get; set; }
        public bool? Toggling_repeat_track { get; set; }
        public bool? Transferring_playback { get; set; }
    }
}
