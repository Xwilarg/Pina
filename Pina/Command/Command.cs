using Discord;
using System;
using System.Threading.Tasks;

namespace Pina.Command
{
    public class Command
    {
        public Func<ICommandContext, CommandArgs, Task> Callback;
        public SlashCommandProperties SlashCommand;
    }
}
