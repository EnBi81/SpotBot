using Newtonsoft.Json;
using System.IO;

namespace SpotBot.Configs
{
    public class Config
    {
        public string Token { get; set; }
        public string Connectionstring { get; set; }
        public string Clientid { get; set; }
        public string Clientsecret { get; set; }
        public string InviteLink { get; set; }
        public string Webpage { get; set; }
        public bool LogLevelDebug { get; set; }
        public string JavaLocation { get; set; }
        public ushort Port { get; set; }


        private static string FileName => @".\config.json";

        public static Config GetConfig()
        {
            var text = File.ReadAllText(FileName);

            Config c = JsonConvert.DeserializeObject<Config>(text);

            return c;
        }
    }
}