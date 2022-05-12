using GroupManager.DataLayer.Models;

namespace GroupManager.Common.Globals;

public struct ManagerConfig
{
    public static List<long> Admins { get; } = new() { 1127927726 };
    public static List<Group> Groups { get; set; } = new();

    public static string BotUserName { get; set; } = "";
}