using Discord;
using Discord.Commands;
using DiscordUtils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pina
{
    public class SettingsModule : ModuleBase
    {
        public static bool CanModify(IUser user, ulong ownerId)
        {
            if (user.Id == ownerId)
                return true;
            IGuildUser guildUser = (IGuildUser)user;
            return guildUser.GuildPermissions.ManageGuild;
        }

        [Command("Prefix")]
        private async Task Prefix(params string[] args)
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

        [Command("Verbosity")]
        private async Task Verbosity(params string[] args)
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

        [Command("Whitelist")]
        private async Task Whitelist(params string[] args)
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
                List<IRole> roles = new List<IRole>();
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

        [Command("Blacklist")]
        private async Task Blacklist(params string[] args)
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
                List<IGuildUser> ids = new List<IGuildUser>();
                foreach (string arg in args)
                {
                    IGuildUser user = await Utils.GetUser(arg, Context.Guild);
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

        [Command("BotInteract"), Alias("BotInteract")]
        private async Task BotInteract([Remainder]string args)
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
                await Program.P.GetDb().SetCanBotInteract(Context.Guild.Id, args == "true");
                await ReplyAsync("Your preferences were updated");
            }
        }

        [Command("VoteRequired"), Alias("VotesRequired")]
        private async Task VoteRequired([Remainder] string args)
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
    }
}
