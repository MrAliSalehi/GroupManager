using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupManager.Application.Commands;

public class AdminBotCommands : HandlerBase
{
    public AdminBotCommands(ITelegramBotClient client) : base(client)
    {
    }
    internal async Task IsActiveAsync(Message message, CancellationToken ct)
    {
        var groups = await GroupController.GetAllGroupsAsync(ct);
        var response = groups.Any(p => p.GroupId == message.Chat.Id)
            ? "Bot Is Active Here"
            : "Bot Is Not Active in This group";
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct);
    }

    internal async Task RemoveGroupAsync(Message message, CancellationToken ct)
    {
        var group = await GroupController.GetGroupByIdAsync(message.Chat.Id, ct);
        if (group is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Registered For Bot",
                cancellationToken: ct);
            return;
        }

        var result = await GroupController.RemoveGroupAsync(message.Chat.Id, ct);
        ManagerConfig.Groups.Remove(group);
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
        var group = await GroupController.GetGroupByIdAsync(message.Chat.Id, ct);
        if (group is not null)
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
        ManagerConfig.Groups.Add(addedGp);
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
}