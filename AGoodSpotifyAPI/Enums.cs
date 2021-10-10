namespace AGoodSpotifyAPI
{
    public enum DeviceType
    {
        Computer, Tablet, Smartphone, Speaker,
        TV, AVR, STB, AudioDongle, GameConsole,
        CastVideo, CastAudio, Automobile, Unknown
    }

    public enum AlbumType
    {
        Album, Single, Compilation
    }

    public enum ReleaseDatePrecision
    {
        Year, Month, Day
    }

    /// <summary>
    /// For more informations please visit
    /// <seealso cref="https://developer.spotify.com/documentation/general/guides/scopes/"/>
    /// </summary>
    public enum AuthScopes
    {
        /// <summary>
        /// Write access to user-provided images.
        /// </summary>
        Ugc_Image_Upload,
        /// <summary>
        /// Read your currently playing content and Spotify Connect devices information.
        /// </summary>
        User_Read_Playback_State,
        /// <summary>
        /// Play content and control playback on your other devices. (Requires Spotify Premium)
        /// </summary>
        Streaming,
        /// <summary>
        /// Get your real email address.
        /// </summary>
        User_Read_Email,
        /// <summary>
        /// Access your collaborative playlists.
        /// </summary>
        Playlist_Read_Collaborative,
        /// <summary>
        /// Control playback on your Spotify clients and Spotify Connect devices.
        /// </summary>
        User_Modify_Playback_State,
        /// <summary>
        /// Access your subscription details.
        /// </summary>
        User_Read_Private,
        /// <summary>
        /// Manage your public playlists.
        /// </summary>
        Playlist_Modify_Public,
        /// <summary>
        /// Manage your saved content.
        /// </summary>
        User_Library_Modify,
        /// <summary>
        /// Read your top artists and content.
        /// </summary>
        User_Top_Read,
        /// <summary>
        /// Read your position in content you have played.
        /// </summary>
        User_Read_Playback_Position,
        /// <summary>
        /// Read your currently playing content.
        /// </summary>
        User_Read_Currently_Playing,
        /// <summary>
        /// Access your private playlists.
        /// </summary>
        Playlist_Read_Private,
        /// <summary>
        /// Access your followers and who you are following.
        /// </summary>
        User_Follow_Read,
        /// <summary>
        /// Communicate with the Spotify app on your device.
        /// </summary>
        App_Remote_Control,
        /// <summary>
        /// Access your recently played items.
        /// </summary>
        User_Read_Recently_Played,
        /// <summary>
        /// Manage your private playlists.
        /// </summary>
        Playlist_Modify_Private,
        /// <summary>
        /// Manage who you are following.
        /// </summary>
        User_Follow_Modify,
        /// <summary>
        /// Access your saved content.
        /// </summary>
        User_Library_Read


    }

    public enum Markets
    {
        FromToken,
        AD, AE, Al, AR, AT, AU, BA, BE, BG, BH,
        BO, BR, BY, CA, CH, CL, CO, CR, CY, CZ,
        DE, DK, DO, DZ, EC, EE, EG, ES, FI, FR,
        GB, GR, GT, HK, HN, HR, HU, ID, IE, IL,
        IN, IS, IT, JO, JP, KW, KZ, LB, LI, LT,
        LU, LV, MA, MC, MD, ME, MK, MT, MX, MY,
        NI, NL, NO, NZ, OM, PA, PE, PH, PL, PS, 
        PT, PY, QA, RO, RS, RU, SA, SE, SG, SI,
        SK, SV, TH, TN, TR, TW, UA, US, UY, VN,
        XK, ZA 
    }       

    public enum AlbumGroups
    {
        Album, Single, Appears_on, Compilation
    }

    public enum SearchType
    {
        Album, Artist, Playlist, Track, Show, Episode
    }

    public enum TimeRange
    {
        /// <summary>
        /// Calculated from several years of data and including all new data as it becomes available
        /// </summary>
        LongTerm,
        /// <summary>
        /// Approximately last 6 months
        /// </summary>
        MediumTerm,
        /// <summary>
        /// Approximately last 4 weeks
        /// </summary>
        ShortTerm
    }
}
