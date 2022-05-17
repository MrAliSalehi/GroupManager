using System.Text.RegularExpressions;

namespace GroupManager.Common.Globals;

public struct RegPatterns
{
    private static readonly Regex MemberBotCommandRegex = new(@"^!.+", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex AdminBotCommandRegex = new(@"^!!.+", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private static readonly Regex MessageLimitCommandRegex =
        new(@"!!set ml\s+(?<count>\d+)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));
    private static readonly Regex SetTbmCommandRegex = new(@"!!set tbm\s+-from\s+(?<from>\d+:\d+\s+\w{2})\s+-until\s+(?<until>\d+:\d+\s+\w{2})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

    public struct Is
    {
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

        public static Match? MessageLimitData(string? message) => message is null ? null : MessageLimitCommandRegex.Match(message);

        public static Match? MemberBotCommand(string? message) => message is null ? null : MemberBotCommandRegex.Match(message);

        public static Match? AdminBotCommand(string? message) => message is null ? null : AdminBotCommandRegex.Match(message);

    }
}