using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupManager.Application.Commands;

public class AdminBotCommands : HandlerBase, IBotCommand
{

    public Group? CurrentGroup { get; set; }

    public AdminBotCommands(ITelegramBotClient client) : base(client)
    {

    }

    internal async Task IsActiveAsync(Message message, CancellationToken ct)
    {

        var response = CurrentGroup is null
            ? "Bot Is Active Here"
            : "Bot Is Not Active in This group";
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct);
    }

    internal async Task RemoveGroupAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Registered For Bot",
                cancellationToken: ct);
            return;
        }

        var result = await GroupController.RemoveGroupAsync(message.Chat.Id, ct);
        var response = result switch
        {
            0 => "Group Has Been Removed",
            1 => "Group Is Not Already In List",
            2 => "There Is Some Issues With Storage",
            _ => "cant get any response"
        };
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct);
    }

    internal async Task AddGroupAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is not null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Already added", cancellationToken: ct);
            return;
        }

        var addedGp = await GroupController.AddGroupAsync(message.Chat.Id, ct);
        if (addedGp is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Add Group Right Now", cancellationToken: ct);
            return;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, "Group Has Been Added To Bot", cancellationToken: ct);
    }

    internal async Task SettingAsync(Message message, CancellationToken ct)
    {
        if (message.From is null)
            return;
        await Client.SendTextMessageAsync(message.Chat.Id, ConstData.MessageOfMainMenu, replyMarkup: InlineButtons.Admin.SettingMenu,
            cancellationToken: ct, replyToMessageId: message.MessageId);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task SetWelcomeAsync(Message message, CancellationToken ct)
    {
        try
        {
            var messageToUpdate = message.Text?.Replace("!!set welcome", "");

            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.WelcomeMessage = messageToUpdate;
            }, message.Chat.Id, ct);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", cancellationToken: ct);
                return;
            }

            await Client.SendTextMessageAsync(message.Chat.Id, $"Welcome Message Set To :\n({messageToUpdate})", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(SetWelcomeAsync));
        }
    }

    internal async Task DisableWelcomeAsync(Message message, CancellationToken ct)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.SayWelcome = false;
            }, message.Chat.Id, ct);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

                return;
            }

            await Client.SendTextMessageAsync(message.Chat.Id, "Welcome Message Disabled!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(DisableWelcomeAsync));
        }

    }

    internal async Task EnableWelcomeAsync(Message message, CancellationToken ct)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.SayWelcome = true;

            }, message.Chat.Id, ct);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Welcome Message Enabled!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(EnableWelcomeAsync));
        }

    }

    internal async Task UnBanUserAsync(Message message, CancellationToken ct)
    {
        try
        {
            var canParse = long.TryParse(message.Text?.Replace("!!unban", ""), out var userIdFromCommand);
            if (!canParse)
            {
                if (message.ReplyToMessage?.From is null)
                    return;

                userIdFromCommand = message.ReplyToMessage.From.Id;
            }

            if (userIdFromCommand is 0)
                return;
            await UserController.UpdateUserAsync(p => { p.IsBanned = false; }, userIdFromCommand, ct);

            await Client.UnbanChatMemberAsync(message.Chat.Id, userIdFromCommand, true, ct);
            await Client.SendTextMessageAsync(message.Chat.Id, $"User {userIdFromCommand} Has Been Unbanned", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UnBanUserAsync));
        }
    }

    internal async Task BanUserAsync(Message message, CancellationToken ct)
    {
        try
        {
            var canParse = long.TryParse(message.Text?.Replace("!!ban", ""), out var userIdFromCommand);
            if (!canParse)
            {
                if (message.ReplyToMessage?.From is null)
                    return;

                userIdFromCommand = message.ReplyToMessage.From.Id;
            }

            if (userIdFromCommand is 0)
                return;

            await UserController.UpdateUserAsync(p => { p.IsBanned = true; }, userIdFromCommand, ct);
            await Client.BanChatMemberAsync(message.Chat.Id, userIdFromCommand, cancellationToken: ct);
            await Client.SendTextMessageAsync(message.Chat.Id, $"User {userIdFromCommand} Has Been banned", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UnBanUserAsync));
        }
    }

    internal async Task EnableForceJoinAsync(Message message, CancellationToken ct)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p => { p.ForceJoin = true; }, message.Chat.Id, ct);

            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Force Join Enabled", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(EnableForceJoinAsync));
        }
    }

    internal async Task DisableForceJoinAsync(Message message, CancellationToken ct)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p => { p.ForceJoin = false; }, message.Chat.Id, ct);

            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Force Join Disabled", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(DisableForceJoinAsync));
        }
    }

    internal async Task AddForceJoinAsync(Message message, CancellationToken ct)
    {
        try
        {
            var channelId = message.Text?.Replace("!!add force", "");
            if (channelId is null or "" or " ")
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Channel Id Is Wrong!", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            await ForceJoinController.AddChannelAsync(CurrentGroup.Id, channelId.Trim(), ct);


            await Client.SendTextMessageAsync(message.Chat.Id, $"Channel @{channelId?.Replace("@", "")} Added To Force Join List.", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddForceJoinAsync));
        }
    }

    internal async Task RemoveForceJoinAsync(Message message, CancellationToken ct)
    {
        try
        {
            var channelId = message.Text?.Replace("!!add force", "");
            if (channelId is null or "" or " ")
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Channel Id Is Wrong!", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }
            var result = await ForceJoinController.RemoveChannelAsync(CurrentGroup.Id, channelId.Replace("!!rem force ", ""), ct);
            var outputText = result switch
            {
                0 => "Channel Has been Removed",
                1 => "Channel Not Found",
                2 => "There Is Some Issues during Remove Channel Operation",
                _ => "-",
            };
            await Client.SendTextMessageAsync(message.Chat.Id, outputText, replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddForceJoinAsync));
        }
    }

    internal async Task GetListOfForceJoinAsync(Message message, CancellationToken ct)
    {
        try
        {
            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            var channelsList = await ForceJoinController.GetAllChannelsAsync(CurrentGroup.Id, ct);
            if (channelsList is null or { Count: 0 })
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "No Channel Found!", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            var outputText = "";
            channelsList.ForEach(p =>
            {
                outputText += $"@{p.ChannelId}\n";
            });
            await Client.SendTextMessageAsync(message.Chat.Id, $"List Of All Force-Join-Channels:\n{outputText}",
                cancellationToken: ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetListOfForceJoinAsync));
        }
    }

    internal async Task MuteAllChatAsync(Message message, CancellationToken ct)
    {

        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        try
        {
            await Client.SetChatPermissionsAsync(message.Chat.Id, Globals.MutePermissions, ct);

            await Client.SendTextMessageAsync(message.Chat.Id, "Group Has been Muted", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Bot Doesn't Have Permissions To Do This\nOr Just Can Do it Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

    }

    public async Task UnMuteAllChatAsync(Message message, CancellationToken ct)
    {

        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        try
        {
            var chat = await Client.GetChatAsync(message.Chat.Id, ct);

            await Client.SetChatPermissionsAsync(message.Chat.Id, Globals.UnMutePermissions, ct);
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Has been UnMuted", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Bot Doesn't Have Permissions To Do This\nOr Just Can Do it Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

    }
}