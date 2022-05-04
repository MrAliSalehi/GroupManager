using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Handlers;

public class CallBackHandler : HandlerBase
{
    public CallBackHandler(ITelegramBotClient client) : base(client)
    {
    }

    public async Task InitHandlerAsync(CallbackQuery callback, CancellationToken ct)
    {
        if (callback.Data is null or "")
            return;

        var dataArr = callback.Data.Split(':');
        switch (dataArr.First())
        {
            case "Admin":
                await AdminCallbacksAsync(callback, callback.Data.Replace("Admin:", ""), ct);
                break;

            default:
                break;
        }
        await Task.CompletedTask;
    }

    private async Task AdminCallbacksAsync(CallbackQuery callback, string data, CancellationToken ct)
    {
        if (!ManagerConfig.Admins.Contains(callback.From.Id))
        {
            await Client.AnswerCallbackQueryAsync(callback.Id, "You Are Not Admin of Bot!", cancellationToken: ct);
            return;
        }

        if (callback.Message is null)
            return;

        switch (data)
        {
            case nameof(InlineButtons.Admin.ConfirmChat):
                await GroupController.AddGroupAsync(callback.Message.Chat.Id, ct);
                await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, "<b>Bot is Now Active</b>", ParseMode.Html, cancellationToken: ct);

                break;
        }
    }
}