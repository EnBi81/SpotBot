using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotBot.Modules
{
    [RequireOwner]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Command("LeaveServer")]
        [Alias("LeaveGuild", "LeaveThis", "LeaveThisShitHole", "Leave Server", "Leave Guild")]
        public async Task LeaveServer() => await Context.Guild.LeaveAsync();

    }
}
