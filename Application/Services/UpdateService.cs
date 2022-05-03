using Serilog;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Services
{
    public class UpdateService : BackgroundService
    {
        private readonly TelegramBotClient _client;
        private readonly AdminService _adminService;
        public UpdateService()
        {
            _client = new TelegramBotClient(Globals.BotConfigs.Token);
            _adminService = new AdminService(_client);
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _client.GetMeAsync(cancellationToken);
            Log.Information("Bot Started With : {0}", me.Username);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _client.ReceiveAsync(OnUpdate, OnError, cancellationToken: stoppingToken, receiverOptions: new ReceiverOptions() { Offset = 0 });
                await Task.Delay(300, stoppingToken);
            }
        }

        private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            var updateHandler = update.Type switch
            {
                UpdateType.Message => Task.CompletedTask,
                UpdateType.MyChatMember when (update.MyChatMember is not null) => _adminService.InitAsync(update.MyChatMember, ct),
                UpdateType.Unknown => Task.CompletedTask,
                UpdateType.ChatMember => Task.CompletedTask,
                UpdateType.ChatJoinRequest => Task.CompletedTask,
                _ => Task.CompletedTask
            };

            await updateHandler;

            await Task.CompletedTask;
        }

        private static Task OnError(ITelegramBotClient client, Exception ex, CancellationToken ct)
        {
            Log.Error(ex, "OnError");
            return Task.CompletedTask;
        }

    }
}