using System.Text.RegularExpressions;

namespace GroupManager.Common.Globals;

public struct RegPatterns
{
    private static readonly Regex MemberBotCommandRegex = new(@"^!.+", RegexOptions.Compiled);
    private static readonly Regex AdminBotCommandRegex = new(@"^!!.+", RegexOptions.Compiled);

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
        public static Match? MemberBotCommand(string? message)
        {
            return message is null ? null : MemberBotCommandRegex.Match(message);
        }
        public static Match? AdminBotCommand(string? message)
        {
            return message is null ? null : AdminBotCommandRegex.Match(message);
        }
    }
}