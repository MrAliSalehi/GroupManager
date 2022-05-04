using Telegram.Bot;

namespace GroupManager.Application.Contracts;

public abstract class HandlerBase
{
    protected HandlerBase(ITelegramBotClient client)
    {
        Client = client;
    }

    protected ITelegramBotClient Client { get; }
}