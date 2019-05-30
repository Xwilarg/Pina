using DiscordUtils;

namespace Pina
{
    public static class Sentences
    {

        private static string Translate(ulong? guildId, string key, params string[] args)
            => Utils.Translate(Program.P.translations, guildId != null ? Program.P.GetDb().GetLanguage(guildId.Value) : null, key, args);

        public static string AlreadyPinned(ulong? guildId) => Translate(guildId, "alreadyPinned");
        public static string MissingPermission(ulong? guildId) => Translate(guildId, "missingPermission");
        public static string NothingToPing(ulong? guildId) => Translate(guildId, "nothingToPing");
        public static string InvalidId(ulong? guildId) => Translate(guildId, "invalidId");

        public static string DataSavedAbout(ulong? guildId, string about) => Translate(guildId, "dataSavedAbout", about);
        public static string GdprPm(ulong? guildId) => Translate(guildId, "gdprPm");

        public static string SettingsPm(ulong? guildId) => Translate(guildId, "settingsPm");
        public static string LanguageHelp(ulong? guildId, string languageList) => Translate(guildId, "languageHelp", languageList);
        public static string VerbosityHelp(ulong? guildId) => Translate(guildId, "verbosityHelp");
        public static string InvalidLanguage(ulong? guildId, string languageList) => Translate(guildId, "invalidLanguage", languageList);
        public static string InvalidVerbosity(ulong? guildId) => Translate(guildId, "invalidVerbosity");
        public static string InvalidWhitelist(ulong? guildId, string id) => Translate(guildId, "invalidWhitelist", id);
        public static string LanguageSet(ulong? guildId, string language) => Translate(guildId, "languageSet", language);
        public static string PrefixSet(ulong? guildId, string prefix) => Translate(guildId, "prefixSet", prefix);
        public static string VerbositySet(ulong? guildId, string verbosity) => Translate(guildId, "verbositySet", verbosity);
        public static string WhitelistSet(ulong? guildId, string roles) => Translate(guildId, "whitelistSet", roles);
        public static string WhitelistUnset(ulong? guildId) => Translate(guildId, "whitelistUnset");

        public static string HelpIntro(ulong? guildId) => Translate(guildId, "helpIntro");
        public static string HelpPerm(ulong? guildId) => Translate(guildId, "helpPerm");
        public static string HelpPin(ulong? guildId) => Translate(guildId, "helpPin");
        public static string HelpSettings(ulong? guildId) => Translate(guildId, "helpSettings");
        public static string HelpLanguage(ulong? guildId) => Translate(guildId, "helpLanguage");
        public static string HelpVerbosity(ulong? guildId) => Translate(guildId, "helpVerbosity");
        public static string HelpWhitelist(ulong? guildId) => Translate(guildId, "helpWhitelist");
        public static string HelpPrefix(ulong? guildId) => Translate(guildId, "helpPrefix");
        public static string HelpCommunication(ulong? guildId) => Translate(guildId, "helpCommunication");
        public static string HelpGdpr(ulong? guildId) => Translate(guildId, "helpGdpr");
        public static string HelpInfo(ulong? guildId) => Translate(guildId, "helpInfo");
    }
}
