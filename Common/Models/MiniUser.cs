namespace GroupManager.Common.Models;

public class MiniUser
{
    public MiniUser()
    {
        ExpireTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds(10);
    }
    public long UserId { get; init; }
    public long ChatId { get; init; }
    public ushort MessageCount { get; set; }
    public TimeSpan ExpireTime { get; }
    public byte IsAlreadyMuted { get; set; }
}