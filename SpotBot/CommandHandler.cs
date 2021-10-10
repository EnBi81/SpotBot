using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SpotBot.Helpers;
using SpotBot.Modules;
using SpotBot.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SpotBot
{
    internal class CommandHandler : IDisposable
    {
        private DiscordSocketClient _client;
        private  CommandService _cmdService;
        private IServiceProvider _services;


        public CommandHandler(DiscordSocketClient client, CommandService cmdService, IServiceProvider services)
        {
            _client = client;
            _cmdService = cmdService;
            _services = services;
        }

        public void Dispose()
        {
            _client.MessageReceived -= MessageRecieved;
            _client = null;
            _cmdService = null;
            _services = null;
            
        }

        internal async Task InitializeAsync()
        {
            await _client.SetStatusAsync(UserStatus.Online);
            _client.MessageReceived += MessageRecieved;
            _cmdService.Log += async msg => await SpotBotClient.LogService.LogAsync(msg);
        }  

        private Task MessageRecieved(SocketMessage arg)
        {
            
            try
            {
                new Thread(async () => {
                    if (arg is not SocketUserMessage message)
                    {
                        return;
                    }
                    if(!SpotBotClient.React)
                    {
                        if (message.Content.ToLower().Contains("reacting") && message.Author.Id == (await _client.GetApplicationInfoAsync()).Owner.Id)
                        {
                            SpotBotClient.React = true;
                            await _client.SetStatusAsync(UserStatus.Online);
                            await message.AddReactionAsync(EmojiHelper.Done);
                        }

                        return;
                    }

                    if (arg.Author.IsBot) return;
                    var context = new SocketCommandContext(_client, message);

                    if (SpotBotClient.Config.LogLevelDebug)
                        Console.WriteLine($"{message.Author} ({message.Channel.Name}): {message.Content}");

                    ulong id = context.Guild is null ? 0 : context.Guild.Id;

                    int argpos = 0;
                    bool execute = message.HasStringPrefix("!sb", ref argpos) 
                    || message.HasStringPrefix(GuildService.GetPrefix(id), ref argpos) 
                    || message.HasMentionPrefix(_client.CurrentUser, ref argpos);

                    if (execute) await _cmdService.ExecuteAsync(context, argpos, _services);

                }).Start();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return Task.CompletedTask;

        }
    }
}