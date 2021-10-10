using Discord;

namespace SpotBot.Helpers
{
    public static class EmojiHelper
    {
        public static Emoji DoubleArrowUp => new Emoji("⏫");
        public static Emoji DoubleArrowDown => new Emoji("⏬");
        public static Emoji ArrowDown => new Emoji("🔽");
        public static Emoji ArrowUp => new Emoji("🔼");

        public static Emoji Play => new Emoji("▶️");
        public static Emoji Pause => new Emoji("⏸️");
        public static Emoji PausePlay => new Emoji("⏯️");
        public static Emoji Next => new Emoji("⏭️");
        public static Emoji Previous => new Emoji("⏮️");
        public static Emoji Mute => new Emoji("🔇");
        public static Emoji VolumeDown => new Emoji("🔉");
        public static Emoji VolumeUp => new Emoji("🔊");
        public static Emoji Loop => new Emoji("🔁");
        public static Emoji Shuffle => new Emoji("🔀");
        public static Emoji CurrentTrack => new Emoji("📑");

        public static Emoji Exit => new Emoji("❌");
        public static Emoji ColorChange => new Emoji("♻️");
        public static Emoji Stop => new Emoji("⏹️");
        public static Emoji Done => new Emoji("✅");
        public static Emoji Cool => new Emoji("🆒");
    }
}
