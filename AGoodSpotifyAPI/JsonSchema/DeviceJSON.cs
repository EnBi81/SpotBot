namespace AGoodSpotifyAPI.JsonSchema
{
    internal class DeviceJSON
    {
        /// <summary>
        /// The device ID. This may be null.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// If this device is the currently active device.
        /// </summary>
        public bool Is_active { get; set; }
        /// <summary>
        /// If this device is currently in a private session.
        /// </summary>
        public bool Is_private_session { get; set; }
        /// <summary>
        /// Whether controlling this device is restricted. At present if this is “true” then no Web API commands will be accepted by this device.
        /// </summary>
        public bool Is_restricted { get; set; }
        /// <summary>
        /// The name of the device.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Device type, such as “Computer”, “Smartphone” or “Speaker”.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The current volume in percent. This may be null.
        /// </summary>
        public int? Volume_percent { get; set; }
    }
}
