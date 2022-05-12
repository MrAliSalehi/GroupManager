namespace GroupManager.DataLayer.Models;
#nullable disable
public class User
{
    //todo : limitations should come from admin configs
    public int Id { get; set; }
    public long UserId { get; set; }
    public short Warns { get; set; } = 0;
    public bool IsBanned { get; set; } = false;
    public bool IsBot { get; set; }
    public short StickerLimits { get; set; }
    public short SentStickers { get; set; } = 0;
    public short VideoLimits { get; set; }
    public short SentVideos { get; set; } = 0;
    public short GifLimits { get; set; }
    public short SentGif { get; set; } = 0;
    public short PhotoLimits { get; set; }
    public short SentPhotos { get; set; } = 0;
}