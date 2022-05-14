using System;
using System.Collections.Generic;

namespace GroupManager.DataLayer.Models
{
    public partial class ForceJoinChannel
    {
        public long Id { get; set; }
        public string ChannelId { get; set; } = string.Empty;

        public long GroupId { get; set; }
        public virtual Group Group { get; set; } = default!;
    }
}
