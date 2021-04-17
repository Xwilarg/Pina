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
using System.Linq;
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

        /// <summary>
        /// Dictionary associating message ID:
        /// The message itself, nb of votes required
        /// </summary>
        public Dictionary<ulong, Tuple<IMessage, int, List<ulong>, bool, IUserMessage>> PinAwaiting = new();

        public async Task PinMessageAsync(IMessage msg, IUser user, ulong? guildId, bool isFromEmote, bool pin)
        {
            bool isNotInGuild = msg.Channel as ITextChannel == null;
            if (!isNotInGuild && (!db.IsWhitelisted(guildId, user) || db.IsBlacklisted(guildId, user)))
            {
                ulong id = ((ITextChannel)msg.Channel).Guild.Id;
                if (db.IsErrorOrMore(db.GetVerbosity(id)))
                    await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + "You aren't allowed to pin/unpin messages.");
            }
            else if (pin && msg.IsPinned)
            {
                if (db.GetVerbosity(msg.Channel as ITextChannel == null ? (ulong?)null : ((ITextChannel)msg.Channel).Guild.Id) == Db.Verbosity.Info)
                    await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + "This message was already pinned.");
            }
            else
            {
                try
                {
                    var nbVotes = db.GetVotesRequired(guildId);
                    if (nbVotes <= 1)
                    {
                        if (pin)
                            await ((IUserMessage)msg).PinAsync();
                        else
                            await ((IUserMessage)msg).UnpinAsync();
                    }
                    else if (PinAwaiting.Values.ToArray().Any(x => x.Item1.Id == msg.Id))
                    {
                        var elem = PinAwaiting.ToArray().First(x => x.Value.Item1.Id == msg.Id);
                        if (!elem.Value.Item3.Contains(user.Id))
                        {
                            if (elem.Value.Item3.Count + 1 >= elem.Value.Item2)
                            {
                                if (pin)
                                    await ((IUserMessage)msg).PinAsync();
                                else
                                    await ((IUserMessage)msg).UnpinAsync();
                                PinAwaiting.Remove(elem.Key);
                                await elem.Value.Item5.DeleteAsync();
                            }
                            else
                            {
                                PinAwaiting[elem.Key].Item3.Add(user.Id);
                                await elem.Value.Item5.ModifyAsync(x => x.Embed = new EmbedBuilder
                                {
                                    Title = x.Embed.Value.Title,
                                    Description = x.Embed.Value.Description,
                                    Color = x.Embed.Value.Color,
                                    Footer = new EmbedFooterBuilder
                                    {
                                        Text = "Current number of votes: " + elem.Value.Item3.Count
                                    }
                                }.Build());
                            }
                        }
                    }
                    else
                    {
                        var n = await msg.Channel.SendMessageAsync(embed: new EmbedBuilder
                        {
                            Title = "Vote",
                            Description = $"A vote was started to {(pin ? "pin" : "unpin")} the message {msg.GetJumpUrl()}\n{nbVotes} required",
                            Color = Color.Blue,
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "Current number of votes: 1"
                            }
                        }.Build());
                        await n.AddReactionAsync(new Emoji("✅"));
                        PinAwaiting.Add(n.Id, new Tuple<IMessage, int, List<ulong>, bool, IUserMessage>(msg, nbVotes, new List<ulong> { user.Id }, pin, n));
                    }
                }
                catch (HttpException http)
                {
                    if (db.IsErrorOrMore(db.GetVerbosity(isNotInGuild ? null : ((ITextChannel)msg.Channel).Guild.Id)))
                    {
                        if (http.HttpCode == HttpStatusCode.Forbidden)
                            await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + "I wasn't able to pin the message, please make sure that I have the 'Manage Messages' permission.");
                        else if (http.HttpCode == HttpStatusCode.BadRequest)
                            await msg.Channel.SendMessageAsync((isFromEmote ? user.Mention + " " : "") + "You reached the pin limit for this channel.");
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
            else if (react.Emote.Name == "✅" && react.UserId != client.CurrentUser.Id && PinAwaiting.ContainsKey(msg.Id))
            {
                var value = PinAwaiting[msg.Id];
                if (!value.Item3.Contains(react.UserId))
                {
                    if (value.Item3.Count + 1 >= value.Item2)
                    {
                        if (value.Item4)
                            await ((IUserMessage)value.Item1).PinAsync();
                        else
                            await ((IUserMessage)value.Item1).UnpinAsync();
                        PinAwaiting.Remove(msg.Id);
                        await value.Item5.DeleteAsync();
                    }
                    else
                    {
                        PinAwaiting[msg.Id].Item3.Add(react.UserId);
                        await value.Item5.ModifyAsync(x => x.Embed = new EmbedBuilder
                        {
                            Title = x.Embed.Value.Title,
                            Description = x.Embed.Value.Description,
                            Color = x.Embed.Value.Color,
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "Current number of votes: " + value.Item3.Count
                            }
                        }.Build());
                    }
                }
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
