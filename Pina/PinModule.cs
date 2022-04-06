using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Pina
{
    class PinModule : ModuleBase
    {
        [Command("Unpin", RunMode = RunMode.Async)]
        private async Task Unpin(params string[] args)
        {
            if (args.Length == 0)
                await ReplyAsync("You must give the ID of the message you want to unpin.");
            else
            {
                ulong id;
                if (!ulong.TryParse(string.Join("", args), out id))
                    await ReplyAsync("I didn't find any message with this id.");
                else
                {
                    IMessage msg = (await Context.Channel.GetPinnedMessagesAsync()).FirstOrDefault(x => x.Id == id);
                    if (msg == null)
                        await ReplyAsync("I didn't find any message with this id.");
                    else
                        await Program.P.PinMessageAsync(msg, Context.User, Context.Guild?.Id, false, false);
                }
            }
        }

        private static async Task<IMessage> GetMessageAsync(string id, IMessageChannel chan)
        {
            if (!ulong.TryParse(id, out ulong uid))
                return null;
            IMessage msg;
            if (uid != 0)
            {
                msg = await chan.GetMessageAsync(uid);
                if (msg != null)
                    return msg;
            }
            if (chan is not ITextChannel textChan || uid == 0)
                return null;
            foreach (ITextChannel c in await textChan.Guild.GetTextChannelsAsync())
            {
                try
                {
                    msg = await c.GetMessageAsync(uid);
                    if (msg != null)
                        return msg;
                }
                catch (Discord.Net.HttpException)
                { }
            }
            return null;
        }

        [Command("Pin", RunMode = RunMode.Async)]
        private async Task Pin(params string[] args)
        {
            if (args.Length == 0)
            {
                var msg = await GetLastMessage();
                if (msg == null)
                {
                    if (Program.P.GetDb().IsErrorOrMore(Program.P.GetDb().GetVerbosity(Context.Guild?.Id)))
                        await ReplyAsync("There is nothing to pin.");
                }
                else
                {
                    await Program.P.PinMessageAsync(msg, Context.User, Context.Guild?.Id, false, true);
                }
            }
            else
            {
                IMessage msg = await GetMessageAsync(args[0], Context.Channel);
                if (msg == null)
                {
                    if (Program.P.GetDb().IsErrorOrMore(Program.P.GetDb().GetVerbosity(Context.Guild?.Id)))
                        await ReplyAsync("I didn't find any message with this id.");
                }
                else
                    await Program.P.PinMessageAsync(msg, Context.User, Context.Guild?.Id, false, true);
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
