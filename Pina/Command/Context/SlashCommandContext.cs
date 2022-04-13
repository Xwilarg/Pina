using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Pina.Command.Context
{
    public class SlashCommandContext : ICommandContext
    {
        public SlashCommandContext(SocketSlashCommand ctx)
        {
            _ctx = ctx;
        }

        private SocketSlashCommand _ctx;

        public SocketGuild Guild => Channel?.Guild as SocketGuild;

        public ITextChannel Channel => _ctx.Channel as ITextChannel;

        public IUser User => _ctx.User;

        public async Task ReplyAsync(string message = "", Embed embed = null, bool ephemeral = false)
        {
            if (_ctx.HasResponded)
            {
                await _ctx.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = message;
                    x.Embed = embed;
                });
            }
            else
            {
                await _ctx.RespondAsync(message, embed: embed);
            }
        }

        public T GetArgument<T>(string key)
        {
            return (T?)(_ctx.Data.Options.FirstOrDefault(x => x.Name == key)?.Value ?? default);
        }
    }
}
