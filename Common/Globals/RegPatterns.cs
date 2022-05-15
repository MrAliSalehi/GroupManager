using System.Text.RegularExpressions;

namespace GroupManager.Common.Globals;

public struct RegPatterns
{
    private static readonly Regex MemberBotCommandRegex = new(@"^!.+", RegexOptions.Compiled);
    private static readonly Regex AdminBotCommandRegex = new(@"^!!.+", RegexOptions.Compiled);

    private static readonly Regex SetTbmCommandRegex =
        new(@"!!set tbm\s+-from\s+(?<from>\d+:\d+)\s+-until\s+(?<until>\d+:\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        public static Match? MemberBotCommand(string? message) => message is null ? null : MemberBotCommandRegex.Match(message);

        public static Match? AdminBotCommand(string? message) => message is null ? null : AdminBotCommandRegex.Match(message);
    }
}