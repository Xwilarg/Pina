using Discord;
using Pina.Command.Context;
using System.Linq;
using System.Threading.Tasks;

namespace Pina.Module
{
    public static class PinModule
    {
        public static async Task UnpinAsync(ICommandContext ctx)
        {
            var arg = ctx.GetArgument<string>("id");
            if (!ulong.TryParse(arg, out var id))
            {
                await ctx.ReplyAsync("Parameter must be a valid ID", ephemeral: true);
            }
            else
            {
                IMessage msg = (await ctx.Channel.GetPinnedMessagesAsync()).FirstOrDefault(x => x.Id == id);
                if (msg == null)
                    await ctx.ReplyAsync("I didn't find any message with this id.", ephemeral: true);
                else
                {
                    if (await Program.P.PinMessageAsync(msg, ctx.User, ctx.Guild?.Id, false, false))
                    {
                        await ctx.ReplyAsync("Your message was unpinned", ephemeral: true);
                    }
                }
            }
        }

        public static async Task PinAsync(ICommandContext ctx)
        {
            var arg = ctx.GetArgument<string>("id");
            ulong? id = null;
            if (ulong.TryParse(arg, out var argId))
            {
                id = argId;
            }
            if (id == null)
            {
                var msg = await GetLastMessage(ctx.Channel);
                if (msg == null)
                {
                    if (Program.P.GetDb().IsErrorOrMore(Program.P.GetDb().GetVerbosity(ctx.Guild?.Id)))
                        await ctx.ReplyAsync("There is nothing to pin.", ephemeral: true);
                }
                else
                {
                    if (await Program.P.PinMessageAsync(msg, ctx.User, ctx.Guild?.Id, false, true))
                    {
                        await ctx.ReplyAsync("Your message was pinned", ephemeral: true);
                    }
                }
            }
            else
            {
                IMessage msg = await Utils.GetMessageAsync(id.Value, ctx.Channel);
                if (msg == null)
                {
                    if (Program.P.GetDb().IsErrorOrMore(Program.P.GetDb().GetVerbosity(ctx.Guild?.Id)))
                        await ctx.ReplyAsync("I didn't find any message with this id.", ephemeral: true);
                }
                else
                {
                    if (await Program.P.PinMessageAsync(msg, ctx.User, ctx.Guild?.Id, false, true))
                    {
                        await ctx.ReplyAsync("Your message was pinned", ephemeral: true);
                    }
                }
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
