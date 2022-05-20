using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Services;

public class MediaLimitService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private static readonly List<Group> Groups = new();
    public MediaLimitService()
    {
        _bot = new TelegramBotClient(Globals.BotConfigs.Token);
    }

    public static async Task ReLoadGroupsAsync(CancellationToken ct = default)
    {
        var groups = await GroupController.GetAllGroupsAsync(ct);
        Groups.AddRange(groups.Where(p => p.LimitMedia));
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await ReLoadGroupsAsync(cancellationToken);
        UpdateService.Update += Update;
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.CompletedTask;
    }

    private async void Update(object? sender, Update e)
    {
        if (e.Message?.From is null)
            return;

        if (e.Message.Chat.Id == e.Message.From.Id)
            return;

        if (ManagerConfig.Admins.Contains(e.Message.From.Id))
            return;
        var currentGroup = Groups.FirstOrDefault(p => p.GroupId == e.Message.Chat.Id);

        var user = await UserController.GetUserByIdAsync(e.Message.From.Id);
        if (user is null)
            return;


        if (currentGroup is null)
            return;

        switch (e.Message.Type)
        {
            case MessageType.Sticker:
                {
                    if (user.SentStickers <= currentGroup.StickerLimits)
                    {
                        await UserController.UpdateUserAsync(p => p.SentStickers++, e.Message.From.Id);
                    }
                    else
                    {
                        await _bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
                    }
                    break;
                }
            case MessageType.Photo:
                {
                    if (user.SentPhotos <= currentGroup.PhotoLimits)
                    {
                        await UserController.UpdateUserAsync(p => p.SentPhotos++, e.Message.From.Id);
                    }
                    else
                    {
                        await _bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
                    }
                    break;
                }
            case MessageType.Video:
                {
                    if (user.SentVideos <= currentGroup.VideoLimits)
                    {
                        await UserController.UpdateUserAsync(p => p.SentVideos++, e.Message.From.Id);
                    }
                    else
                    {
                        await _bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
                    }
                    break;
                }
        }

        if (e.Message.Animation is not null)
        {
            if (user.SentGif <= currentGroup.GifLimits)
            {
                await UserController.UpdateUserAsync(p => p.SentGif++, e.Message.From.Id);
            }
            else
            {
                await _bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
            }
        }
    }
}