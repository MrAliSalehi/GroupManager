global using GroupManager.Common.Globals;
global using Serilog;
global using GroupManager.Common.Extensions;
using GroupManager.Common.Models;
using Telegram.Bot.Types;


namespace GroupManager.Common.Globals
{
    public static class Globals
    {
        public static IConfiguration Configuration { get; set; } = default!;
        public static BotConfigs BotConfigs { get; set; } = new();
        public static string ApplicationEnv => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")!;

        public static readonly ChatPermissions MutePermissions = new()
        {
            CanInviteUsers = true,
        };
        public static readonly ChatPermissions UnMutePermissions = new()
        {

            CanSendMessages = true,
            CanSendPolls = true,
            CanSendMediaMessages = true,
            CanSendOtherMessages = true,
            CanInviteUsers = true,
            CanAddWebPagePreviews = true,
            CanPinMessages = false,
            CanChangeInfo = false,
        };
    }

}