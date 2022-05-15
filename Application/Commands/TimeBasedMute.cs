using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupManager.Application.Commands;

public class TimeBasedMute
{
    private readonly ITelegramBotClient _bot;

    public TimeBasedMute()
    {
        _bot = new TelegramBotClient(Globals.BotConfigs.Token);
    }

    public async Task TimeBasedMuteAsync(Chat chat)
    {
        await _bot.SetChatPermissionsAsync(chat.Id, Globals.MutePermissions);
        await _bot.SendTextMessageAsync(chat.Id, "Auto Mute has been triggered!");
    }
    public async Task TimeBasedUnMuteAsync(Chat chat)
    {
        await _bot.SetChatPermissionsAsync(chat.Id, Globals.UnMutePermissions);
        await _bot.SendTextMessageAsync(chat.Id, "Auto Mute Turned Off!");
    }
}