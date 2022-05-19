namespace GroupManager.DataLayer.Models;

public partial class FloodSettings
{
    public int Id { get; set; }
    /// <summary>
    /// Number of second That AntiFlood Will Reset Cached Users,(Per Second Message)
    /// </summary>
    public uint Interval { get; set; }
    /// <summary>
    /// Count of messages that user can send in given interval
    /// </summary>
    public uint MessageCountPerInterval { get; set; }
    public bool Enabled { get; set; }
    public bool MuteOnDetect { get; set; }
    public bool BanOnDetect { get; set; }
    public TimeSpan RestrictTime { get; set; }
    public long GroupId { get; set; }
    public virtual Group Group { get; set; } = default!;

}