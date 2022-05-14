using System.Diagnostics.CodeAnalysis;
using GroupManager.DataLayer.Models;
using Humanizer;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupManager.Common.Globals;

public struct InlineButtons
{
    public static void ChangeButtonValue(string buttonText, InlineKeyboardMarkup keyboard, Action<InlineKeyboardButton> changesOnButton)
    {
        InlineKeyboardButton? button = null;
        foreach (var inline in keyboard.InlineKeyboard)
        {
            foreach (var inlineKeyboardButton in inline)
            {
                if (!inlineKeyboardButton.Text.Contains(buttonText))
                    continue;
                button = inlineKeyboardButton;
                goto Out;
            }
        }
    Out:
        if (button is not null)
            changesOnButton(button);
    }
    public struct Admin
    {

        public static readonly InlineKeyboardMarkup ConfirmChat = new(InlineKeyboardButton.WithCallbackData("Confirm Chat", $"{nameof(Admin)}:{nameof(ConfirmChat)}"));

        public static readonly InlineKeyboardMarkup SettingMenu = new(new[]
        {
            new[]
            {
                //Admin:SettingMenu:Warn
                InlineKeyboardButton.WithCallbackData("Warn Settings", $"{nameof(Admin)}:{nameof(SettingMenu)}:{nameof(Warn)}"),
                InlineKeyboardButton.WithCallbackData("Curse Settings", $"{nameof(Admin)}:{nameof(SettingMenu)}:{nameof(Curse)}"),
            },
            new[]
            {
                //Admin:SettingMenu:cls
                InlineKeyboardButton.WithCallbackData("Close Menu", $"{nameof(Admin)}:{nameof(SettingMenu)}:{ConstData.Close}")
            }


        });

        public struct Warn
        {
            public static InlineKeyboardMarkup GetMenu(Group group)
            {

                Menu.InlineKeyboard.First().First().Text = $"Max Warns {group.MaxWarns}";

                Menu.InlineKeyboard.ToList()[3].SingleOrDefault(p => p.Text.Contains("Ban User"))!.Text =
                    "Ban User " + (group.BanOnMaxWarn ? ConstData.TrueEmoji : ConstData.FalseEmoji);

                Menu.InlineKeyboard.ToList()[3].SingleOrDefault(p => p.Text.Contains("Mute User"))!.Text =
                    "Mute User " + (group.MuteOnMaxWarn ? ConstData.TrueEmoji : ConstData.FalseEmoji);
                return Menu;
            }


            private static readonly InlineKeyboardMarkup Menu = new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Max Warns 0", $"{ConstData.IgnoreMe}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("+", $"{nameof(Admin)}:{nameof(Warn)}:{ConstData.Plus}"),
                        InlineKeyboardButton.WithCallbackData("-", $"{nameof(Admin)}:{nameof(Warn)}:{ConstData.Minus}")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("On Max Warns:",ConstData.IgnoreMe),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Ban User ",$"{nameof(Admin)}:{nameof(Warn)}:{ConstData.Ban}"),
                        InlineKeyboardButton.WithCallbackData("Mute User ",$"{nameof(Admin)}:{nameof(Warn)}:{ConstData.Mute}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Back", $"{nameof(Admin)}:{nameof(Warn)}:{ConstData.Back}")
                    },
                });
        }

        public struct Curse
        {
            public static InlineKeyboardMarkup GetMenu(Group group)
            {
                var banStat = group.BanOnCurse ? ConstData.TrueEmoji : ConstData.FalseEmoji;
                var warnStat = group.WarnOnCurse ? ConstData.TrueEmoji : ConstData.FalseEmoji;
                var muteStat = group.MuteOnCurse ? ConstData.TrueEmoji : ConstData.FalseEmoji;
                Menu.InlineKeyboard.First().First().Text = $"Ban {banStat}";
                Menu.InlineKeyboard.First().ToList()[1].Text = $"Warn {warnStat}";
                Menu.InlineKeyboard.First().ToList()[2].Text = $"Mute {muteStat}";
                Menu.InlineKeyboard.ToList()[1].ToList()[1].Text = group.MuteTime.Humanize();
                return Menu;
            }
            public static readonly InlineKeyboardMarkup Menu = new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Ban ",$"{nameof(Admin)}:{nameof(Curse)}:{ConstData.Ban}"),
                        InlineKeyboardButton.WithCallbackData("Warn ",$"{nameof(Admin)}:{nameof(Curse)}:{ConstData.Warn}"),
                        InlineKeyboardButton.WithCallbackData("Mute ",$"{nameof(Admin)}:{nameof(Curse)}:{ConstData.Mute}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Mute Time:",ConstData.IgnoreMe),
                        InlineKeyboardButton.WithCallbackData("time here",ConstData.IgnoreMe),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Modify Time",$"{nameof(Admin)}:{nameof(Curse)}:{nameof(MuteTimeModify)}"),
                        InlineKeyboardButton.WithCallbackData("Back",$"{nameof(Admin)}:{nameof(Curse)}:{ConstData.Back}"),
                    }

                });

            public static readonly InlineKeyboardMarkup MuteTimeModify = new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Hour +",$"{nameof(Admin)}:{nameof(MuteTimeModify)}:{ConstData.HourPlus}"),
                        InlineKeyboardButton.WithCallbackData("Min +",$"{nameof(Admin)}:{nameof(MuteTimeModify)}:{ConstData.MinutePlus}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Hour -",$"{nameof(Admin)}:{nameof(MuteTimeModify)}:{ConstData.HourMinus}"),
                        InlineKeyboardButton.WithCallbackData("Min -",$"{nameof(Admin)}:{nameof(MuteTimeModify)}:{ConstData.MinuteMinus}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Back",$"{nameof(Admin)}:{nameof(MuteTimeModify)}:{ConstData.Back}"),
                    }
                });
        }
    }

    public struct Member
    {
        public static InlineKeyboardMarkup CreateForceJoinMarkup(List<ForceJoinChannel> channels, long userId)
        {
            var outPut = new List<List<InlineKeyboardButton>>();
            var splitter = 0;
            foreach (var channel in channels)
            {
                if (splitter % 2 == 0)
                {
                    outPut.Add(new List<InlineKeyboardButton>());
                }

                var channelId = channel.ChannelId.Contains("https") ? channel.ChannelId.Trim() : $"https://t.me/{channel.ChannelId.Trim()}";

                outPut.Last().Add(InlineKeyboardButton.WithUrl($"Channel-{splitter}", channelId!));
                splitter++;
            }
            outPut.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData("Confirm", $"{nameof(Member)}:Force:{userId}") });
            return new InlineKeyboardMarkup(outPut);
        }
    }
}

public struct ConstData
{
    public const string IgnoreMe = "Ignore";
    public const string Warn = "wrn";
    public const string Close = "cls";
    public const string Back = "bk";
    public const string Plus = "+";
    public const string Minus = "-";
    public const string HourPlus = $"H{Plus}";
    public const string HourMinus = $"H{Minus}";
    public const string MinutePlus = $"M{Plus}";
    public const string MinuteMinus = $"M{Minus}";
    public const string Ban = "bn";
    public const string Mute = "mt";
    public const char TrueEmoji = '✅';
    public const char FalseEmoji = '❌';

    public const string MessageOfMainMenu = "Settings:";
    public const string MessageOfCurseMenu = "Curse Configs:";
    public const string MessageOfWarnMenu = "Warn Configs:";
    public const string MessageOfModifyMuteTimeMenu = "Modify Mute Time:";

}