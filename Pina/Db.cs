using Newtonsoft.Json;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Collections.Generic;
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
        private const string defaultVebosity = "info";
        private const string defaultWhitelist = "0";

        public async Task InitGuildAsync(ulong guildId)
        {
            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildId.ToString())
                   .With("language", defaultLanguage)
                   .With("verbosity", defaultVebosity)
                   .With("whitelist", defaultWhitelist)
                    ).RunAsync(conn);
                UpdateLanguage(guildId, defaultLanguage);
                UpdateVerbosity(guildId, defaultVebosity);
                UpdateWhitelist(guildId, defaultWhitelist);
            }
            else
            {
                dynamic json = await R.Db(dbName).Table("Languages").Get(guildId.ToString()).RunAsync(conn);
                UpdateLanguage(guildId, json.language);
                UpdateVerbosity(guildId, json.verbosity);
                UpdateWhitelist(guildId, json.whitelist);
            }
        }

        public async Task SetLanguageAsync(ulong guildId, string language)
        {
            await R.Db(dbName).Table("Languages").Update(R.HashMap("id", guildId.ToString())
                .With("language", language)
                ).RunAsync(conn);
            UpdateLanguage(guildId, language);
        }

        private string GetLanguage(ulong guildId)
        {
            return (guildsLanguage[guildId]);
        }

        private void UpdateLanguage(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsLanguage);

        private void UpdateVerbosity(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsVerbosity);

        private void UpdateWhitelist(ulong guildId, string value)
            => UpdateDictionary(guildId, value, guildsWhitelist);

        private void UpdateDictionary(ulong guildId, string value, Dictionary<ulong, string> dict)
        {
            if (dict.ContainsKey(guildId))
                dict[guildId] = value;
            else
                dict.Add(guildId, value);
        }

        public async Task<string> GetGuildAsync(ulong guildId)
            => JsonConvert.SerializeObject(await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn));

        private Dictionary<ulong, string> guildsLanguage;
        private Dictionary<ulong, string> guildsVerbosity;
        private Dictionary<ulong, string> guildsWhitelist;

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
