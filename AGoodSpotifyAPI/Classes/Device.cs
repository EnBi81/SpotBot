using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Classes
{
    public class Device
    {
        public string Id { get; }
        public bool IsActive { get; }
        public bool IsPrivateSession { get; }
        public bool IsRestricted { get; }
        public string Name { get; }
        public DeviceType Type { get; }
        public int? Volume { get; }

        internal Device(DeviceJSON dev)
        {
            Id = dev.Id;
            IsActive = dev.Is_active;
            IsPrivateSession = dev.Is_private_session;
            IsRestricted = dev.Is_restricted;
            Name = dev.Name;
            Type = Enum.Parse<DeviceType>(dev.Type);
            Volume = dev.Volume_percent;
        }


        public static async Task<Device[]> GetUserDevices(string token)
        {
            var res = await WebHelper.GetUserDevices(token).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);

            return (from d in res.Result.Devices select new Device(d)).ToArray();

        }
    }
}
