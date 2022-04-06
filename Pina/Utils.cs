using Discord;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pina
{
    public static class Utils
    {
        public static string CleanWord(string word)
        {
            StringBuilder finalStr = new();
            foreach (char c in word)
            {
                if (char.IsLetterOrDigit(c))
                {
                    finalStr.Append(char.ToLowerInvariant(c));
                }
            }
            var result = finalStr.ToString();
            return result.Length == 0 ? word : result;
        }

        public static IRole GetRole(string name, IGuild guild)
        {
            if (guild == null)
            {
                return null;
            }
            Match match = Regex.Match(name, "<@&([0-9]{18})>");
            if (match.Success)
            {
                IRole role = guild.GetRole(ulong.Parse(match.Groups[1].Value));
                if (role != null)
                {
                    return role;
                }
            }
            if (ulong.TryParse(name, out ulong id2))
            {
                IRole role = guild.GetRole(id2);
                if (role != null)
                {
                    return role;
                }
            }
            string lowerName = CleanWord(name);
            foreach (IRole role in guild.Roles)
            {
                if (CleanWord(role.Name) == lowerName)
                {
                    return role;
                }
            }
            return null;
        }

        public static async Task<IGuildUser> GetUserAsync(string name, IGuild guild)
        {
            Match match = Regex.Match(name, "<@[!]?([0-9]{18})>");
            if (match.Success)
            {
                IGuildUser user = await guild.GetUserAsync(ulong.Parse(match.Groups[1].Value));
                if (user != null)
                {
                    return user;
                }
            }
            if (ulong.TryParse(name, out ulong id2))
            {
                IGuildUser user = await guild.GetUserAsync(id2);
                if (user != null)
                {
                    return user;
                }
            }
            name = name.ToLowerInvariant();
            foreach (IGuildUser user in await guild.GetUsersAsync())
            {
                if (user.Nickname?.ToLowerInvariant() == name || user.Username.ToLowerInvariant() == name || user.ToString().ToLowerInvariant() == name)
                {
                    return user;
                }
            }
            return null;
        }

        public static async Task<IMessage> GetMessageAsync(string id, IMessageChannel chan)
        {
            if (!ulong.TryParse(id, out ulong uid))
                return null;
            IMessage msg;
            if (uid != 0)
            {
                msg = await chan.GetMessageAsync(uid);
                if (msg != null)
                    return msg;
            }
            if (chan is not ITextChannel textChan || uid == 0)
                return null;
            foreach (ITextChannel c in await textChan.Guild.GetTextChannelsAsync())
            {
                try
                {
                    msg = await c.GetMessageAsync(uid);
                    if (msg != null)
                        return msg;
                }
                catch (Discord.Net.HttpException)
                { }
            }
            return null;
        }

        public static string TimeSpanToString(TimeSpan ts)
        {
            string finalStr = $"{ts.Seconds} seconds";
            if (ts.Days > 0)
                finalStr = $"{ts.Days} days, {ts.Hours} hours, {ts.Minutes} minutes and {finalStr}";
            else if (ts.Hours > 0)
                finalStr = $"{ts.Hours} hours, {ts.Minutes} minutes and {finalStr}";
            else if (ts.Minutes > 0)
                finalStr = $"{ts.Minutes} minutes and {finalStr}";
            return finalStr;
        }
    }
}
