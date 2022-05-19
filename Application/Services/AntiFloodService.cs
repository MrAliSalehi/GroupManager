using GroupManager.Common.Models;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Query;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupManager.Application.Services;

public class AntiFloodService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    public static List<FloodSettings> Settings = new();

    private readonly List<MiniUser> _users = new();
    private const int Interval = 10;
    public AntiFloodService()
    {
        _bot = new TelegramBotClient(Globals.BotConfigs.Token);
    }


    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadAntiFloodGroupsAsync(cancellationToken);

        UpdateService.Update += Update;

        await base.StartAsync(cancellationToken);
    }

    public static async Task LoadAntiFloodGroupsAsync(CancellationToken ct = default)
    {
        var settings = await FloodController.GetAllFloodEnabledGroupsAsync(ct);
        if (settings is null)
            return;

        Settings = settings;
    }

    private async void Update(object? sender, Update e)
    {
        if (e.Message?.From is null)
            return;

        if (ManagerConfig.Admins.Contains(e.Message.From.Id))
            return;

        var group = Settings.FirstOrDefault(p => p.Group.GroupId == e.Message.Chat.Id);
        if (group is null)
            return;

        if (!group.Enabled)
            return;

        var user = _users.SingleOrDefault(p => p.UserId == e.Message.From.Id);

        if (user is null)
        {
            var addUser = new MiniUser(group.Interval)
            {
                MessageCount = 1,
                UserId = e.Message.From.Id,
                ChatId = e.Message.Chat.Id,
                IsAlreadyMuted = 0
            };

            _users.Add(addUser);

        }
        else
        {
            user.MessageCount++;

            if (user.MessageCount < group.MessageCountPerInterval)
                return;

            await OnMaxMessageReached(user.UserId, group);
        }
    }
    private async Task OnMaxMessageReached(long userId, FloodSettings setting)
    {
        var user = _users.FirstOrDefault(p => p.UserId == userId);
        if (user is null)
            return;

        if (user.IsAlreadyMuted is 1)
            return;
        try
        {
            if (setting.MuteOnDetect)
            {
                await _bot.RestrictChatMemberAsync(user.ChatId, user.UserId, Globals.MutePermissions, DateTime.Now.Add(setting.RestrictTime));
                await _bot.SendTextMessageAsync(user.ChatId, $"User {user.UserId} has been muted due To Spamming");
            }
            if (setting.BanOnDetect)
            {
                await _bot.BanChatMemberAsync(user.ChatId, user.UserId, DateTime.Now.Add(setting.RestrictTime));
                await _bot.SendTextMessageAsync(user.ChatId, $"User {user.UserId} has been Banned due To Spamming");
            }
            user.IsAlreadyMuted = 1;
        }
        catch (Exception)
        {
            await _bot.SendTextMessageAsync(user.ChatId, $"Cant Restrict User Right Now\n There Might Me Some Permission Issues here!");
        }

    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_users.Count <= 0)
            {
                await Task.Delay(1000 * 2, stoppingToken);
                continue;
            }
            await Task.Delay(1000 * (Interval - 2), stoppingToken);
            var expiredUsers = _users.Where(p => TimeSpan.Compare(DateTime.Now.TimeOfDay, p.ExpireTime) is 1 or 0).ToList();

            foreach (var user in expiredUsers)
            {
                _users.Remove(user);
            }
        }
    }
}