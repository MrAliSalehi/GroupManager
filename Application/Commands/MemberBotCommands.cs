using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Humanizer;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupManager.Application.Commands;

public class MemberBotCommands : HandlerBase
{
    public MemberBotCommands(ITelegramBotClient client) : base(client)
    {
    }

    public async Task MeAsync(Message message, CancellationToken ct)
    {
        if (message.From is null)
            return;
        var group = await GroupController.GetGroupByIdAsync(message.Chat.Id, ct);
        if (group is null)
            return;

        var user = await UserController.GetUserByIdAsync(message.From.Id, ct) ?? await UserController.TryAddUserAsync(message.From.Id, ct);


        await Client.SendTextMessageAsync(message.Chat.Id,
            $"User: {message.From.Id}-@{message.From.Username}\nSent Stickers:{user.SentStickers}/{group.StickerLimits}\nSent Gif:{user.SentGif}/{group.GifLimits}\n" +
            $"Sent Videos:{user.SentVideos}/{group.VideoLimits}\nSent Photos:{group.PhotoLimits}\nMessage Count:{user.MessageCount}/{group.MaxMessagePerUser}\n\n" +
            $"Warns:{user.Warns}/{group.MaxWarns}",
            cancellationToken: ct, replyToMessageId: message.MessageId);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task TimeAsync(Message message, CancellationToken ct)
    {
        if (message.From is null)
            return;
        var time = message.Date.TimeOfDay;
        await Client.SendTextMessageAsync(message.Chat.Id, $"Time:{time}\n({time.Humanize(5)})",
            replyToMessageId: message.MessageId, cancellationToken: ct);
    }

    internal async Task BenMeAsync(Message message, CancellationToken ct)
    {
        if (message.From is null)
            return;
        if (ManagerConfig.Admins.Contains(message.From.Id))
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Ban Admins", replyToMessageId: message.MessageId, cancellationToken: ct);
            return;
        }
        try
        {
            await Client.BanChatMemberAsync(message.Chat.Id, message.From.Id, DateTime.Now + TimeSpan.FromMinutes(20), cancellationToken: ct);
            await Client.SendTextMessageAsync(message.Chat.Id, "Good Bye!", replyToMessageId: message.MessageId, cancellationToken: ct);

        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "There Is Some Permission Issues", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
    }
}