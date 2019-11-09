using Discord;
using Newtonsoft.Json;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pina
{
    public class Db
    {
        public Db()
        {
            R = RethinkDB.R;
            guildsLanguage = new Dictionary<ulong, string>();
            guildsVerbosity = new Dictionary<ulong, string>();
            guildsWhitelist = new Dictionary<ulong, string>();
            guildsBlacklist = new Dictionary<ulong, string>();
            guildsPrefix = new Dictionary<ulong, string>();
        }

        public async Task InitAsync(string dbName = "Pina")
        {
            this.dbName = dbName;
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                await R.DbCreate(dbName).RunAsync(conn);
            if (!await R.Db(dbName).TableList().Contains("Guilds").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Guilds").RunAsync(conn);
        }

        private const string defaultLanguage = "en";
        private const string defaultVebosity = "error";
        private const string defaultWhitelist = "0";
        private const string defaultPrefix = "p.";

        public async Task InitGuildAsync(ulong guildId)
        {
            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildId.ToString())
                   .With("language", defaultLanguage)
                   .With("verbosity", defaultVebosity)
                   .With("whitelist", defaultWhitelist)
                   .With("backlist", defaultWhitelist)
                   .With("prefix", defaultPrefix)
                    ).RunAsync(conn);
                UpdateLanguage(guildId, defaultLanguage);
                UpdateVerbosity(guildId, defaultVebosity);
                UpdateWhitelist(guildId, defaultWhitelist);
                UpdateBlacklist(guildId, defaultWhitelist);
                UpdatePrefix(guildId, defaultPrefix);
            }
            else
            {
                dynamic json = await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn);
                UpdateLanguage(guildId, (string)json.language);
                UpdateVerbosity(guildId, (string)json.verbosity);
                UpdateWhitelist(guildId, (string)json.whitelist);
                UpdatePrefix(guildId, (string)json.prefix);
                var blacklist = (string)json.blacklist;
                UpdateBlacklist(guildId, blacklist ?? "0");
            }
        }

        public async Task SetLanguageAsync(ulong guildId, string language)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("language", language)
                ).RunAsync(conn);
            UpdateLanguage(guildId, language);
        }

        public async Task SetPrefix(ulong guildId, string prefix)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("prefix", prefix)
                ).RunAsync(conn);
            UpdatePrefix(guildId, prefix);
        }

        public async Task SetVerbosity(ulong guildId, string verbosity)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("verbosity", verbosity)
                ).RunAsync(conn);
            UpdateVerbosity(guildId, verbosity);
        }

        public async Task SetWhitelist(ulong guildId, string whitelist)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("whitelist", whitelist)
                ).RunAsync(conn);
            UpdateWhitelist(guildId, whitelist);
        }

        public async Task SetBlacklist(ulong guildId, string blacklist)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("blacklist", blacklist)
                ).RunAsync(conn);
            UpdateBlacklist(guildId, blacklist);
        }

        public bool IsErrorOrMore(Verbosity v)
            => v == Verbosity.Error || v == Verbosity.Info;

        public string GetLanguage(ulong? guildId)
            => guildId == null ? defaultLanguage : guildsLanguage[guildId.Value];

        public string GetPrefix(ulong? guildId)
            => guildsLanguage == null ? defaultPrefix : guildsPrefix[guildId.Value];

        public Verbosity GetVerbosity(ulong? guildId)
        {
            string str = guildId == null ? defaultVebosity : guildsVerbosity[guildId.Value];
            if (str == "none")
                return Verbosity.None;
            else if (str == "error")
                return Verbosity.Error;
            else
                return Verbosity.Info;
        }

        public bool IsWhitelisted(ulong? guildId, IUser user)
        {
            if (guildId == null)
                return true;
            string value = guildsWhitelist[guildId.Value];
            if (value == "0")
                return true;
            IGuildUser guildUser = (IGuildUser)user;
            string[] allRoles = value.Split('|');
            return guildUser.RoleIds.Any(x => allRoles.Contains(x.ToString()));
        }

        public bool IsBlacklisted(ulong? guildId, IUser user)
        {
            if (guildId == null)
                return false;
            string value = guildsBlacklist[guildId.Value];
            if (value == "0")
                return false;
            IGuildUser guildUser = (IGuildUser)user;
            string[] allUsers = value.Split('|');
            return allUsers.Contains(guildUser.Id.ToString());
        }

        private void UpdateLanguage(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsLanguage);

        private void UpdateVerbosity(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsVerbosity);

        private void UpdateWhitelist(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsWhitelist);

        private void UpdateBlacklist(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsBlacklist);

        private void UpdatePrefix(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsPrefix);

        private void UpdateDictionary(ulong guildId, string value, Dictionary<ulong, string> dict)
        {
            if (dict.ContainsKey(guildId))
                dict[guildId] = value;
            else
                dict.Add(guildId, value);
        }

        public async Task<string> GetGuildAsync(ulong guildId)
            => JsonConvert.SerializeObject(await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn));

        public enum Verbosity
        {
            None,
            Error,
            Info
        }

        private Dictionary<ulong, string> guildsLanguage;
        private Dictionary<ulong, string> guildsVerbosity;
        private Dictionary<ulong, string> guildsWhitelist;
        private Dictionary<ulong, string> guildsBlacklist;
        private Dictionary<ulong, string> guildsPrefix;

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
