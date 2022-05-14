global using GroupManager.Common.Globals;
global using Serilog;
using GroupManager.Common.Models;
using Telegram.Bot.Types;


namespace GroupManager.Common.Globals
{
    public static class Globals
    {
        public static IConfiguration Configuration { get; set; } = default!;
        public static BotConfigs BotConfigs { get; set; } = new();
        public static string ApplicationEnv => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")!;
        public static readonly ChatPermissions MuteUserChatPermissions = new()
        {
            CanSendOtherMessages = false,
            CanSendMediaMessages = false,
            CanInviteUsers = false,
            CanChangeInfo = false,
            CanAddWebPagePreviews = false,
            CanPinMessages = false,
            CanSendMessages = false,
            CanSendPolls = false
        };
    }

}