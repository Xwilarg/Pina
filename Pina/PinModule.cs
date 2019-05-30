using Discord;
using Discord.Commands;
using DiscordUtils;
using System.Linq;
using System.Threading.Tasks;

namespace Pina
{
    class PinModule : ModuleBase
    {
        [Command("Pin", RunMode = RunMode.Async)]
        private async Task Info(params string[] args)
        {
            if (args.Length == 0)
            {
                var msgs = (await Context.Channel.GetMessagesAsync(2).FlattenAsync()).ToArray();
                if (msgs.Length < 2)
                    await ReplyAsync("There is nothing to ping.");
                else
                    await Program.P.PinMessageAsync(msgs[1]);
            }
            else
            {
                IMessage msg = await Utils.GetMessage(args[0], Context.Channel);
                if (msg == null)
                    await ReplyAsync("I didn't found any message with this id");
                else
                    await Program.P.PinMessageAsync(msg);
            }
        }
    }
}
