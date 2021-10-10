using Discord;
using Discord.Commands;
using SpotBot.Helpers;
using SpotBot.Services;
using System.Threading.Tasks;

namespace SpotBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class PrefixModule : ModuleBase<SocketCommandContext>
    {
        [Command("Prefix")]
        public async Task GetPrefix()
        {
            string prefix = GuildService.GetPrefix(Context.Guild.Id);
            var embed = new EmbedBuilder().WithTitle($"I'm reacting to `{prefix}`").WithColor(Color.Orange).Build();
            await ReplyAsync(embed: embed);
        }

        [Command("Prefix")]
        public async Task SetPrefix(string prefix)
        {
            if(prefix.Contains("\"") || prefix.Contains("\'") ||prefix.Contains(";"))
            {
                await ReplyAsync("\" \' ; is prohibited in the prefix");
                return;
            }

            ulong id = Context.Guild.Id;
            var oldPrefix = GuildService.GetPrefix(id);
            Embed embed;

            if (prefix == oldPrefix)
            {
                embed = new EmbedBuilder().WithTitle("Nothing has changed.").WithColor(Color.Red).Build();
                await ReplyAsync(embed: embed);
                return;
            }

            if(prefix.Length > 10)
            {
                await ReplyAsync(EmojiHelper.Exit + " The prefix's length must be less or equal to 10.");
                return;
            }

            await GuildService.ChangePrefix(id, prefix);
            embed = new EmbedBuilder().WithTitle($"New Prefix: `{prefix}`").WithColor(Color.Green).Build();
            await ReplyAsync(embed: embed);
        }

        [Command("NoPrefix")]
        public async Task NoPrefix()
        {
            ulong id = Context.Guild.Id;

            await GuildService.ChangePrefix(id, "");
            var embed = new EmbedBuilder().WithTitle("Prefix has been turned off on this server.\nTo turn back on, use command prefix {yourPrefix}.").WithColor(Color.Green).Build();
            await ReplyAsync(embed: embed);
        }
    }
}
