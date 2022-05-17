using System.Timers;
using GroupManager.Common.Models;
using Telegram.Bot;

namespace GroupManager.Application.Commands;

public class SpamCommands
{
    private List<MiniUser> Users { get; } = new();

    private readonly ITelegramBotClient _client;

    private event EventHandler<MiniUser> MaxMessageReached;
    private void OnMaxMessageReached(MiniUser e) => MaxMessageReached?.Invoke(null, e);

    public SpamCommands(ITelegramBotClient client)
    {
        _client = client;

        var timer = new BetterTimer(ResetUserOnTimer)
        {
            Interval = 1000 * 10,
            AutoReset = true,
        };
        MaxMessageReached += OnMaxMessageReached;
        timer.Start();
    }



    private void ResetUserOnTimer(object? sender, ElapsedEventArgs e)
    {
        var expiredUsers = Users.Where(p => TimeSpan.Compare(e.SignalTime.TimeOfDay, p.ExpireTime) is 1 or 0).ToList();

        foreach (var user in expiredUsers)
        {
            Users.Remove(user);
        }

    }

    internal void AddUserOrIncreaseMessageCount(long userId, long chatId)
    {
        var getUser = Users.SingleOrDefault(p => p.UserId == userId && p.ChatId == chatId);
        if (getUser is null)
        {
            var user = new MiniUser
            {
                MessageCount = 0,
                UserId = userId,
                ChatId = chatId,
                IsAlreadyMuted = 0
            };

            Users.Add(user);
        }
        else
        {
            getUser.MessageCount++;
            if (getUser.MessageCount > 5)
            {
                OnMaxMessageReached(getUser);
            }
        }
    }

    private async void OnMaxMessageReached(object? sender, MiniUser e)
    {
        if (e.IsAlreadyMuted is 1)
            return;
        try
        {
            await _client.RestrictChatMemberAsync(e.ChatId, e.UserId, Globals.MutePermissions, DateTime.Now.AddDays(366));
            await _client.SendTextMessageAsync(e.ChatId, $"User {e.UserId} has been muted due To Spamming");
            e.IsAlreadyMuted = 1;
        }
        catch (Exception)
        {
            await _client.SendTextMessageAsync(e.ChatId, $"Cant Restrict User Right Now\n There Might Me Some Permission Issues here!");
        }

    }

}
