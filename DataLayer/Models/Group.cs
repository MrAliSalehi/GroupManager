
namespace GroupManager.DataLayer.Models
{
    public partial class Group
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public short MaxWarns { get; set; }
        public bool BanOnCurse { get; set; }
        public bool MuteOnCurse { get; set; }
        public TimeSpan MuteTime { get; set; } = TimeSpan.FromHours(3);
        public bool WarnOnCurse { get; set; }
        public bool BanOnMaxWarn { get; set; }
        public bool MuteOnMaxWarn { get; set; }
        public bool SayWelcome { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool ForceJoin { get; set; }
        public virtual ICollection<ForceJoinChannel> ForceJoinChannel { get; set; } = default!;
        public bool TimeBasedMute { get; set; } = false;
        public DateTime TimeBasedMuteFromTime { get; set; }
        public DateTime TimeBasedMuteUntilTime { get; set; }
        public string TimeBasedMuteFuncHashId { get; set; } = string.Empty;
        public string TimeBasedUnmuteFuncHashId { get; set; } = string.Empty;
        public uint MaxMessagePerUser { get; set; } = 300;
        public bool EnableMessageLimitPerUser { get; set; }
        public bool AntiJoin { get; set; } = false;
        public bool AntiBot { get; set; } = false;
        public bool AntiForward { get; set; }
        public virtual FloodSettings FloodSetting { get; set; } = default!;
    }
}
