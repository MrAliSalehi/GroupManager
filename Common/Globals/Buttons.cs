using Telegram.Bot.Types.ReplyMarkups;

namespace GroupManager.Common.Globals;

public struct InlineButtons
{
    public struct Admin
    {
        public static readonly InlineKeyboardMarkup ConfirmChat =
            new(InlineKeyboardButton.WithCallbackData("Confirm Chat", $"{nameof(Admin)}:{nameof(ConfirmChat)}"));
    }
}