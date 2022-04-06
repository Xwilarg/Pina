using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Pina.Command
{
    public interface ICommandContext
    {
        public Task ReplyAsync(string message = "", Embed embed = null);
        public SocketGuild Guild { get; }
        public T GetArgument<T>();
    }
}
