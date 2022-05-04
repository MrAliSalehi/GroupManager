using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Handlers;

public class MyChatMemberHandler : HandlerBase
{
    public MyChatMemberHandler(ITelegramBotClient client) : base(client)
    {
    }

    internal async Task InitHandlerAsync(ChatMemberUpdated chatMember, CancellationToken ct)
    {

        await OwnChatMemberUpdateAsync(chatMember, ct);

        await CheckNewGroupAsync(chatMember, ct);

        await ChatStatusUpdateAsync(chatMember, ct);
    }

    private async Task CheckNewGroupAsync(ChatMemberUpdated update, CancellationToken ct)
    {
        if (!ManagerConfig.Admins.Contains(update.From.Id))
        {
            await Client.SendTextMessageAsync(update.Chat.Id,
                "Bot Is Added From Unregistered Account\n<i>Leaving Chat...</i>", ParseMode.Html,
                cancellationToken: ct);
            await Client.LeaveChatAsync(update.Chat.Id, ct);
            return;
        }
    }

    private async Task OwnChatMemberUpdateAsync(ChatMemberUpdated update, CancellationToken ct)
    {
        if (update.NewChatMember.User.Username != ManagerConfig.BotUserName)
            return;

        switch (update.NewChatMember.Status)
        {
            case ChatMemberStatus.Member when (update.OldChatMember.Status == ChatMemberStatus.Administrator):
                await Client.SendTextMessageAsync(update.Chat.Id, "<i>Bot Is No Longer Admin In This Group!</i>", ParseMode.Html, cancellationToken: ct);
                break;
            case ChatMemberStatus.Member:
                {
                    var userId = update.From.Username is null ?
                        (update.From.FirstName + " " + update.From.LastName)
                        : $"@{update.From.Username}";
                    await Client.SendTextMessageAsync(update.Chat.Id, $"Added By {userId}\nManager <b>Is Not Registered Here</b>\nPlease Make Sure To Grant <b>Admin</b> Permissions For Bot!", ParseMode.Html, cancellationToken: ct);
                    break;
                }
            case ChatMemberStatus.Administrator:

                await Client.SendTextMessageAsync(update.Chat.Id, "<i>Bot Has Been Marked As Administrator\nan Bot-Admin Need To Confirm This Button.</i>",
                    ParseMode.Html, replyMarkup: InlineButtons.Admin.ConfirmChat, cancellationToken: ct);
                break;

            case ChatMemberStatus.Left:
            case ChatMemberStatus.Kicked:
                await GroupController.RemoveGroupAsync(update.Chat.Id, ct);
                break;
            case ChatMemberStatus.Creator:
            case ChatMemberStatus.Restricted:
            default:
                break;
        }
    }
    private async Task ChatStatusUpdateAsync(ChatMemberUpdated chatMember, CancellationToken ct)
    {

    }

}