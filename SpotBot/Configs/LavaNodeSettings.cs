using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SpotBot.Configs
{
    public class LavaNodeSettings
    {
        public string Host { get; set; }
        public string Authorization { get; set; }
        public ushort Port { get; set; }
        public bool Connect { get => Use == 1; }
        public int Use { get; set; }


        public static string FileName => ".\\nodes.json";

        public static List<LavaNodeSettings> GetNodeSettings()
        {
            var txt = File.ReadAllText(FileName);

            return JsonConvert.DeserializeObject<List<LavaNodeSettings>>(txt);

        }
        
    }
}
