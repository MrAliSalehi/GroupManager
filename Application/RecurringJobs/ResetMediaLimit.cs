using GroupManager.DataLayer.Controller;

namespace GroupManager.Application.RecurringJobs;

public class ResetMediaLimit
{
    public async Task ResetMediaLimitAsync()
    {
        await UserController.UpdateAllUsersAsync(p =>
        {
            p.SentGif = 0;
            p.SentPhotos = 0;
            p.SentVideos = 0;
            p.SentStickers = 0;
        });
    }
}