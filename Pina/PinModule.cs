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
                var msg = await GetLastMessage();
                if (msg == null)
                    await ReplyAsync("There is nothing to ping.");
                else
                    await Program.P.PinMessageAsync(msg);
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

        /// Sometimes the last message isn't pingable (like it can be the Discord message that say that a message was pinged) so we get the previous one
        private async Task<IMessage> GetLastMessage(int getCount = 2)
        {
            var msgs = (await Context.Channel.GetMessagesAsync(getCount).FlattenAsync()).ToArray();
            if (msgs.Length != getCount)
                return null;
            if (msgs.Last() as IUserMessage != null)
                return msgs.Last();
            return await GetLastMessage(getCount + 1);
        }
    }
}
