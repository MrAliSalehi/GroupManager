using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Humanizer;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using WordFilter;

namespace GroupManager.Application.Commands;

public class GroupCommands : HandlerBase
{
    private readonly TextFilter _textFilter;
    private static readonly ChatPermissions MutePermissions = new ChatPermissions()
    {
        CanSendMessages = false,
        CanSendMediaMessages = false,
        CanChangeInfo = false,
        CanSendOtherMessages = false,
        CanPinMessages = false,
        CanAddWebPagePreviews = false,
        CanInviteUsers = false,
        CanSendPolls = false,
    };
    public GroupCommands(ITelegramBotClient client) : base(client)
    {
        _textFilter = new TextFilter();
    }

    public async Task HandleGroupAsync(Message message, CancellationToken ct)
    {
        await FilterWordsAsync(message, ct);
    }
    private async Task FilterWordsAsync(Message message, CancellationToken ct)
    {
        if (message.Text is null || message.From is null)
            return;


        if (!_textFilter.IsBadSentence(message.Text))
            return;


        var group = ManagerConfig.Groups.SingleOrDefault(p => p.GroupId == message.Chat.Id);
        if (group is null)
            return;

        var status = "";
        if (group.MuteOnCurse)
        {
            await Client.RestrictChatMemberAsync(message.Chat.Id, message.From.Id, MutePermissions
                , DateTime.Now + group.MuteTime, ct);
            status = $"User Has Been Muted For {group.MuteTime.Humanize()}";
        }

        if (group.WarnOnCurse)
        {
            var user = await UserController.UpdateUserAsync(user =>
             {
                 user.Warns++;
             }, message.From.Id, ct);
            if (user is null)
                return;
            var msg = await Client.SendTextMessageAsync(message.Chat.Id, $"User @{message.From.Username} Received a warning", replyToMessageId: message.MessageId, cancellationToken: ct);
            if (user.Warns >= group.MaxWarns)
            {
                var stat = await Client.EditMessageTextAsync(msg.Chat.Id, msg.MessageId, $"{msg.Text}\nMax Warns Reached!",
                    cancellationToken: ct);
                if (group.BanOnMaxWarn)
                {
                    await Client.BanChatMemberAsync(message.Chat.Id, message.From.Id, cancellationToken: ct);
                    await Client.EditMessageTextAsync(stat.Chat.Id, stat.MessageId, $"{stat.Text}\nUser Has Been Banned!",
                        cancellationToken: ct);
                }

                if (group.MuteOnMaxWarn)
                {
                    await Client.RestrictChatMemberAsync(message.Chat.Id, message.From.Id, MutePermissions,
                        DateTime.Now + group.MuteTime, ct);
                    await Client.EditMessageTextAsync(stat.Chat.Id, stat.MessageId, $"{stat.Text}\nUser Has Been Muted!",
                        cancellationToken: ct);
                }
            }
        }

        if (group.BanOnCurse)
        {
            await Client.BanChatMemberAsync(message.Chat.Id, message.From.Id, cancellationToken: ct);
            status = "User Has Been Banned";
        }

        await Client.SendTextMessageAsync(message.Chat.Id, $"{status}\n<b>Watch Your Words Please!</b>",
            ParseMode.Html, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

}