using GroupManager.Application.Commands;
using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Humanizer;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WordFilter;

namespace GroupManager.Application.Handlers;

public class MessageHandler : HandlerBase
{
    private readonly AdminBotCommands _adminBotCommands;
    private readonly MemberBotCommands _memberBotCommands;
    private readonly GroupCommands _groupCommands;


    public MessageHandler(ITelegramBotClient client) : base(client)
    {
        _adminBotCommands = new AdminBotCommands(client);
        _memberBotCommands = new MemberBotCommands(client);
        _groupCommands = new GroupCommands(client);
    }
    public async Task InitHandlerAsync(Message message, CancellationToken ct)
    {
        if (message.From is null)
            return;

        await UserController.TryAddUserAsync(message.From.Id, ct);

        if (RegPatterns.Is.AdminBotCommand(message.Text))
            await AdminCommandsAsync(message, ct);

        if (RegPatterns.Is.MemberBotCommand(message.Text))
            await MemberCommandsAsync(message, ct);

        if (message.Chat.Id != message.From.Id)
        {
            var group = await GroupController.GetGroupByIdAsync(message.Chat.Id, ct);
            if (group is not null)
                await _groupCommands.HandleGroupAsync(message, group, ct);
        }
    }

    private async Task AdminCommandsAsync(Message message, CancellationToken ct)
    {
        if (message.From is null)
            return;

        if (!ManagerConfig.Admins.Contains(message.From.Id))
        {
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var command = RegPatterns.Get.AdminBotCommand(message.Text);
        if (command is null)
            return;

        var response = command.Value.Replace("!!", "") switch
        {
            "is active" => _adminBotCommands.IsActiveAsync(message, ct),
            "remove gp" => _adminBotCommands.RemoveGroupAsync(message, ct),
            "add gp" => _adminBotCommands.AddGroupAsync(message, ct),
            "sett" => _adminBotCommands.SettingAsync(message, ct),
            _ => Task.CompletedTask
        };
        await response;
    }

    private async Task MemberCommandsAsync(Message message, CancellationToken ct)
    {
        var command = RegPatterns.Get.MemberBotCommand(message.Text);
        if (command is null)
            return;
        var response = command.Value.Replace("!", "") switch
        {
            "me" => _memberBotCommands.MeAsync(message, ct),
            _ => Task.CompletedTask
        };
        await response;
    }

}