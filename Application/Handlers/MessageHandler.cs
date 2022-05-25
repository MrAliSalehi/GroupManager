using GroupManager.Application.Commands;
using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
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
        if (message.Chat.Id == message.From.Id)
            return;

        try
        {
            if (RegPatterns.Is.AdminBotCommand(message.Text))
                await AdminCommandsAsync(message, ct).ConfigureAwait(false);

            if (RegPatterns.Is.MemberBotCommand(message.Text))
                await MemberCommandsAsync(message, ct).ConfigureAwait(false);

            var group = await GroupController.GetGroupByIdIncludeChannelAsync(message.Chat.Id, ct).ConfigureAwait(false);
            if (group is null)
                return;

            await UserController.TryAddUserAsync(message.From.Id, ct).ConfigureAwait(false);

            await _groupCommands.HandleGroupAsync(message, group, ct).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(InitHandlerAsync));
        }
    }

    private async Task AdminCommandsAsync(Message message, CancellationToken ct)
    {
        if (message.From is null)
            return;


        var command = RegPatterns.Get.AdminBotCommand(message.Text);
        if (command is null)
            return;

        var group = await GroupController.GetGroupByIdWithAdminsAsync(message.Chat.Id, ct).ConfigureAwait(false);

        if (!(group?.Admins.Any(x => x.UserId == message.From.Id) ?? false) && !ManagerConfig.Admins.Contains(message.From.Id))
        {
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        if (ManagerConfig.Admins.Contains(message.From.Id))
        {
            var baseAdminResponse = command.Value.Replace("!!", "") switch
            {
                "remove gp" => _adminBotCommands.RemoveGroupAsync(message, ct),
                "add gp" => _adminBotCommands.AddGroupAsync(message, ct),
                "admins" => _adminBotCommands.AdminListAsync(message, ct),
                { } x when (x.StartsWith("set admin")) => _adminBotCommands.SetAdminAsync(message, ct),
                { } x when (x.StartsWith("rem admin")) => _adminBotCommands.RemoveAdminAsync(message, ct),
                _ => Task.CompletedTask
            };
            await baseAdminResponse.ConfigureAwait(false);
        }

        _adminBotCommands.CurrentGroup = group;

        var response = command.Value.Replace("!!", "") switch
        {
            "help" => _adminBotCommands.HelpAsync(message, ct),
            "sett" => _adminBotCommands.SettingAsync(message, ct),
            "is active" => _adminBotCommands.IsActiveAsync(message, ct),

            "enable welcome" => _adminBotCommands.EnableWelcomeAsync(message, ct),
            "disable welcome" => _adminBotCommands.DisableWelcomeAsync(message, ct),
            { } x when (x.StartsWith("set welcome")) => _adminBotCommands.SetWelcomeAsync(message, ct),

            { } x when (x.StartsWith("unban")) => _adminBotCommands.UnBanUserAsync(message, ct),
            { } x when (x.StartsWith("ban")) => _adminBotCommands.BanUserAsync(message, ct),

            "enable force" => _adminBotCommands.EnableForceJoinAsync(message, ct),
            "disable force" => _adminBotCommands.DisableForceJoinAsync(message, ct),
            "list force" => _adminBotCommands.GetListOfForceJoinAsync(message, ct),
            { } x when (x.Contains("add force")) => _adminBotCommands.AddForceJoinAsync(message, ct),
            { } x when (x.Contains("rem force")) => _adminBotCommands.RemoveForceJoinAsync(message, ct),

            "mute all" => _adminBotCommands.MuteAllChatAsync(message, ct),
            "unmute all" => _adminBotCommands.UnMuteAllChatAsync(message, ct),
            { } x when (x.StartsWith("mute")) => _adminBotCommands.MuteUserAsync(message, ct),
            { } x when (x.StartsWith("unmute")) => _adminBotCommands.UnMuteUserAsync(message, ct),

            { } x when (x.Contains("set tbm")) => _adminBotCommands.SetTimeBasedMuteAsync(message, ct),
            "enable tbm" => _adminBotCommands.EnableTimeBasedMuteAsync(message, ct),
            "disable tbm" => _adminBotCommands.DisableTimeBasedMuteAsync(message, ct),
            { } x when (x.Contains("monitor tbm")) => _adminBotCommands.MonitorTbmAsync(message, ct),

            { } x when (x.Contains("set ml")) => _adminBotCommands.SetMessageLimitPerUserDayAsync(message, ct),
            "enable ml" => _adminBotCommands.EnableMessageLimitAsync(message, ct),
            "disable ml" => _adminBotCommands.DisableMessageLimitAsync(message, ct),

            { } x when (x.Contains("set flood")) => _adminBotCommands.FloodSettingsAsync(message, ct),
            "enable flood" => _adminBotCommands.EnableFloodAsync(message, ct),
            "disable flood" => _adminBotCommands.DisableFloodAsync(message, ct),

            { } x when (x.StartsWith("set ms")) => _adminBotCommands.SetMessageSizeLimitAsync(message, ct),
            "enable ms" => _adminBotCommands.EnableMessageSizeLimitAsync(message, ct),
            "disable ms" => _adminBotCommands.DisableMessageSizeLimitAsync(message, ct),

            { } x when (x.StartsWith("filter")) => _adminBotCommands.AddNewFilterWordAsync(message, ct),

            { } x when (x.StartsWith("set media limit")) => _adminBotCommands.SetMediaLimitAsync(message, ct),
            "enable media limit" => _adminBotCommands.EnableMediaLimitAsync(message, ct),
            "disable media limit" => _adminBotCommands.DisableMediaLimitAsync(message, ct),

            { } x when (x.StartsWith("set lang")) => _adminBotCommands.SetLanguageAsync(message, ct),
            "enable lang" => _adminBotCommands.EnableLanguageLimitAsync(message, ct),
            "disable lang" => _adminBotCommands.DisableLanguageLimitAsync(message, ct),

            _ => Task.CompletedTask
        };
        await response.ConfigureAwait(false);
    }

    private async Task MemberCommandsAsync(Message message, CancellationToken ct)
    {
        var command = RegPatterns.Get.MemberBotCommand(message.Text);
        if (command is null)
            return;
        var response = command.Value.Replace("!", "") switch
        {
            "me" => _memberBotCommands.MeAsync(message, ct),
            "time" => _memberBotCommands.TimeAsync(message, ct),
            "ban me" => _memberBotCommands.BenMeAsync(message, ct),
            _ => Task.CompletedTask
        };
        await response.ConfigureAwait(false);
    }

}