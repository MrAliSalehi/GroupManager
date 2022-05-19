using GroupManager.Application.Contracts;
using GroupManager.Application.Handlers;
using GroupManager.Application.RecurringJobs;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Hangfire;
using Humanizer;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using WordFilter;

namespace GroupManager.Application.Commands;

public class GroupCommands : HandlerBase, IBotCommand
{
    private readonly TextFilter _textFilter;


    public Group? CurrentGroup { get; set; }
    public GroupCommands(ITelegramBotClient client) : base(client)
    {
        _textFilter = new TextFilter();

    }

    public async Task HandleGroupAsync(Message message, Group group, CancellationToken ct)
    {
        CurrentGroup = group;
        var tasks = new List<Task>
        {
            FilterWordsAsync(message, ct),
            CheckForceJoinAsync(message, ct),
            CheckUserMessageLimitAsync(message, ct),
            //CheckAntiSpamAsync(message),
        };

        await Task.WhenAll(tasks);

    }

    //private async Task CheckAntiSpamAsync(Message message)
    //{
    //    if (CurrentGroup is null || message.From is null)
    //    {
    //        await Task.CompletedTask;
    //        return;
    //    }

    //    var canGetValue = ManagerConfig.SpamCommands.TryGetValue(message.Chat.Id, out var spam);
    //    if (canGetValue || spam is null)
    //        return;

    //    spam.AddUserOrIncreaseMessageCount(message.From.Id, message.Chat.Id);
    //    await Task.CompletedTask;
    //}

    private async Task CheckUserMessageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null or { EnableMessageLimitPerUser: false } || message.From is null)
            return;

        if (ManagerConfig.Admins.Contains(message.From.Id))
            return;

        var updatedUser = await UserController.UpdateUserAsync(usr => usr.MessageCount++, message.From.Id, ct)
                          ?? await UserController.TryAddUserAsync(message.MessageId, ct);

        if (updatedUser.MessageCount >= CurrentGroup.MaxMessagePerUser)
        {
            var unMuteTime = DateTime.Now.AddDays(1);
            BackgroundJob.Schedule<ResetMessageLimit>((reset) => reset.ResetUserLimitAsync(message.From.Id, ct), unMuteTime);

            await Client.SendTextMessageAsync(message.Chat.Id,
                $"User @{message.From.Username}\nYou Reached <b>Max</b> Message ({CurrentGroup.MaxMessagePerUser}) Per Day!\n" +
                $"This Value Will Reset Tomorrow In:<b>{unMuteTime:G}</b>\n(<i>{unMuteTime.TimeOfDay.Humanize(6)}</i>)",
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: ct);
            await UserController.UpdateUserAsync(usr => usr.MessageCount = 0, message.From.Id, ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            await Client.RestrictChatMemberAsync(message.Chat.Id, message.From.Id, Globals.MutePermissions, unMuteTime.AddHours(2), ct);

        }

    }

    private async Task FilterWordsAsync(Message message, CancellationToken ct = default)
    {
        if (message.Text is null || message.From is null || CurrentGroup is null)
            return;

        if (!_textFilter.IsBadSentence(message.Text))
            return;


        var status = "";
        if (CurrentGroup.MuteOnCurse)
        {
            await Client.RestrictChatMemberAsync(message.Chat.Id, message.From.Id, Globals.MutePermissions
                , DateTime.Now + CurrentGroup.MuteTime, ct);
            status = $"User Has Been Muted For {CurrentGroup.MuteTime.Humanize()}";
        }

        if (CurrentGroup.WarnOnCurse)
        {
            var user = await UserController.UpdateUserAsync(user =>
             {
                 user.Warns++;
             }, message.From.Id, ct);
            if (user is null)
                return;
            var msg = await Client.SendTextMessageAsync(message.Chat.Id, $"User @{message.From.Username} Received a warning", replyToMessageId: message.MessageId, cancellationToken: ct);
            if (user.Warns >= CurrentGroup.MaxWarns)
            {
                var stat = await Client.EditMessageTextAsync(msg.Chat.Id, msg.MessageId, $"{msg.Text}\nMax Warns Reached!",
                    cancellationToken: ct);
                if (CurrentGroup.BanOnMaxWarn)
                {
                    await UserController.UpdateUserAsync(p => { p.IsBanned = true; }, message.From.Id, ct);
                    await Client.BanChatMemberAsync(message.Chat.Id, message.From.Id, cancellationToken: ct);
                    await Client.EditMessageTextAsync(stat.Chat.Id, stat.MessageId, $"{stat.Text}\nUser Has Been Banned!",
                        cancellationToken: ct);
                }

                else if (CurrentGroup.MuteOnMaxWarn)
                {
                    await Client.RestrictChatMemberAsync(message.Chat.Id, message.From.Id, Globals.MutePermissions,
                        DateTime.Now + CurrentGroup.MuteTime, ct);
                    await Client.EditMessageTextAsync(stat.Chat.Id, stat.MessageId, $"{stat.Text}\nUser Has Been Muted!",
                        cancellationToken: ct);
                }
            }
        }

        if (CurrentGroup.BanOnCurse)
        {
            await Client.BanChatMemberAsync(message.Chat.Id, message.From.Id, cancellationToken: ct);
            status = "User Has Been Banned";
        }

        await Client.SendTextMessageAsync(message.Chat.Id, $"{status}\n<b>Watch Your Words Please!</b>",
            ParseMode.Html, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    private async Task CheckForceJoinAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null or { ForceJoin: false })
            return;
        if (message?.From is null)
            return;

        var notJoined = await ChatMemberHandler.CheckUserChatMemberAsync(message.From.Id, CurrentGroup.ForceJoinChannel, Client, ct);
        if (notJoined.Count is 0)
            return;

        await Client.RestrictChatMemberAsync(CurrentGroup.GroupId, message.From.Id, Globals.MutePermissions,
            cancellationToken: ct);
        var channelsText = "";
        notJoined.ForEach(ch =>
        {
            channelsText += $"@{ch.ChannelId.Trim()}\n";
        });
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        await Client.SendTextMessageAsync(CurrentGroup.GroupId,
            $"User @{message.From.Username}\nYou Are Not Joined In Out Channels\nPlease Join First And Then Confirm The Button\n{channelsText}",
            replyMarkup: InlineButtons.Member.CreateForceJoinMarkup(notJoined, message.From.Id),
            cancellationToken: ct);
    }

}