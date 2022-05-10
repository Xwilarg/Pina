using Discord;
using Pina.Command.Context;
using System;
using System.Threading.Tasks;

namespace Pina.Command
{
    public class Command
    {
        public Func<ICommandContext, Task> Callback;
        public SlashCommandProperties SlashCommand;
    }
}
