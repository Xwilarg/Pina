using Discord;
using Pina.Command;
using Pina.Command.Context;
using System.Linq;
using System.Threading.Tasks;

namespace Pina.Module
{
    public static class PinModule
    {
        public static async Task UnpinAsync(ICommandContext ctx)
        {
            IMessage msg = (await ctx.Channel.GetPinnedMessagesAsync()).FirstOrDefault(x => x.Id == ctx.GetArgument<ulong>("id"));
            if (msg == null)
                await ctx.ReplyAsync("I didn't find any message with this id.");
            else
                await Program.P.PinMessageAsync(msg, ctx.User, ctx.Guild?.Id, false, false);
        }

        public static async Task PinAsync(ICommandContext ctx)
        {
            var id = ctx.GetArgument<ulong?>("id");
            if (id == null)
            {
                var msg = await GetLastMessage(ctx.Channel);
                if (msg == null)
                {
                    if (Program.P.GetDb().IsErrorOrMore(Program.P.GetDb().GetVerbosity(ctx.Guild?.Id)))
                        await ctx.ReplyAsync("There is nothing to pin.");
                }
                else
                {
                    await Program.P.PinMessageAsync(msg, ctx.User, ctx.Guild?.Id, false, true);
                }
            }
            else
            {
                IMessage msg = await Utils.GetMessageAsync(id.Value, ctx.Channel);
                if (msg == null)
                {
                    if (Program.P.GetDb().IsErrorOrMore(Program.P.GetDb().GetVerbosity(ctx.Guild?.Id)))
                        await ctx.ReplyAsync("I didn't find any message with this id.");
                }
                else
                    await Program.P.PinMessageAsync(msg, ctx.User, ctx.Guild?.Id, false, true);
            }
        }

        /// Sometimes the last message isn't pingable (like it can be the Discord message that say that a message was pinged) so we get the previous one
        private static async Task<IMessage> GetLastMessage(ITextChannel chan, int getCount = 2)
        {
            var msgs = (await chan.GetMessagesAsync(getCount).FlattenAsync()).ToArray();
            if (msgs.Length != getCount)
                return null;
            if (msgs.Last() as IUserMessage != null)
                return msgs.Last();
            return await GetLastMessage(chan, getCount + 1);
        }
    }
}
