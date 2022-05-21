
namespace GroupManager.DataLayer.Models
{
    public partial class User
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public uint Warns { get; set; }
        //public bool IsBanned { get; set; }
        //public bool IsBot { get; set; }
        //public uint StickerLimits { get; set; }
        public uint SentStickers { get; set; }
        //public uint VideoLimits { get; set; }
        public uint SentVideos { get; set; }
        //  public uint GifLimits { get; set; }
        public uint SentGif { get; set; }
        // public uint PhotoLimits { get; set; }
        public uint SentPhotos { get; set; }
        public uint MessageCount { get; set; }
    }
}
