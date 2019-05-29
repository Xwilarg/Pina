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
        private async Task Help()
        {
            await ReplyAsync("", false, new EmbedBuilder()
            {
                Color = Color.Purple,
                Title = "Help",
                Description =
                    "I'm here to simplify the process of pinning messages." + Environment.NewLine +
                    "Please make sure that I have the 'Manage Messages' permission (I need it to pin messages)" + Environment.NewLine +
                    "Then you have 2 ways to pin a message:" + Environment.NewLine +
                    "Add a 📌 reaction to the message you want to pin" + Environment.NewLine +
                    "Do the 'Pin' command followed by the ID of the message you want to pin"
            }.Build());
        }
    }
}
