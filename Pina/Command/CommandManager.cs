using Discord;
using Discord.WebSocket;
using Pina.Command.Context;
using Pina.Module;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pina.Command
{
    public class CommandManager
    {
        public CommandManager(ulong? debugGuildId)
        {
            _debugGuildId = debugGuildId;
            _commands = new()
            {
                #region Communication Module
                {
                    "info", new()
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
                        Callback = CommunicationModule.InviteAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "invite",
                            Description = "Get the invite link of the bot"
                        }.Build()
                    }
                },
                {
                    "gdpr",
                    new()
                    {
                        Callback = CommunicationModule.GdprAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "gdpr",
                            Description = "Get the invite link of the bot"
                        }.Build()
                    }
                },
                #endregion
                #region Pin Module
                {
                    "unpin",
                    new()
                    {
                        Callback = PinModule.UnpinAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "unpin",
                            Description = "Unpin a message",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "id",
                                    Description = "ID of the message",
                                    Type = ApplicationCommandOptionType.String,
                                    IsRequired = true
                                }
                            }
                        }.Build()
                    }
                },
                {
                    "pin",
                    new()
                    {
                        Callback = PinModule.PinAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "pin",
                            Description = "Pin a message",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "id",
                                    Description = "ID of the message",
                                    Type = ApplicationCommandOptionType.String,
                                    IsRequired = false
                                }
                            }
                        }.Build()
                    }
                },
                #endregion
                #region Settings Module
                {
                    "verbosity",
                    new()
                    {
                        Callback = SettingsModule.VerbosityAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "verbosity",
                            Description = "Set the verbosity level of the bot",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "verbosity",
                                    Description = "\"none\", \"info\" or \"error\"",
                                    Type = ApplicationCommandOptionType.String,
                                    IsRequired = false
                                }
                            }
                        }.Build()
                    }
                },
                {
                    "whitelist",
                    new()
                    {
                        Callback = SettingsModule.WhitelistAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "whitelist",
                            Description = "Set who is allowed to (un)pin messages",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "whitelist",
                                    Description = "List of roles, separated by an empty space",
                                    Type = ApplicationCommandOptionType.String,
                                    IsRequired = false
                                }
                            }
                        }.Build()
                    }
                },
                {
                    "blacklist",
                    new()
                    {
                        Callback = SettingsModule.BlacklistAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "blacklist",
                            Description = "Set who is not allowed to (un)pin messages",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "blacklist",
                                    Description = "List of roles, separated by an empty space",
                                    Type = ApplicationCommandOptionType.String,
                                    IsRequired = false
                                }
                            }
                        }.Build()
                    }
                },
                {
                    "botinteract",
                    new()
                    {
                        Callback = SettingsModule.BotInteractAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "botinteract",
                            Description = "Set if others bots are allowed to do commands",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "caninteract",
                                    Description = "\"true\" or \"false\"",
                                    Type = ApplicationCommandOptionType.String,
                                    IsRequired = false
                                }
                            }
                        }.Build()
                    }
                },
                {
                    "voterequired",
                    new()
                    {
                        Callback = SettingsModule.VoteRequiredAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "voterequired",
                            Description = "Set the number of votes required for a message to be pinned",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "nbvotes",
                                    Description = "Number of votes required or 0 to disable",
                                    Type = ApplicationCommandOptionType.Integer,
                                    IsRequired = true
                                }
                            }
                        }.Build()
                    }
                },
                {
                    "canunpin",
                    new()
                    {
                        Callback = SettingsModule.CanUnpinAsync,
                        SlashCommand = new SlashCommandBuilder()
                        {
                            Name = "canunpin",
                            Description = "Are users allowed to unpin messages",
                            Options = new()
                            {
                                new SlashCommandOptionBuilder()
                                {
                                    Name = "canunpin",
                                    Description = "Number of votes required or 0 to disable",
                                    Type = ApplicationCommandOptionType.Integer,
                                    IsRequired = true
                                }
                            }
                        }.Build()
                    }
                }
                #endregion
            };
        }

        private Dictionary<string, Command> _commands;

        public IEnumerable<Command> LoadCommands()
        {
            AreCommandsLoaded = true;
            return _commands.Values;
        }

        public async Task InvokeCommandAsync(string key, ICommandContext ctx)
        {
            await _commands[key].Callback(ctx);
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
