using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Pina
{
    class CommunicationModule : ModuleBase
    {

        [Command("Info")]
        private async Task Info()
        {
            await ReplyAsync(embed: new EmbedBuilder()
            {
                Color = Color.Purple,
                Fields = new()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Uptime",
                        Value = Utils.TimeSpanToString(DateTime.Now.Subtract(Program.P.StartTime)),
                        IsInline = true
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Creator",
                        Value = "Zirk#0001",
                        IsInline = true
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Account creation",
                        Value = Program.P.client.CurrentUser.CreatedAt.ToString("HH:mm:ss dd/MM/yy"),
                        IsInline = true
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Last version",
                        Value = new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString("HH:mm:ss dd/MM/yy"),
                        IsInline = true
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "GitHub",
                        Value = "https://github.com/Xwilarg/Pina",
                        IsInline = true
                    }
                }
            }.Build());
        }

        [Command("Help")]
        private async Task Help(params string[] _)
        {
            await ReplyAsync("", false, new EmbedBuilder()
            {
                Color = Color.Purple,
                Title = "Help",
                Description =
                    "I'm here to simplify the process of pinning messages." + Environment.NewLine +
                    "Please make sure that I have the 'Manage Messages permission (I need it to pin them)." + Environment.NewLine +
                    "You have 2 ways to do it:\nAdding a 📌 reaction to it\nDo the 'Pin' command, optionally followed by the ID of the message." + Environment.NewLine +
                    "You can also unpin messages by using the 'Unpin' command followed by the ID of the message, or by adding ⛔ to it." +
                    (Context.Guild == null ? "" : Environment.NewLine + Environment.NewLine +
                        "You can also change my default behaviour with these commands:" + Environment.NewLine +
                        "**Language [language name]**: Set my speaking language" + Environment.NewLine +
                        "**Verbosity [none/error/info]**: Set if I say something or not when something occur" + Environment.NewLine +
                        "**Whitelist [(optional)roles]**: Set the roles that can pin messages, don't write anything for all, admins are not affected by this" + Environment.NewLine +
                        "**Blacklist [(optional)users]**: Set the users that can't pin messages, don't write anything for none, admins are not affected by this" + Environment.NewLine +
                        "**Prefix [(optional)prefix]**: Set the prefix for bot command, don't write anything to allow the use of command without one" + Environment.NewLine +
                        "**BotInteract [true/false]**: Set if other bots are allowed to do commands" + Environment.NewLine +
                        "**CanUnpin [true/false]**: Set if user can unpin messages" + Environment.NewLine +
                        "**VoteRequired [number of votes required]**: Set the number of people that need to vote to pin/unpin a message, set to 1 to disable") +
                    Environment.NewLine + Environment.NewLine +
                    "You can also use these command:" + Environment.NewLine +
                    "**Gdpr**: Display the information I have about this guild" + Environment.NewLine +
                    "**Info**: Display various information about me" + Environment.NewLine +
                    "**Invite**: Display the invite link of the bot" + Environment.NewLine
            }.Build());
        }

        [Command("Invite")]
        private async Task Invite(params string[] _)
        {
            await ReplyAsync("<https://discordapp.com/api/oauth2/authorize?client_id=583314556848308261&permissions=10240&scope=bot>");
        }

        [Command("GDPR"), Summary("Show infos the bot have about the user and the guild")]
        public async Task GDPR(params string[] _)
        {
            if (Context.Guild == null)
                await ReplyAsync("I only save data about guilds.");
            else
                await ReplyAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Blue,
                    Title = $"Data saved about {Context.Guild.Name}:",
                    Description = await Program.P.GetDb().GetGuildAsync(Context.Guild.Id)
                }.Build());
        }
    }
}
