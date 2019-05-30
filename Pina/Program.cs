using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using DiscordUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        private string statsWebsite, statsToken;

        private Db db;

        public Dictionary<string, Dictionary<string, string>> translations;
        public Dictionary<string, List<string>> translationKeyAlternate;

        public Db GetDb()
            => db;

        private Program()
        {
            P = this;
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Utils.Log;
            commands.Log += Utils.Log;
        }

        private async Task MainAsync()
        {
            if (!File.Exists("Keys/Credentials.json"))
                throw new FileNotFoundException("Missing Keys/Credentials.json");
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText("Keys/Credentials.json"));
            if (json.botToken == null)
                throw new NullReferenceException("Missing botToken in Credentials file");
            statsWebsite = json.statsWebsite;
            statsToken = json.statsToken;

            db = new Db();
            await db.InitAsync();

            translations = new Dictionary<string, Dictionary<string, string>>();
            translationKeyAlternate = new Dictionary<string, List<string>>();
            Utils.InitTranslations(translations, translationKeyAlternate, "../../Pina-translations/Translations");

            client.MessageReceived += HandleCommandAsync;
            client.ReactionAdded += ReactionAdded;
            client.GuildAvailable += InitGuild;
            client.JoinedGuild += InitGuild;

            await commands.AddModuleAsync<CommunicationModule>(null);
            await commands.AddModuleAsync<PinModule>(null);
            await commands.AddModuleAsync<SettingsModule>(null);

            await client.LoginAsync(TokenType.Bot, (string)json.botToken);
            StartTime = DateTime.Now;
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task InitGuild(SocketGuild guild)
        {
            await db.InitGuildAsync(guild.Id);
        }

        public async Task PinMessageAsync(IMessage msg, ulong? guildId)
        {
            if (msg.IsPinned)
            {
                if (db.GetVerbosity(msg.Channel as ITextChannel == null ? (ulong?)null : ((ITextChannel)msg.Channel).Guild.Id) == Db.Verbosity.Info)
                    await msg.Channel.SendMessageAsync(Sentences.AlreadyPinned(guildId));
            }
            else
            {
                try
                {
                    await ((IUserMessage)msg).PinAsync();
                }
                catch (HttpException)
                {
                    if (db.IsErrorOrMore(db.GetVerbosity(msg.Channel as ITextChannel == null ? (ulong?)null : ((ITextChannel)msg.Channel).Guild.Id)))
                        await msg.Channel.SendMessageAsync(Sentences.MissingPermission(guildId));
                }
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel _, SocketReaction react)
        {
            if (react.Emote.Name == "📌" || react.Emote.Name == "📍")
            {
                bool isInGuild = react.Channel as ITextChannel == null;
                if (isInGuild && !db.IsWhitelisted(((ITextChannel)react.Channel).Guild.Id, react.User.Value))
                {
                    ulong id = ((ITextChannel)react.Channel).Guild.Id;
                    if (db.IsErrorOrMore(db.GetVerbosity(id)))
                        await react.Channel.SendMessageAsync(Sentences.WhitelistError(id, react.User.Value.Mention));
                }
                else
                    await PinMessageAsync(await msg.GetOrDownloadAsync(), isInGuild ? (ulong?)null : ((ITextChannel)react.Channel).Guild.Id);
                await Utils.WebsiteUpdate("Pina", statsWebsite, statsToken, "nbMsgs", "1");
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage msg = arg as SocketUserMessage;
            if (msg == null || arg.Author.IsBot) return;
            int pos = 0;
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || msg.HasStringPrefix(db.GetPrefix(msg.Channel as ITextChannel == null ? (ulong?)null : ((ITextChannel)msg.Channel).Guild.Id), ref pos))
            {
                SocketCommandContext context = new SocketCommandContext(client, msg);
                IResult result = await commands.ExecuteAsync(context, pos, null);
                if (result.IsSuccess && statsWebsite != null && statsToken != null)
                    await Utils.WebsiteUpdate("Pina", statsWebsite, statsToken, "nbMsgs", "1");
            }
        }
    }
}
