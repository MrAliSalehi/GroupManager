using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Telegram.Bot.Types.User;

namespace GroupManager.Application.Handlers;

public class ChatMemberHandler : HandlerBase, IBotCommand
{
    public Group? CurrentGroup { get; set; }


    public ChatMemberHandler(ITelegramBotClient client) : base(client)
    {
    }

    public async Task UserJoinedChatAsync(List<User> users, Chat chat, CancellationToken ct)
    {
        var group = await GroupController.GetGroupByIdAsync(chat.Id, ct);
        CurrentGroup = group;

        await SayWelcomeAsync(users, chat, ct);
        await ForceJoinUserListAsync(users, ct);

    }

    private async Task ForceJoinUserListAsync(List<User> users, CancellationToken ct)
    {
        if (CurrentGroup is null or { ForceJoin: false })
            return;
        var fullGroup = await GroupController.GetGroupByIdIncludeChannelAsync(CurrentGroup.GroupId, ct);
        if (fullGroup is null)
            return;

        foreach (var user in users)
        {

            var notJoined = await CheckUserChatMemberAsync(user.Id, fullGroup.ForceJoinChannel, Client, ct);
            if (notJoined.Count == 0)
                continue;

            await Client.RestrictChatMemberAsync(CurrentGroup.GroupId, user.Id, Globals.MuteUserChatPermissions,
                cancellationToken: ct);
            var channelsText = "";
            notJoined.ForEach(ch =>
            {
                channelsText += $"@{ch.ChannelId.Trim()}\n";
            });
            await Client.SendTextMessageAsync(CurrentGroup.GroupId,
                $"User @{user.Username}\nYou Are Not Joined In Out Channels\nPlease Join First And Then Confirm The Button\n{channelsText}",
                replyMarkup: InlineButtons.Member.CreateForceJoinMarkup(notJoined, user.Id),
                cancellationToken: ct);
        }

    }

    internal static async ValueTask<List<ForceJoinChannel>> CheckUserChatMemberAsync(
        long userId,
        IEnumerable<ForceJoinChannel> channels,
        ITelegramBotClient client,
        CancellationToken ct)
    {
        var notJoined = new List<ForceJoinChannel>();
        foreach (var channel in channels)
        {
            try
            {

                var chatMember = await client.GetChatMemberAsync($"@{channel.ChannelId}", userId, ct);
                if (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator
                    or ChatMemberStatus.Member)
                    continue;
                notJoined.Add(channel);
            }
            catch (Exception)
            {
                notJoined.Add(channel);
            }
        }
        return notJoined;

    }
    private async Task SayWelcomeAsync(List<User> users, Chat chat, CancellationToken ct)
    {
        if (CurrentGroup is null or { SayWelcome: false })
            return;


        foreach (var user in users)
        {
            await Client.SendTextMessageAsync(chat.Id, $"User @{user.Username}\n{CurrentGroup.WelcomeMessage}", cancellationToken: ct);
        }
    }

}