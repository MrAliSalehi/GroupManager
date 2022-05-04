using System.Text.RegularExpressions;
using GroupManager.Application.Commands;
using GroupManager.Application.Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupManager.Application.Handlers;

public class MessageHandler : HandlerBase
{
    private readonly AdminBotCommands _adminBotCommands;
    private readonly MemberBotCommands _memberBotCommands;
    public MessageHandler(ITelegramBotClient client) : base(client)
    {
        _adminBotCommands = new AdminBotCommands(client);
        _memberBotCommands = new MemberBotCommands(client);
    }
    public async Task InitHandlerAsync(Message message, CancellationToken ct)
    {
        if (RegPatterns.Is.AdminBotCommand(message.Text))
            await AdminCommandsAsync(message, ct);

        if (RegPatterns.Is.MemberBotCommand(message.Text))
            await MemberCommandsAsync(message, ct);

    }

    private async Task AdminCommandsAsync(Message message, CancellationToken ct)
    {
        var command = RegPatterns.Get.AdminBotCommand(message.Text);
        if (command is null)
            return;

        var response = command.Value.Replace("!!", "") switch
        {
            "is active" => _adminBotCommands.IsActiveAsync(message, ct),
            "remove gp" => _adminBotCommands.RemoveGroupAsync(message, ct),
            "add gp" => _adminBotCommands.AddGroupAsync(message, ct),
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
    }
}