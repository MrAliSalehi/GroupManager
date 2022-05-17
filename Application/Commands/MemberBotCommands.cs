using GroupManager.Application.Contracts;
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

        await Client.SendTextMessageAsync(message.Chat.Id, $"User: {message.From!.Id}", cancellationToken: ct, replyToMessageId: message.MessageId);
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
}