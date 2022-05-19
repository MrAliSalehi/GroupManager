using GroupManager.Application.Commands;
using Telegram.Bot;

namespace GroupManager.Common.Globals;

public struct ManagerConfig
{
    public static List<long> Admins { get; } = new() { 1127927726 };
    /// <summary>
    /// Key Is Group Id And SpamCommands Is Separate For Each Group
    /// </summary>
    //public static Dictionary<long, SpamCommands> SpamCommands { get; } = new();
    public static ITelegramBotClient Client { get; set; } = new TelegramBotClient(Globals.BotConfigs.Token);
    public static string BotUserName { get; set; } = "";
}