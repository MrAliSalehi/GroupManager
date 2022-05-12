namespace GroupManager.DataLayer.Models;
#nullable disable
public class Group
{
    public int Id { get; set; }
    public long GroupId { get; set; }
    public short MaxWarns { get; set; }
    public bool BanOnCurse { get; set; } = false;
    public bool MuteOnCurse { get; set; } = true;
    public bool WarnOnCurse { get; set; } = true;
    public TimeSpan MuteTime { get; set; } = TimeSpan.FromHours(3);

    public bool BanOnMaxWarn { get; set; } = false;
    public bool MuteOnMaxWarn { get; set; } = true;
}