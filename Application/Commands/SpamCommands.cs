//using System.Timers;
//using GroupManager.Common.Models;
//using GroupManager.DataLayer.Models;
//using Telegram.Bot;
//using Telegram.Bot.Types;

//namespace GroupManager.Application.Commands;

//public class SpamCommands
//{
//    private List<MiniUser> Users { get; } = new();

//    private event EventHandler<MiniUser> MaxMessageReached;
//    private void OnMaxMessageReached(MiniUser e) => MaxMessageReached?.Invoke(null, e);
//    private readonly FloodSettings _settings;
//    private readonly BetterTimer _timer;
//    public SpamCommands(FloodSettings settings)
//    {
//        this._settings = settings;

//        _timer = new BetterTimer(ResetUserOnTimer)
//        {
//            Interval = 1000 * settings.Interval,
//            AutoReset = true,
//        };
//        MaxMessageReached += OnMaxMessageReached;
//    }


//    internal void StartTimer()
//    {
//        try
//        {
//            _timer.Start();
//            Log.Information("timer Started");
//        }
//        catch (Exception e)
//        {
//            Log.Error(e, "FloodTimer StartTimer");
//        }
//    }

//    private void ResetUserOnTimer(object? sender, ElapsedEventArgs e)
//    {
//        var expiredUsers = Users.Where(p => TimeSpan.Compare(e.SignalTime.TimeOfDay, p.ExpireTime) is 1 or 0).ToList();

//        foreach (var user in expiredUsers)
//        {
//            Users.Remove(user);
//            Log.Information("User {user} Removed From Cache", user.UserId);
//        }

//    }

//    internal void AddUserOrIncreaseMessageCount(long userId, long chatId)
//    {
//        if (ManagerConfig.Admins.Contains(userId))
//            return;
//        var getUser = Users.SingleOrDefault(p => p.UserId == userId && p.ChatId == chatId);
//        if (getUser is null)
//        {
//            var user = new MiniUser(_settings.Interval)
//            {
//                MessageCount = 0,
//                UserId = userId,
//                ChatId = chatId,
//                IsAlreadyMuted = 0
//            };

//            Users.Add(user);
//        }
//        else
//        {
//            getUser.MessageCount++;
//            Log.Information("User {user} ++ message", getUser.UserId);

//            if (getUser.MessageCount >= _settings.MessageCountPerInterval)
//            {
//                OnMaxMessageReached(getUser);
//            }
//        }
//    }

//    private async void OnMaxMessageReached(object? sender, MiniUser e)
//    {
//        Log.Information("User {user} on max message reached", e.UserId);
//        if (e.IsAlreadyMuted is 1)
//            return;
//        try
//        {
//            if (_settings.MuteOnDetect)
//            {
//                await ManagerConfig.Client.RestrictChatMemberAsync(e.ChatId, e.UserId, Globals.MutePermissions, DateTime.Now.Add(_settings.RestrictTime));
//                await ManagerConfig.Client.SendTextMessageAsync(e.ChatId, $"User {e.UserId} has been muted due To Spamming");
//            }
//            if (_settings.BanOnDetect)
//            {
//                await ManagerConfig.Client.BanChatMemberAsync(e.ChatId, e.UserId, DateTime.Now.Add(_settings.RestrictTime));
//                await ManagerConfig.Client.SendTextMessageAsync(e.ChatId, $"User {e.UserId} has been Banned due To Spamming");

//            }
//            e.IsAlreadyMuted = 1;
//        }
//        catch (Exception)
//        {
//            await ManagerConfig.Client.SendTextMessageAsync(e.ChatId, $"Cant Restrict User Right Now\n There Might Me Some Permission Issues here!");
//        }

//    }

//}
