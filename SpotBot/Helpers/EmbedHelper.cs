using System;
using System.Linq;
using AGoodSpotifyAPI.InterFaces;
using Discord;

namespace SpotBot.Helpers
{
    public static class EmbedHelper
    {
        public static Embed CreateEmbed
            (string title, Discord.IUser Author = null, Color? color = null, string description = null,
            DateTimeOffset? timestamp = null, bool withCurrentTimeStamp = false)
        {
            var e = new EmbedBuilder().WithTitle(title);
            if(!(Author is null))
                e = e.WithAuthor(Author);  
            if(!(color is null))
                e = e.WithColor(color.Value);
            if(!(description is null))
                e = e.WithDescription(description);
            if (!(timestamp is null))
                e = e.WithTimestamp(timestamp.Value);
            else if (withCurrentTimeStamp)
                e = e.WithCurrentTimestamp();
            

            return e.Build();
        }

        public static Embed SpotUser(AGoodSpotifyAPI.InterFaces.IUser user)
        {
            var embed = new EmbedBuilder()
               .WithAuthor(user.DisplayName, user.Images.Any() ? user.Images.First().Url : null, user.ExternalUrl)
               .WithTitle("Id: " + user.Id)
               .WithDescription("Followers: " + user.Followers)
               .WithCurrentTimestamp()
               .WithColor(Color.Green)
               .Build();
            return embed;
        }
    }
}
