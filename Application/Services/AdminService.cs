using GroupManager.Application.Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Services;

public class AdminService : Service
{
    public AdminService(ITelegramBotClient client) : base(client)
    {

    }

    internal async Task InitAsync(ChatMemberUpdated chatMember, CancellationToken ct)
    {
        if (!ManagerConfig.Admins.Contains(chatMember.From.Id))
        {
            await Client.SendTextMessageAsync(chatMember.Chat.Id,
                "Bot Is Added From Unregistered Account\n<i>Leaving Chat...</i>", ParseMode.Html,
                cancellationToken: ct);
            await Client.LeaveChatAsync(chatMember.Chat.Id, ct);
            return;
        }
        await ChatStatusUpdateAsync(chatMember, ct);
    }
    private async Task ChatStatusUpdateAsync(ChatMemberUpdated chatMember, CancellationToken ct)
    {
        switch (chatMember.NewChatMember.Status)
        {
            case ChatMemberStatus.Member when (chatMember.OldChatMember.Status == ChatMemberStatus.Administrator):
                await Client.SendTextMessageAsync(chatMember.Chat.Id, "<i>Bot Is No Longer Admin In This Group!</i>", ParseMode.Html, cancellationToken: ct);
                break;
            case ChatMemberStatus.Member:
                {
                    var userId = chatMember.From.Username is null ?
                        (chatMember.From.FirstName + " " + chatMember.From.LastName)
                        : $"@{chatMember.From.Username}";
                    await Client.SendTextMessageAsync(chatMember.Chat.Id, $"Added By {userId}\nManager <b>Is Not Registered Here</b>\nPlease Make Sure To Grant <b>Admin</b> Permissions For Bot!", ParseMode.Html, cancellationToken: ct);
                    break;
                }
            case ChatMemberStatus.Administrator:
                await Client.SendTextMessageAsync(chatMember.Chat.Id, "<i>Bot Has Been Marked As Administrator</i>",
                    ParseMode.Html, cancellationToken: ct);
                break;

            case ChatMemberStatus.Creator:
            case ChatMemberStatus.Left:
            case ChatMemberStatus.Kicked:
            case ChatMemberStatus.Restricted:
            default:
                break;
        }
    }
}