﻿
using System.ComponentModel.DataAnnotations;
using HtmlAgilityPack;
using Mosaik.Core;

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
        public bool LimitMessageSize { get; set; }
        public uint MaxMessageSize { get; set; }
        public bool LimitMedia { get; set; }
        public uint StickerLimits { get; set; }
        public uint VideoLimits { get; set; }
        public uint GifLimits { get; set; }
        public uint PhotoLimits { get; set; }
        public virtual FloodSettings FloodSetting { get; set; } = default!;

        public bool LanguageLimit { get; set; }
        [Required]
        public virtual int PhoneTypeId { get; set; }

        [EnumDataType(typeof(Language))]
        public Language AllowedLanguage
        {
            get => (Language)this.PhoneTypeId;
            set => this.PhoneTypeId = (int)value;
        }

        public bool FilterTelLink { get; set; }
        public bool FilterPublicLink { get; set; }
        public bool FilterHashTag { get; set; }
        public bool FilterId { get; set; }
        public virtual ICollection<Admin> Admins { get; set; }
    }
}
