namespace GroupManager.Common.Models;

public class MiniUser
{
    public MiniUser(uint interval)
    {
        ExpireTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(interval));
    }
    public long UserId { get; init; }
    public long ChatId { get; init; }
    public ushort MessageCount { get; set; }
    public TimeSpan ExpireTime { get; }
    public byte IsAlreadyMuted { get; set; }
}