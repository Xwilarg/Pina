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
            guildsVerbosity = new();
            guildsWhitelist = new();
            guildsBlacklist = new();
            guildsPrefix = new();
            guildsCanBotInteract = new();
            guildsVotesRequired = new();
            guildsCanUnpin = new();
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

        private const string defaultVebosity = "error";
        private const string defaultWhitelist = "0";
        private const string defaultPrefix = "p.";
        public const bool defaultCanBotInteract = false;
        public const bool defaultCanUnpin = true;

        public async Task InitGuildAsync(ulong guildId)
        {
            if (guildsPrefix.ContainsKey(guildId))
                return;

            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildId.ToString())
                   .With("verbosity", defaultVebosity)
                   .With("whitelist", defaultWhitelist)
                   .With("blacklist", defaultWhitelist)
                   .With("prefix", defaultPrefix)
                   .With("canBotInteract", defaultCanBotInteract)
                   .With("canUnpin", defaultCanUnpin)
                    ).RunAsync(conn);
                UpdateVerbosity(guildId, defaultVebosity);
                UpdateWhitelist(guildId, defaultWhitelist);
                UpdateBlacklist(guildId, defaultWhitelist);
                UpdatePrefix(guildId, defaultPrefix);
                UpdateCanBotInteract(guildId, defaultCanBotInteract);
                UpdateBotVotesRequired(guildId, 1);
                UpdateCanUnpin(guildId, defaultCanUnpin);
            }
            else
            {
                dynamic json = await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn);
                UpdateVerbosity(guildId, (string)json.verbosity);
                UpdateWhitelist(guildId, (string)json.whitelist);
                UpdatePrefix(guildId, (string)json.prefix);
                var blacklist = (string)json.blacklist;
                UpdateBlacklist(guildId, blacklist ?? defaultWhitelist);
                var canBotInteract = (bool?)json.canBotInteract;
                UpdateCanBotInteract(guildId, canBotInteract ?? defaultCanBotInteract);
                var votesRequired = (int?)json.votesRequired;
                UpdateBotVotesRequired(guildId, votesRequired ?? 1);
                var canUnpin = (bool?)json.canUnpin;
                UpdateCanUnpin(guildId, canUnpin ?? defaultCanUnpin);
            }
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

        public async Task SetCanBotInteract(ulong guildId, bool value)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("canBotInteract", value)
                ).RunAsync(conn);
            UpdateCanBotInteract(guildId, value);
        }

        public async Task SetBotVotesRequired(ulong guildId, int value)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("votesRequired", value)
                ).RunAsync(conn);
            UpdateBotVotesRequired(guildId, value);
        }

        public async Task SetCanUnpin(ulong guildId, bool value)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("canUnpin", value)
                ).RunAsync(conn);
            UpdateCanUnpin(guildId, value);
        }

        public bool IsErrorOrMore(Verbosity v)
            => v == Verbosity.Error || v == Verbosity.Info;

        public string GetPrefix(ulong? guildId)
            => guildsPrefix == null ? defaultPrefix : guildsPrefix[guildId.Value];

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

        public bool IsCanBotInteract(ulong? guildId)
        {
            if (guildId == null || !guildsCanBotInteract.ContainsKey(guildId.Value))
                return false;
            var value = guildsCanBotInteract[guildId.Value];
            return value;
        }

        public int GetVotesRequired(ulong? guildId)
        {
            if (guildId == null || !guildsVotesRequired.ContainsKey(guildId.Value))
                return 1;
            return guildsVotesRequired[guildId.Value];
        }

        public bool IsCanUnpin(ulong? guildId)
        {
            if (guildId == null || !guildsCanUnpin.ContainsKey(guildId.Value))
                return defaultCanUnpin;
            var value = guildsCanUnpin[guildId.Value];
            return value;
        }

        private void UpdateVerbosity(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsVerbosity);

        private void UpdateWhitelist(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsWhitelist);

        private void UpdateBlacklist(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsBlacklist);

        private void UpdatePrefix(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsPrefix);

        private void UpdateCanBotInteract(ulong guildId, bool value)
            => UpdateDictionary(guildId, value, guildsCanBotInteract);

        private void UpdateBotVotesRequired(ulong guildId, int value)
            => UpdateDictionary(guildId, value, guildsVotesRequired);

        private void UpdateCanUnpin(ulong guildId, bool value)
            => UpdateDictionary(guildId, value, guildsCanUnpin);

        private void UpdateDictionary<T>(ulong guildId, T value, Dictionary<ulong, T> dict)
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

        private Dictionary<ulong, string> guildsVerbosity;
        private Dictionary<ulong, string> guildsWhitelist;
        private Dictionary<ulong, string> guildsBlacklist;
        private Dictionary<ulong, string> guildsPrefix;
        private Dictionary<ulong, bool> guildsCanBotInteract;
        private Dictionary<ulong, int> guildsVotesRequired;
        private Dictionary<ulong, bool> guildsCanUnpin;

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
