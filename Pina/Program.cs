using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using DiscordBotsList.Api;
using DiscordUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pina
{
    class Program
    {
        public static async Task Main()
            => await new Program().MainAsync();

        public readonly DiscordSocketClient client;
        private readonly CommandService commands = new();

        public DateTime StartTime { private set; get; }
        public static Program P { private set; get; }

        private string statsWebsite, statsToken;

        private Db db;

        public Dictionary<string, Dictionary<string, string>> translations;
        public Dictionary<string, List<string>> translationKeyAlternate;

        private AuthDiscordBotListApi dblApi;
        private DateTime lastDiscordBotsSent;

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
            lastDiscordBotsSent = DateTime.MinValue;
            if (json.dblId != null & json.dblToken != null)
                dblApi = new AuthDiscordBotListApi(ulong.Parse((string)json.dblId), (string)json.dblToken);
            else
                dblApi = null;

            db = new Db();
            await db.InitAsync();

            translations = new Dictionary<string, Dictionary<string, string>>();
            translationKeyAlternate = new Dictionary<string, List<string>>();
            Utils.InitTranslations(translations, translationKeyAlternate, "../../Pina-translations/Translations");

            client.MessageReceived += HandleCommandAsync;
            client.ReactionAdded += ReactionAdded;
            client.GuildAvailable += InitGuild;
            client.JoinedGuild += InitGuild;
            client.JoinedGuild += GuildCountChange;
            client.LeftGuild += GuildCountChange;
            client.Connected += UpdateDiscordBots;

            await commands.AddModuleAsync<CommunicationModule>(null);
            await commands.AddModuleAsync<PinModule>(null);
            await commands.AddModuleAsync<SettingsModule>(null);

            await client.LoginAsync(TokenType.Bot, (string)json.botToken);
            StartTime = DateTime.Now;
            await client.StartAsync();

            if (statsWebsite != null && statsToken != null)
            {
                var task = Task.Run(async () =>
                {
                    for (;;)
                    {
                        await Task.Delay(60000);
                        if (client.ConnectionState == ConnectionState.Connected)
                            await Utils.WebsiteUpdate("Pina", statsWebsite, statsToken, "serverCount", client.Guilds.Count.ToString());
                    }
                });
            }

            await Task.Delay(-1);
        }

        private async Task GuildCountChange(SocketGuild _)
        {
            await UpdateDiscordBots();
        }

        private async Task UpdateDiscordBots()
        {
            if (dblApi != null && lastDiscordBotsSent.AddMinutes(10).CompareTo(DateTime.Now) < 0)
            {
                lastDiscordBotsSent = DateTime.Now;
                await dblApi.UpdateStats(client.Guilds.Count);
            }
        }

        private async Task InitGuild(SocketGuild guild)
        {
            await db.InitGuildAsync(guild.Id);
        }

        public async Task PinMessageAsync(IMessage msg, IUser user, ulong? guildId, bool isFromEmote, bool pin)
        {
            bool isNotInGuild = msg.Channel as ITextChannel == null;
            if (!isNotInGuild && (!db.IsWhitelisted(guildId, user) || db.IsBlacklisted(guildId, user)))
            {
                ulong id = ((ITextChannel)msg.Channel).Guild.Id;
                if (db.IsErrorOrMore(db.GetVerbosity(id)))
                    await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + Sentences.WhitelistError(id));
            }
            else if (pin && msg.IsPinned)
            {
                if (db.GetVerbosity(msg.Channel as ITextChannel == null ? (ulong?)null : ((ITextChannel)msg.Channel).Guild.Id) == Db.Verbosity.Info)
                    await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + Sentences.AlreadyPinned(guildId));
            }
            else
            {
                try
                {
                    if (pin)
                        await ((IUserMessage)msg).PinAsync();
                    else
                        await ((IUserMessage)msg).UnpinAsync();
                }
                catch (HttpException http)
                {
                    if (db.IsErrorOrMore(db.GetVerbosity(isNotInGuild ? null : ((ITextChannel)msg.Channel).Guild.Id)))
                    {
                        if (http.HttpCode == HttpStatusCode.Forbidden)
                            await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + Sentences.MissingPermission(guildId));
                        else if (http.HttpCode == HttpStatusCode.BadRequest)
                            await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + Sentences.TooManyPins(guildId));
                        else
                            throw;
                    }
                }
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel _, SocketReaction react)
        {
            if (react.Emote.Name == "📌" || react.Emote.Name == "📍")
            {
                await PinMessageAsync(await msg.GetOrDownloadAsync(), react.User.IsSpecified ? react.User.Value : null, react.Channel as ITextChannel == null ? null : ((ITextChannel)react.Channel).Guild.Id, true, true);
                await Utils.WebsiteUpdate("Pina", statsWebsite, statsToken, "nbMsgs", "1");
            }
            else if (react.Emote.Name == "⛔" || react.Emote.Name == "🚫")
            {
                await PinMessageAsync(await msg.GetOrDownloadAsync(), react.User.IsSpecified ? react.User.Value : null, react.Channel as ITextChannel == null ? null : ((ITextChannel)react.Channel).Guild.Id, true, false);
                await Utils.WebsiteUpdate("Pina", statsWebsite, statsToken, "nbMsgs", "1");
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage msg = arg as SocketUserMessage;
            if (msg == null || (arg.Author.IsBot && !db.IsCanBotInteract(msg.Channel is ITextChannel textChan ? textChan.GuildId : null))) return;
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
