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
        private bool CanModify(IUser user, ulong ownerId)
        {
            if (user.Id == ownerId)
                return true;
            IGuildUser guildUser = (IGuildUser)user;
            return guildUser.GuildPermissions.ManageGuild;
        }

        [Command("Language")]
        private async Task Language(params string[] args)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Sentences.SettingsPm(null));
                return;
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Sentences.SettingsNoPerm(Context.Guild.Id));
                return;
            }
            if (args.Length == 0)
            {
                await ReplyAsync(Sentences.LanguageHelp(Context.Guild.Id, string.Join(", ", Program.P.translationKeyAlternate.Select(x => x.Value[1]))));
                return;
            }
            string userInput = args[0].ToLower();
            string language = null;
            if (Program.P.translationKeyAlternate.ContainsKey(userInput))
                language = userInput;
            foreach (var l in Program.P.translationKeyAlternate)
            {
                if (l.Value.Contains(userInput))
                {
                    language = l.Key;
                    break;
                }
            }
            if (language == null)
                await ReplyAsync(Sentences.InvalidLanguage(Context.Guild.Id, string.Join(", ", Program.P.translationKeyAlternate.Select(x => x.Value[1]))));
            else
            {
                await Program.P.GetDb().SetLanguageAsync(Context.Guild.Id, language);
                await ReplyAsync(Sentences.LanguageSet(Context.Guild.Id, Program.P.translationKeyAlternate[language][1]));
            }
        }

        [Command("Prefix")]
        private async Task Prefix(params string[] args)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Sentences.SettingsPm(null));
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Sentences.SettingsNoPerm(Context.Guild.Id));
            }
            else if (args.Length == 0)
            {
                await Program.P.GetDb().SetPrefix(Context.Guild.Id, "");
                await ReplyAsync(Sentences.PrefixSet(Context.Guild.Id, Sentences.None(Context.Guild.Id)));
            }
            else
            {
                string prefix = args[0];
                await Program.P.GetDb().SetPrefix(Context.Guild.Id, prefix);
                await ReplyAsync(Sentences.PrefixSet(Context.Guild.Id, prefix));
            }
        }

        [Command("Verbosity")]
        private async Task Verbosity(params string[] args)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Sentences.SettingsPm(null));
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Sentences.SettingsNoPerm(Context.Guild.Id));
            }
            else if (args.Length == 0)
            {
                await ReplyAsync(Sentences.VerbosityHelp(Context.Guild.Id));
            }
            else
            {
                string verbosity = args[0].ToLower();
                if (verbosity != "none" && verbosity != "error" && verbosity != "info")
                    await ReplyAsync(Sentences.InvalidVerbosity(Context.Guild.Id));
                else
                {
                    await Program.P.GetDb().SetVerbosity(Context.Guild.Id, verbosity);
                    await ReplyAsync(Sentences.VerbositySet(Context.Guild.Id, verbosity));
                }
            }
        }

        [Command("Whitelist")]
        private async Task Whitelist(params string[] args)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Sentences.SettingsPm(null));
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Sentences.SettingsNoPerm(Context.Guild.Id));
            }
            else if (args.Length == 0)
            {
                await Program.P.GetDb().SetWhitelist(Context.Guild.Id, "0");
                await ReplyAsync(Sentences.WhitelistUnset(Context.Guild.Id));
            }
            else
            {
                List<IRole> roles = new List<IRole>();
                foreach (string arg in args)
                {
                    IRole role = Utils.GetRole(arg, Context.Guild);
                    if (role == null)
                    {
                        await ReplyAsync(Sentences.InvalidWhitelist(Context.Guild.Id, arg));
                        return;
                    }
                    roles.Add(role);
                }
                await Program.P.GetDb().SetWhitelist(Context.Guild.Id, string.Join("|", roles.Select(x => x.Id)));
                await ReplyAsync(Sentences.WhitelistSet(Context.Guild.Id, string.Join(", ", roles.Select(x => x.Name.Replace("@everyone", "@ everyone")))));
            }
        }

        [Command("Blacklist")]
        private async Task Blacklist(params string[] args)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Sentences.SettingsPm(null));
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Sentences.SettingsNoPerm(Context.Guild.Id));
            }
            else if (args.Length == 0)
            {
                await Program.P.GetDb().SetBlacklist(Context.Guild.Id, "0");
                await ReplyAsync(Sentences.BlacklistUnset(Context.Guild.Id));
            }
            else
            {
                List<IGuildUser> ids = new List<IGuildUser>();
                foreach (string arg in args)
                {
                    IGuildUser user = await Utils.GetUser(arg, Context.Guild);
                    if (user == null)
                    {
                        await ReplyAsync(Sentences.InvalidBlacklist(Context.Guild.Id, arg));
                        return;
                    }
                    ids.Add(user);
                }
                await Program.P.GetDb().SetBlacklist(Context.Guild.Id, string.Join("|", ids.Select(x => x.Id)));
                await ReplyAsync(Sentences.BlacklistSet(Context.Guild.Id, string.Join(", ", ids.Select(x => x.ToString()))));
            }
        }

        [Command("BotInteract"), Alias("BotInteract")]
        private async Task BotInteract([Remainder]string args)
        {
            args = args?.ToLower();
            if (Context.Guild == null)
            {
                await ReplyAsync(Sentences.SettingsPm(null));
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Sentences.SettingsNoPerm(Context.Guild.Id));
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
                await ReplyAsync(Sentences.SettingsPm(null));
            }
            if (!CanModify(Context.User, Context.Guild.OwnerId))
            {
                await ReplyAsync(Sentences.SettingsNoPerm(Context.Guild.Id));
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
