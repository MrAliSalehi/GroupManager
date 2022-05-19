global using GroupManager.Common.Globals;
global using Serilog;
global using GroupManager.Common.Extensions;
using GroupManager.Common.Models;
using GroupManager.DataLayer.Models;
using Telegram.Bot.Types;


namespace GroupManager.Common.Globals
{
    public static class Globals
    {
        public static IConfiguration Configuration { get; set; } = default!;
        public static BotConfigs BotConfigs { get; set; } = new();
        public static string ApplicationEnv => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")!;
        public static ServiceProvider ServiceProvider { get; set; } = default!;
        private static DateTime Now => DateTime.Now;
        public static string ConnectionString(string name = "ManagerDb") => Configuration.GetConnectionString(name);

        //public static readonly Hashids TbmMuteHashIds = new("TBM_MUTE", 4);
        //public static readonly Hashids TbmUnMuteHashIds = new("TBM_UNMUTE", 4);

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

        public static readonly FloodSettings DefaultFloodSettings = new()
        {
            BanOnDetect = false,
            MuteOnDetect = true,
            Enabled = true,
            Interval = 10,
            MessageCountPerInterval = 7,
            RestrictTime = TimeSpan.FromDays(3),
        };
    }

}