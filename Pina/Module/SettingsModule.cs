﻿using Discord;
using Pina.Command;
using Pina.Command.Context;
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

        public static async Task VerbosityAsync(ICommandContext ctx)
        {
            var verb = ctx.GetArgument<string>("verbosity");
            if (ctx.Guild == null)
            {
                await ctx.ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(ctx.User, ctx.Guild.OwnerId))
            {
                await ctx.ReplyAsync("You don't have the permission to do this command.");
            }
            else if (verb == null)
            {
                await ctx.ReplyAsync("You must provide a verbosity between none, error and info");
            }
            else
            {
                string verbosity = verb.ToLowerInvariant();
                if (verbosity != "none" && verbosity != "error" && verbosity != "info")
                    await ctx.ReplyAsync("The selected verbosity must be none, error or info.");
                else
                {
                    await Program.P.GetDb().SetVerbosity(ctx.Guild.Id, verbosity);
                    await ctx.ReplyAsync($"Your verbosity was set to {verbosity}.");
                }
            }
        }

        public static async Task WhitelistAsync(ICommandContext ctx)
        {
            var whitelist = ctx.GetArgument<string>("whitelist");
            if (ctx.Guild == null)
            {
                await ctx.ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(ctx.User, ctx.Guild.OwnerId))
            {
                await ctx.ReplyAsync("You don't have the permission to do this command.");
            }
            else if (whitelist == null)
            {
                await Program.P.GetDb().SetWhitelist(ctx.Guild.Id, "0");
                await ctx.ReplyAsync("Your whitelist was removed.");
            }
            else
            {
                List<IRole> roles = new();
                foreach (string arg in whitelist.Split(' '))
                {
                    IRole role = Utils.GetRole(arg, ctx.Guild);
                    if (role == null)
                    {
                        await ctx.ReplyAsync($"I didn't find any matching role for {arg}.");
                        return;
                    }
                    roles.Add(role);
                }
                await Program.P.GetDb().SetWhitelist(ctx.Guild.Id, string.Join("|", roles.Select(x => x.Id)));
                await ctx.ReplyAsync($"Your whitelist was set to the following roles:\n{string.Join(", ", roles.Select(x => x.Name.Replace("@everyone", "@ everyone")))}");
            }
        }

        public static async Task BlacklistAsync(ICommandContext ctx)
        {
            var blacklist = ctx.GetArgument<string>("blacklist");
            if (ctx.Guild == null)
            {
                await ctx.ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(ctx.User, ctx.Guild.OwnerId))
            {
                await ctx.ReplyAsync("You don't have the permission to do this command.");
            }
            else if (blacklist != null)
            {
                await Program.P.GetDb().SetBlacklist(ctx.Guild.Id, "0");
                await ctx.ReplyAsync("Your blacklist was removed.");
            }
            else
            {
                List<IGuildUser> ids = new();
                foreach (string arg in blacklist.Split(' '))
                {
                    IGuildUser user = await Utils.GetUserAsync(arg, ctx.Guild);
                    if (user == null)
                    {
                        await ctx.ReplyAsync($"I didn't find any matching user for {arg}.");
                        return;
                    }
                    ids.Add(user);
                }
                await Program.P.GetDb().SetBlacklist(ctx.Guild.Id, string.Join("|", ids.Select(x => x.Id)));
                await ctx.ReplyAsync($"Your blacklist was set to the following users:\n{string.Join(", ", ids.Select(x => x.ToString()))}");
            }
        }

        public static async Task BotInteractAsync(ICommandContext ctx)
        {
            var canInterract = ctx.GetArgument<string>("caninteract");
            if (ctx.Guild == null)
            {
                await ctx.ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(ctx.User, ctx.Guild.OwnerId))
            {
                await ctx.ReplyAsync("You don't have the permission to do this command.");
            }
            else if (canInterract != "true" && canInterract != "false")
            {
                await ctx.ReplyAsync("You must provide 'true' or 'false' as an argument.");
            }
            else
            {
                await Program.P.GetDb().SetCanBotInteract(ctx.Guild.Id, canInterract == "true");
                await ctx.ReplyAsync("Your preferences were updated");
            }
        }

        public static async Task VoteRequiredAsync(ICommandContext ctx)
        {
            var nbVotes = ctx.GetArgument<int>("nbvotes");
            if (ctx.Guild == null)
            {
                await ctx.ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(ctx.User, ctx.Guild.OwnerId))
            {
                await ctx.ReplyAsync("You don't have the permission to do this command.");
            }
            await Program.P.GetDb().SetBotVotesRequired(ctx.Guild.Id, nbVotes);
            await ctx.ReplyAsync("Your preferences were updated");
        }

        public static async Task CanUnpinAsync(ICommandContext ctx)
        {
            var canUnpin = ctx.GetArgument<string>("canunpin");
            if (ctx.Guild == null)
            {
                await ctx.ReplyAsync("This command is only available in a guild.");
            }
            if (!CanModify(ctx.User, ctx.Guild.OwnerId))
            {
                await ctx.ReplyAsync("You don't have the permission to do this command.");
            }
            else if (canUnpin != "true" && canUnpin != "false")
            {
                await ctx.ReplyAsync("You must provide 'true' or 'false' as an argument.");
            }
            else
            {
                await Program.P.GetDb().SetCanUnpin(ctx.Guild.Id, canUnpin == "true");
                await ctx.ReplyAsync("Your preferences were updated");
            }
        }
    }
}