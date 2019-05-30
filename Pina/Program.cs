using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using DiscordUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pina
{
    class Program
    {
        public static async Task Main()
            => await new Program().MainAsync();

        public readonly DiscordSocketClient client;
        private readonly CommandService commands = new CommandService();

        public DateTime StartTime { private set; get; }
        public static Program P { private set; get; }

        private Program()
        {
            P = this;
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Utils.Log;
            commands.Log += Utils.LogError;
        }

        private async Task MainAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            client.ReactionAdded += ReactionAdded;

            await commands.AddModuleAsync<CommunicationModule>(null);
            await commands.AddModuleAsync<PinModule>(null);

            await client.LoginAsync(TokenType.Bot, File.ReadAllText("Keys/token.txt"));
            StartTime = DateTime.Now;
            await client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task PinMessageAsync(IMessage msg)
        {
            if (msg.IsPinned)
                await msg.Channel.SendMessageAsync("This message was already pinned");
            else
            {
                try
                {
                    await ((IUserMessage)msg).PinAsync();
                }
                catch (HttpException)
                {
                    await msg.Channel.SendMessageAsync("I wasn't able to pin the message, please make sure that I have the 'Manage Messages' permission.");
                }
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel _, SocketReaction react)
        {
            if (react.Emote.Name == "📌" || react.Emote.Name == "📍")
                await PinMessageAsync(await msg.GetOrDownloadAsync());
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage msg = arg as SocketUserMessage;
            if (msg == null || arg.Author.IsBot) return;
            int pos = 0;
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || msg.HasStringPrefix("p.", ref pos))
            {
                SocketCommandContext context = new SocketCommandContext(client, msg);
                await commands.ExecuteAsync(context, pos, null);
            }
        }
    }
}
