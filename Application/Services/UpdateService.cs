using GroupManager.Application.Handlers;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Services
{
    public class UpdateService : BackgroundService
    {
        private readonly TelegramBotClient _client;
        private readonly MyChatMemberHandler _myChatMemberHandler;
        private readonly MessageHandler _messageHandler;
        private readonly CallBackHandler _callBackHandler;
        public UpdateService()
        {
            _client = new TelegramBotClient(Globals.BotConfigs.Token);
            _myChatMemberHandler = new MyChatMemberHandler(_client);
            _messageHandler = new MessageHandler(_client);
            _callBackHandler = new CallBackHandler(_client);
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _client.GetMeAsync(cancellationToken);
            Log.Information("Bot Started With : {0}", me.Username);
            ManagerConfig.BotUserName = me.Username ?? "-";
            _client.StartReceiving(OnUpdate, OnError, cancellationToken: cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            var updateHandler = update.Type switch
            {
                UpdateType.Message when (update.Message is not null) => _messageHandler.InitHandlerAsync(update.Message, ct),
                UpdateType.ChatMember when (update.ChatMember is not null) => _myChatMemberHandler.InitHandlerAsync(update.ChatMember, ct),
                UpdateType.MyChatMember when (update.MyChatMember is not null) => _myChatMemberHandler.InitHandlerAsync(update.MyChatMember, ct),

                UpdateType.CallbackQuery when (update.CallbackQuery is not null) => _callBackHandler.InitHandlerAsync(update.CallbackQuery, ct),
                UpdateType.ChatJoinRequest => Task.CompletedTask,
                UpdateType.Unknown => Task.CompletedTask,
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