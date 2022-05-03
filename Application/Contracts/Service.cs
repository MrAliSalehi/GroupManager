using Telegram.Bot;

namespace GroupManager.Application.Contracts;

public abstract class Service
{
    protected Service(ITelegramBotClient client)
    {
        Client = client;
    }

    protected ITelegramBotClient Client { get; }
}