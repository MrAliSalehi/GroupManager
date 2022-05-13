using GroupManager.Application.Commands;
using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Telegram.Bot;
using Telegram.Bot.Types;

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

        var group = await GroupController.GetGroupByIdAsync(message.Chat.Id, ct);

        _adminBotCommands.CurrentGroup = group;

        var response = command.Value.Replace("!!", "") switch
        {
            "sett" => _adminBotCommands.SettingAsync(message, ct),
            "is active" => _adminBotCommands.IsActiveAsync(message, ct),

            "remove gp" => _adminBotCommands.RemoveGroupAsync(message, ct),
            "add gp" => _adminBotCommands.AddGroupAsync(message, ct),

            "enable welcome" => _adminBotCommands.EnableWelcomeAsync(message, ct),
            "disable welcome" => _adminBotCommands.DisableWelcomeAsync(message, ct),
            { } x when (x.Contains("set welcome")) => _adminBotCommands.SetWelcomeAsync(message, ct),

            { } x when (x.Contains("unban")) => _adminBotCommands.UnBanUserAsync(message, ct),
            { } x when (x.Contains("ban")) => _adminBotCommands.BanUserAsync(message, ct),

            "enable force" => _adminBotCommands.EnableForceJoinAsync(message, ct),
            "disable force" => _adminBotCommands.DisableForceJoinAsync(message, ct),
            { } x when (x.Contains("add force")) => _adminBotCommands.AddForceJoinAsync(message, ct),
            { } x when (x.Contains("rem force")) => _adminBotCommands.RemoveForceJoinAsync(message, ct),

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