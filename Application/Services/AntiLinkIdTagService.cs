using GroupManager.DataLayer.Controller;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Services;

public class AntiLinkIdTagService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    public AntiLinkIdTagService(ITelegramBotClient bot)
    {
        _bot = bot;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        UpdateService.Update += OnUpdate;
        await Task.CompletedTask;
    }

    private async void OnUpdate(object? sender, Update e)
    {

        if (e.Type != UpdateType.Message)
            return;
        try
        {
            await CheckTextAsync(e.Message);

        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error On AntiLink Service:{method}", nameof(OnUpdate));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.CompletedTask;
    }

    private async Task CheckTextAsync(Message? message)
    {


        if (message?.Text is null)
            return;

        if (message.From is null)
            return;

        if (message.Chat.Id == message.From.Id)
            return;

        if (ManagerConfig.Admins.Contains(message.From.Id))
            return;

        var group = await GroupController.GetGroupByIdAsync(message.Chat.Id);

        if (group is null)
            return;

        if (group.FilterTelLink)
        {
            var isTelegramLink = RegPatterns.Is.TelegramLink(message.Text);
            if (isTelegramLink)
                await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }

        if (group.FilterPublicLink)
        {
            var isPublicLink = RegPatterns.Is.PublicLink(message.Text);
            if (isPublicLink)
                await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);

        }

        if (group.FilterId)
        {
            var isId = RegPatterns.Is.Id(message.Text);
            if (isId)
                await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }

        if (group.FilterHashTag)
        {
            var isHashTag = RegPatterns.Is.HashTag(message.Text);
            if (isHashTag)
                await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }
    }
}