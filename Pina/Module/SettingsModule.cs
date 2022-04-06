using Discord;
using Pina.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pina.Module
{
    public class SettingsModule
    {
        public static bool CanModify(IUser user, ulong ownerId)
        {
            if (user == null) // Not supposed to happen but still do
            {
                return false;
            }
            if (user.Id == ownerId)
            {
                return true;
            }
            IGuildUser guildUser = (IGuildUser)user;
            return guildUser.GuildPermissions.ManageGuild;
        }

        public async Task PrefixAsync(ICommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                await ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync("You don't have the permission to do this command.");
            }
            else if (args.Length == 0)
            {
                await Program.P.GetDb().SetPrefix(Context.Guild.Id, "");
                await ReplyAsync("Your prefix was unset");
            }
            else
            {
                string prefix = args[0];
                await Program.P.GetDb().SetPrefix(Context.Guild.Id, prefix);
                await ReplyAsync($"Your prefix was set to {prefix}.");
            }
        }

        public async Task VerbosityAsync(ICommandContext ctx)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync("You don't have the permission to do this command.");
            }
            else if (args.Length == 0)
            {
                await ReplyAsync("You must provide a verbosity between none, error and info");
            }
            else
            {
                string verbosity = args[0].ToLower();
                if (verbosity != "none" && verbosity != "error" && verbosity != "info")
                    await ReplyAsync("The selected verbosity must be none, error or info.");
                else
                {
                    await Program.P.GetDb().SetVerbosity(Context.Guild.Id, verbosity);
                    await ReplyAsync($"Your verbosity was set to {verbosity}.");
                }
            }
        }

        public async Task WhitelistAsync(params string[] args)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync("You don't have the permission to do this command.");
            }
            else if (args.Length == 0)
            {
                await Program.P.GetDb().SetWhitelist(Context.Guild.Id, "0");
                await ReplyAsync("Your whitelist was removed.");
            }
            else
            {
                List<IRole> roles = new();
                foreach (string arg in args)
                {
                    IRole role = Utils.GetRole(arg, Context.Guild);
                    if (role == null)
                    {
                        await ReplyAsync($"I didn't find any matching role for {arg}.");
                        return;
                    }
                    roles.Add(role);
                }
                await Program.P.GetDb().SetWhitelist(Context.Guild.Id, string.Join("|", roles.Select(x => x.Id)));
                await ReplyAsync($"Your whitelist was set to the following roles:\n{string.Join(", ", roles.Select(x => x.Name.Replace("@everyone", "@ everyone")))}");
            }
        }

        public async Task BlacklistAsync(ICommandContext ctx)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync("You don't have the permission to do this command.");
            }
            else if (args.Length == 0)
            {
                await Program.P.GetDb().SetBlacklist(Context.Guild.Id, "0");
                await ReplyAsync("Your blacklist was removed.");
            }
            else
            {
                List<IGuildUser> ids = new();
                foreach (string arg in args)
                {
                    IGuildUser user = await Utils.GetUserAsync(arg, Context.Guild);
                    if (user == null)
                    {
                        await ReplyAsync($"I didn't find any matching user for {arg}.");
                        return;
                    }
                    ids.Add(user);
                }
                await Program.P.GetDb().SetBlacklist(Context.Guild.Id, string.Join("|", ids.Select(x => x.Id)));
                await ReplyAsync($"Your blacklist was set to the following users:\n{string.Join(", ", ids.Select(x => x.ToString()))}");
            }
        }

        public async Task BotInteractAsync(ICommandContext ctx)
        {
            args = args?.ToLowerInvariant();
            if (Context.Guild == null)
            {
                await ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync("You don't have the permission to do this command.");
            }
            else if (args != "true" && args != "false")
            {
                await ReplyAsync("You must provide 'true' or 'false' as an argument.");
            }
            else
            {
                await Program.P.GetDb().SetCanBotInteract(Context.Guild.Id, args == "true");
                await ReplyAsync("Your preferences were updated");
            }
        }

        public async Task VoteRequiredAsync(ICommandContext ctx)
        {
            args = args?.ToLower();
            if (Context.Guild == null)
            {
                await ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync("You don't have the permission to do this command.");
            }
            int value;
            if (!int.TryParse(args, out value) || value < 0)
            {
                await ReplyAsync("You must provide a positive number as argument.");
            }
            else
            {
                await Program.P.GetDb().SetBotVotesRequired(Context.Guild.Id, value);
                await ReplyAsync("Your preferences were updated");
            }
        }

        public async Task CanUnpinAsync(ICommandContext ctx)
        {
            args = args?.ToLower();
            if (Context.Guild == null)
            {
                await ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync("You don't have the permission to do this command.");
            }
            else if (args != "true" && args != "false")
            {
                await ReplyAsync("You must provide 'true' or 'false' as an argument.");
            }
            else
            {
                await Program.P.GetDb().SetCanUnpin(Context.Guild.Id, args == "true");
                await ReplyAsync("Your preferences were updated");
            }
        }
    }
}
