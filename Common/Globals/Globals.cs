global using GroupManager.Common.Globals;
global using Serilog;
global using GroupManager.Common.Extensions;
using System.Runtime.InteropServices;
using GroupManager.Common.Models;
using GroupManager.DataLayer.Models;
using Telegram.Bot.Types;


namespace GroupManager.Common.Globals
{
    public static class Globals
    {
        public static IConfiguration Configuration { get; set; } = default!;
        public static BotConfigs BotConfigs { get; set; } = new();
        public static ServiceProvider ServiceProvider { get; set; } = default!;
        public static string SlashOrBackSlash => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/" : "\\";
        public static string ConnectionString(string name = "ManagerDb") => Configuration.GetConnectionString(name);
        public static List<Describer> Describers { get; } = new();

        private static DateTime Now => DateTime.Now;

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