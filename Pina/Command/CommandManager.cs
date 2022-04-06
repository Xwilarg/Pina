using Discord;
using Discord.WebSocket;
using Pina.Module;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pina.Command
{
    public class CommandManager
    {
        public CommandManager(ulong? debugGuildId)
        {
            _debugGuildId = debugGuildId;
            _commands = new()
            {
                { "info", new()
                    {
                        Callback = CommunicationModule.InfoAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "info",
                            Description = "Get information about the bot"
                        }.Build()
                    }
                },
                {
                    "help",
                    new()
                    {
                        Callback = CommunicationModule.HelpAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "help",
                            Description = "Get the help"
                        }.Build()
                    }
                },
                {
                    "invite",
                    new()
                    {
                        Callback = CommunicationModule.HelpAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "help",
                            Description = "Get the invite link of the bot"
                        }.Build()
                    }
                },
                {
                    "gdpr",
                    new()
                    {
                        Callback = CommunicationModule.HelpAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "gdpr",
                            Description = "Get the invite link of the bot"
                        }.Build()
                    }
                }
            };
        }

        private Dictionary<string, Command> _commands;

        public IEnumerable<Command> LoadCommands()
        {
            AreCommandsLoaded = true;
            return _commands.Values;
        }

        public bool AreCommandsLoaded { get; private set; }

        public SocketGuild GetDebugGuild(DiscordSocketClient client)
        {
            if (_debugGuildId.HasValue && Debugger.IsAttached)
            {
                return client.GetGuild(_debugGuildId.Value);
            }
            return null;
        }

        private ulong? _debugGuildId;
    }
}
