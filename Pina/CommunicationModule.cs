using Discord;
using Discord.Commands;
using DiscordUtils;
using System;
using System.Threading.Tasks;

namespace Pina
{
    class CommunicationModule : ModuleBase
    {
        [Command("Info")]
        private async Task Info()
        {
            await ReplyAsync("", false, Utils.GetBotInfo(Program.P.StartTime, "Pina", Program.P.client.CurrentUser));
        }

        [Command("Help")]
        private async Task Help(params string[] _)
        {
            await ReplyAsync("", false, new EmbedBuilder()
            {
                Color = Color.Purple,
                Title = "Help",
                Description =
                    Sentences.HelpIntro(Context.Guild?.Id) + Environment.NewLine +
                    Sentences.HelpPerm(Context.Guild?.Id) + Environment.NewLine +
                    Sentences.HelpPin(Context.Guild?.Id) + Environment.NewLine +
                    Sentences.HelpUnpin(Context.Guild?.Id) +
                    (Context.Guild == null ? "" : Environment.NewLine + Environment.NewLine +
                        Sentences.HelpSettings(Context.Guild.Id) + Environment.NewLine +
                        Sentences.HelpLanguage(Context.Guild.Id) + Environment.NewLine +
                        Sentences.HelpVerbosity(Context.Guild.Id) + Environment.NewLine +
                        Sentences.HelpWhitelist(Context.Guild.Id) + Environment.NewLine +
                        Sentences.HelpPrefix(Context.Guild.Id)) +
                    Environment.NewLine + Environment.NewLine +
                    Sentences.HelpCommunication(Context.Guild?.Id) + Environment.NewLine +
                    Sentences.HelpGdpr(Context.Guild?.Id) + Environment.NewLine +
                    Sentences.HelpInfo(Context.Guild?.Id) + Environment.NewLine +
                    Sentences.HelpInvite(Context.Guild?.Id) + Environment.NewLine
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
                await ReplyAsync(Sentences.GdprPm(null));
            else
                await ReplyAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Blue,
                    Title = Sentences.DataSavedAbout(Context.Guild.Id, Context.Guild.Name),
                    Description = await Program.P.GetDb().GetGuildAsync(Context.Guild.Id)
                }.Build());
        }
    }
}
