using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Mosaik.Core;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Services;

public class LanguageService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private static readonly List<Group> Groups = new();
    private readonly TextFilter _textFilter;
    public LanguageService()
    {
        _textFilter = new TextFilter();
        _bot = new TelegramBotClient(Globals.BotConfigs.Token);
    }

    public static async Task ReloadGroupsAsync(CancellationToken ct = default)
    {
        var group = await GroupController.GetAllGroupsAsync(ct);
        Groups.AddRange(group.Where(p => p.LanguageLimit));
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        UpdateService.Update += OnUpdate;
        await base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.CompletedTask;
    }

    private async void OnUpdate(object? sender, Update e)
    {
        if (e.Type != UpdateType.Message)
            return;

        if (e.Message?.Text is null)
            return;

        if (e.Message?.From is null)
            return;

        if (e.Message.Chat.Id == e.Message.From.Id)
            return;

        if (ManagerConfig.Admins.Contains(e.Message.From.Id))
            return;

        var currentGroup = Groups.FirstOrDefault(p => p.GroupId == e.Message.Chat.Id);

        if (currentGroup is null)
            return;

        var allowedLang = currentGroup.AllowedLanguage;
        var lang = await _textFilter.DetectAsync(e.Message.Text);
        if (lang is Language.English || lang == allowedLang)
            return;


        await _bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);

        await Task.CompletedTask;
    }

}