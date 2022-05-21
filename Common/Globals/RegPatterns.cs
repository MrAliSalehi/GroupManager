using System.Text.RegularExpressions;

namespace GroupManager.Common.Globals;

public struct RegPatterns
{
    private static readonly Regex PublicLinkRegex =
        new(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(600));

    private static readonly Regex TelegramLinkRegex = new(@"(t|telesco|telegram)\.(me|dog|pe)\/(.+)",
        RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromSeconds(1));

    private static readonly Regex IdRegex = new(@"^@\w{4,}", RegexOptions.Compiled | RegexOptions.Multiline,
        TimeSpan.FromMilliseconds(300));
    private static readonly Regex HashTagRegex = new(@"^#[a-z-0-9_]+", RegexOptions.Compiled | RegexOptions.Multiline,
        TimeSpan.FromMilliseconds(300));

    private static readonly Regex MemberBotCommandRegex = new(@"^!.+", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex AdminBotCommandRegex = new(@"^!!.+", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex MessageLimitCommandRegex = new(@"!!set ml\s+(?<count>\d+)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));
    private static readonly Regex MuteUserCommandRegex = new(@"!!mute\s+(?<userId>\d+)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));
    private static readonly Regex BaseCommandRegex = new(@"(?<=[-{1,2}|\/])(?<name>[a-zA-Z0-9]*)[ |:|""]*(?<value>[\w|.|?|=|&|+| |:|\/|\\]*)(?=[ |""]|$)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(2));

    private static readonly Regex SetTbmCommandRegex = new(@"!!set tbm\s+-from\s+(?<from>\d+:\d+\s+\w{2})\s+-until\s+(?<until>\d+:\d+\s+\w{2})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));



    public struct Is
    {

        public static bool Id(string message) => IdRegex.IsMatch(message);
        public static bool HashTag(string message) => HashTagRegex.IsMatch(message);
        public static bool PublicLink(string message) => PublicLinkRegex.IsMatch(message);
        public static bool TelegramLink(string message) => TelegramLinkRegex.IsMatch(message);
        public static bool MemberBotCommand(string? message)
        {
            return message is not null && MemberBotCommandRegex.IsMatch(message);
        }

        public static bool AdminBotCommand(string? message)
        {
            return message is not null && AdminBotCommandRegex.IsMatch(message);
        }
    }

    public struct Get
    {
        public static Match? TbmData(string? message) => message is null ? null : SetTbmCommandRegex.Match(message);

        public static Match? MuteUserData(string? message) => message is null ? null : MuteUserCommandRegex.Match(message);

        public static Match? MessageLimitData(string? message) => message is null ? null : MessageLimitCommandRegex.Match(message);

        public static Match? MemberBotCommand(string? message) => message is null ? null : MemberBotCommandRegex.Match(message);

        public static Match? AdminBotCommand(string? message) => message is null ? null : AdminBotCommandRegex.Match(message);

        public static MatchCollection? BaseCommandData(string? message) => message is null ? null : BaseCommandRegex.Matches(message);
    }
}