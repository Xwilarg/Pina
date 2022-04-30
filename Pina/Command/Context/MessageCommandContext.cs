using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pina.Command.Context
{
    public class MessageCommandContext : ICommandContext
    {
        private string _arg;
        private IMessage _message;

        public MessageCommandContext(IMessage message, string arg)
        {
            _arg = arg;
            _message = message;
        }

        public SocketGuild Guild => Channel?.Guild as SocketGuild;

        public ITextChannel Channel => _message.Channel as ITextChannel;

        public IUser User => _message.Author;

        public async Task ReplyAsync(string message = "", Embed embed = null, bool ephemeral = false)
        {
            if (ephemeral)
            {
                await Channel.SendMessageAsync(message, embed: embed);
            }
        }

        public T GetArgument<T>(string key)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)_arg;
            }
            if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(_arg);
            }
            throw new NotImplementedException();
        }
    }
}
