using Telegram.Bot;

namespace GroupManager.Application.RecurringJobs;

public class TimeBasedMute
{

    public static TelegramBotClient? Bot { get; set; }
    public static long ChatId { get; set; }
    public async Task TimeBasedMuteAsync()
    {
        if (Bot is null || ChatId is 0)
        {
            Log.Error("Bot:{bot}\nChat:{chat}", Bot is null, ChatId);
            return;
        }
        await Bot.SetChatPermissionsAsync(ChatId, Globals.MutePermissions);
        await Bot.SendTextMessageAsync(ChatId, "Auto Mute Enabled!");

    }
    public async Task TimeBasedUnMuteAsync()
    {
        if (Bot is null || ChatId is 0)
        {
            Log.Error("Bot:{bot}\nChat:{chat}", Bot is null, ChatId);
            return;
        }
        await Bot.SetChatPermissionsAsync(ChatId, Globals.UnMutePermissions);
        await Bot.SendTextMessageAsync(ChatId, "Auto Mute Turned Off!");
    }
}