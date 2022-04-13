using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Pina.Command.Context
{
    public interface ICommandContext
    {
        public Task ReplyAsync(string message = "", Embed embed = null, bool ephemeral = false);
        public SocketGuild Guild { get; }
        public ITextChannel Channel { get; }
        public IUser User { get; }
        public T GetArgument<T>(string key);
    }
}
