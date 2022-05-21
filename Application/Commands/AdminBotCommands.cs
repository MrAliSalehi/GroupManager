using System.Text.RegularExpressions;
using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Hangfire;
using Humanizer;
using GroupManager.Application.RecurringJobs;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Group = GroupManager.DataLayer.Models.Group;
using GroupManager.Application.Services;
using GroupManager.Common.Attributes;
using Mosaik.Core;

namespace GroupManager.Application.Commands;

public class AdminBotCommands : HandlerBase, IBotCommand, IDescriber
{
    public Group? CurrentGroup { get; set; }
    private readonly TextFilter _textFilter;
    private readonly RecurringJobManager _manager;

    public AdminBotCommands(ITelegramBotClient client) : base(client)
    {
        _manager = new RecurringJobManager();
        _textFilter = new TextFilter();
        _manager.AddOrUpdate<ResetMediaLimit>("ResetMediaLimit", (reset) => reset.ResetMediaLimitAsync(), "00 00 * * *");
    }
    [Describer("Is Active", "Check If Group Is Under bot Protection Or Not", null)]
    internal async Task IsActiveAsync(Message message, CancellationToken ct = default)
    {

        var response = CurrentGroup is null
            ? "Bot Is Not Active in This group"
            : "Bot Is Active Here";
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct);
    }

    internal async Task RemoveGroupAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Registered For Bot",
                cancellationToken: ct);
            return;
        }

        var removeGroupResult = await GroupController.RemoveGroupAsync(message.Chat.Id, ct);
        var removeForceChannelsResult = await ForceJoinController.RemoveAllRelatedChannelsAsync(message.Chat.Id, ct);
        var removeFloodSettingsResult = await FloodController.RemoveSettingsAsync(message.Chat.Id, ct);
        var removeGroupResponse = removeGroupResult switch
        {
            0 => "Group Has Been Removed",
            1 => "Group Is Not Already In List",
            2 => "There Is Some Issues With Storage",
            _ => "cant get any response"
        };

        var removeForceChannelsResponse = removeForceChannelsResult switch
        {
            0 => "Force Channels Removed.",
            1 => "No Force Channel Found.",
            _ => "Cant Get Any Execution Response For Force Channels"
        };
        var removeSettingResponse = removeFloodSettingsResult switch
        {
            0 => "Flood Setting Removed",
            1 => "Flood Setting Does not Exists",
            _ => "Cant Get Any FloodSetting"
        };
        await Client.SendTextMessageAsync(message.Chat.Id, $"{removeGroupResponse}\n{removeForceChannelsResponse}\n{removeSettingResponse}", cancellationToken: ct);
    }

    internal async Task AddGroupAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is not null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Already added", cancellationToken: ct);
            return;
        }

        var addedGp = await GroupController.AddGroupAsync(message.Chat.Id, ct);
        if (addedGp is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Add Group Right Now", cancellationToken: ct);
            return;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, "Group Has Been Added To Bot", cancellationToken: ct);
    }

    internal async Task SettingAsync(Message message, CancellationToken ct = default)
    {
        if (message.From is null)
            return;
        await Client.SendTextMessageAsync(message.Chat.Id, ConstData.MessageOfMainMenu, replyMarkup: InlineButtons.Admin.SettingMenu,
            cancellationToken: ct, replyToMessageId: message.MessageId);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task SetWelcomeAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var messageToUpdate = message.Text?.Replace("!!set welcome", "");

            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.WelcomeMessage = messageToUpdate;
            }, message.Chat.Id, ct);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", cancellationToken: ct);
                return;
            }

            await Client.SendTextMessageAsync(message.Chat.Id, $"Welcome Message Set To :\n({messageToUpdate})", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(SetWelcomeAsync));
        }
    }

    internal async Task DisableWelcomeAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.SayWelcome = false;
            }, message.Chat.Id, ct);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

                return;
            }

            await Client.SendTextMessageAsync(message.Chat.Id, "Welcome Message Disabled!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(DisableWelcomeAsync));
        }

    }

    internal async Task EnableWelcomeAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.SayWelcome = true;

            }, message.Chat.Id, ct);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Welcome Message Enabled!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(EnableWelcomeAsync));
        }

    }

    internal async Task UnBanUserAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var canParse = long.TryParse(message.Text?.Replace("!!unban", ""), out var userIdFromCommand);
            if (!canParse)
            {
                if (message.ReplyToMessage?.From is null)
                    return;

                userIdFromCommand = message.ReplyToMessage.From.Id;
            }

            if (userIdFromCommand is 0)
                return;

            await Client.UnbanChatMemberAsync(message.Chat.Id, userIdFromCommand, true, ct);
            await Client.SendTextMessageAsync(message.Chat.Id, $"User {userIdFromCommand} Has Been Unbanned", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UnBanUserAsync));
        }
    }

    internal async Task BanUserAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var canParse = long.TryParse(message.Text?.Replace("!!ban", ""), out var userIdFromCommand);
            if (!canParse)
            {
                if (message.ReplyToMessage?.From is null)
                    return;

                userIdFromCommand = message.ReplyToMessage.From.Id;
            }

            if (userIdFromCommand is 0)
                return;

            await Client.BanChatMemberAsync(message.Chat.Id, userIdFromCommand, cancellationToken: ct);
            await Client.SendTextMessageAsync(message.Chat.Id, $"User {userIdFromCommand} Has Been banned", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UnBanUserAsync));
        }
    }

    internal async Task EnableForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p => { p.ForceJoin = true; }, message.Chat.Id, ct);

            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Force Join Enabled", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(EnableForceJoinAsync));
        }
    }

    internal async Task DisableForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p => { p.ForceJoin = false; }, message.Chat.Id, ct);

            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Force Join Disabled", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(DisableForceJoinAsync));
        }
    }

    internal async Task AddForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var channelId = message.Text?.Replace("!!add force", "");
            if (channelId is null or "" or " ")
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Channel Id Is Wrong!", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            await ForceJoinController.AddChannelAsync(CurrentGroup.Id, channelId.Trim(), ct);


            await Client.SendTextMessageAsync(message.Chat.Id, $"Channel @{channelId?.Replace("@", "")} Added To Force Join List.", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddForceJoinAsync));
        }
    }

    internal async Task RemoveForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var channelId = message.Text?.Replace("!!add force", "");
            if (channelId is null or "" or " ")
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Channel Id Is Wrong!", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }
            var result = await ForceJoinController.RemoveChannelAsync(CurrentGroup.Id, channelId.Replace("!!rem force ", ""), ct);
            var outputText = result switch
            {
                0 => "Channel Has been Removed",
                1 => "Channel Not Found",
                2 => "There Is Some Issues during Remove Channel Operation",
                _ => "-",
            };
            await Client.SendTextMessageAsync(message.Chat.Id, outputText, replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddForceJoinAsync));
        }
    }

    internal async Task GetListOfForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            var channelsList = await ForceJoinController.GetAllChannelsAsync(CurrentGroup.Id, ct);
            if (channelsList is null or { Count: 0 })
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "No Channel Found!", replyToMessageId: message.MessageId, cancellationToken: ct);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                return;
            }

            var outputText = "";
            channelsList.ForEach(p =>
            {
                outputText += $"@{p.ChannelId}\n";
            });
            await Client.SendTextMessageAsync(message.Chat.Id, $"List Of All Force-Join-Channels:\n{outputText}",
                cancellationToken: ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetListOfForceJoinAsync));
        }
    }

    internal async Task MuteAllChatAsync(Message message, CancellationToken ct = default)
    {

        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        try
        {
            await Client.SetChatPermissionsAsync(message.Chat.Id, Globals.MutePermissions, ct);
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Has been Muted", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Bot Doesn't Have Permissions To Do This\nOr Just Can Do it Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

    }

    internal async Task UnMuteAllChatAsync(Message message, CancellationToken ct = default)
    {

        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        try
        {
            var chat = await Client.GetChatAsync(message.Chat.Id, ct);

            await Client.SetChatPermissionsAsync(message.Chat.Id, Globals.UnMutePermissions, ct);
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Has been UnMuted", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Bot Doesn't Have Permissions To Do This\nOr Just Can Do it Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

    }

    internal async Task SetTimeBasedMuteAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var data = RegPatterns.Get.TbmData(message.Text);
        if (data is null or { Success: false })
            return;
        var from = data.Groups["from"];
        var until = data.Groups["until"];

        if (!from.Success || !until.Success)
            return;
        var fromTime = DateTime.Parse(from.Value);
        var untilTime = DateTime.Parse(until.Value);
        var muteTime = TimeSpan.FromTicks((fromTime - untilTime).Ticks);
        await GroupController.UpdateGroupAsync(p =>
        {
            p.TimeBasedMuteFromTime = fromTime;
            p.TimeBasedMuteUntilTime = untilTime;
        }, message.Chat.Id, ct);
        await Client.SendTextMessageAsync(message.Chat.Id,
            $"Time-Based-Mute has been set to:\n<b>[{fromTime.TimeOfDay.Humanize(3)}]</b> until <b>[{untilTime.TimeOfDay.Humanize(3)}]</b>." +
            $"\nGroup Will Be muted for <b>[{muteTime.Humanize(3)}]</b>", ParseMode.Html,
            replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task EnableTimeBasedMuteAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var muteHashId = $"Mute{CurrentGroup.GroupId}";
        var unmuteHashId = $"UnMute{CurrentGroup.GroupId}";

        await GroupController.UpdateGroupAsync(p =>
        {
            p.TimeBasedMute = true;
            p.TimeBasedMuteFuncHashId = muteHashId;
            p.TimeBasedUnmuteFuncHashId = unmuteHashId;
        }, message.Chat.Id, ct);


        var fromHour = CurrentGroup.TimeBasedMuteFromTime.Hour;
        var fromMinute = CurrentGroup.TimeBasedMuteFromTime.Minute;
        var fromCron = $"{fromMinute} {fromHour} * * *";

        var untilHour = CurrentGroup.TimeBasedMuteUntilTime.Hour;
        var untilMinute = CurrentGroup.TimeBasedMuteUntilTime.Minute;
        var untilCron = $"{untilMinute} {untilHour} * * *";


        TimeBasedMute.Bot = Client as TelegramBotClient;
        TimeBasedMute.ChatId = message.Chat.Id;

        _manager.RemoveIfExists(muteHashId);
        _manager.RemoveIfExists(unmuteHashId);
        _manager.AddOrUpdate<TimeBasedMute>(muteHashId, (tbm) => tbm.TimeBasedMuteAsync(), fromCron, TimeZoneInfo.Local);

        _manager.AddOrUpdate<TimeBasedMute>(unmuteHashId, (tbm) => tbm.TimeBasedUnMuteAsync(), untilCron, TimeZoneInfo.Local);


        await Client.SendTextMessageAsync(message.Chat.Id, "Time-Based-Mute Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task DisableTimeBasedMuteAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        _manager.Trigger(CurrentGroup.TimeBasedUnmuteFuncHashId);

        _manager.RemoveIfExists(CurrentGroup.TimeBasedUnmuteFuncHashId);
        _manager.RemoveIfExists(CurrentGroup.TimeBasedMuteFuncHashId);

        await GroupController.UpdateGroupAsync(p =>
        {
            p.TimeBasedMute = false;
            p.TimeBasedMuteFuncHashId = string.Empty;
            p.TimeBasedUnmuteFuncHashId = string.Empty;
        }, message.Chat.Id, ct);

        await Client.SendTextMessageAsync(message.Chat.Id, "Time-Based-Mute Deactivated", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task MonitorTbmAsync(Message message, CancellationToken ct = default)
    {
        if (message.Text is null)
            return;
        var monitorApi = JobStorage.Current.GetMonitoringApi();

        var data = "";
        if (message.Text.Contains("-s"))
        {
            data += "\n Scheduled:\n";

            var items = monitorApi.ScheduledJobs(0, 10);
            if (items is null)
            {
                data += "Nothing Found For -Scheduled";
                goto SkipScheduled;
            }

            foreach (var item in items)
            {
                var scheduledAt = item.Value.ScheduledAt is null ? "-" : item.Value.ScheduledAt.Value.Humanize();
                var job = item.Value.Job is null ? "-" : item.Value.Job.Method.Name;
                data +=
                    $"Key:{item.Key}\n" +
                    $"Enqueue At:{item.Value.EnqueueAt.Humanize()}\n" +
                    $"InScheduledState:{item.Value.InScheduledState}\n" +
                    $"MethodName:{job}\n" +
                    $"ScheduledAt:{scheduledAt}\n";
            }
        }
    SkipScheduled:
        if (message.Text.Contains("-q"))
        {
            data += "\nQueues:\n";
            var queues = monitorApi.Queues();
            if (queues is null or { Count: 0 })
            {
                data += "Nothing Found For -Queues";
                goto SkipQueues;
            }

            foreach (var queue in queues)
            {
                var jobs = "";
                queue.FirstJobs.ForEach(v =>
                 {
                     var job = v.Value.Job is null ? "-" : v.Value.Job.Method.Name;

                     jobs += $"Key:{v.Key}\n" +
                             $"MethodName:{job}\n" +
                             $"Enqueued At:{v.Value.EnqueuedAt.Humanize()}\n" +
                             $"InEnqueuedState:{v.Value.InEnqueuedState}\n" +
                             $"State:{v.Value.State}\n";

                 });
                data += $"Name:{queue.Name}\n" +
                        $"Len:{queue.Length}" +
                        $"Fetched:{queue.Fetched}\n" +
                        $"----Jobs:[{jobs}]";

            }
        }
    SkipQueues:
        await Client.SendTextMessageAsync(message.Chat.Id, data, cancellationToken: ct);
    }

    internal async Task SetMessageLimitPerUserDayAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var regex = RegPatterns.Get.MessageLimitData(message.Text);
        if (regex is null or { Success: false })
            return;
        var canParse = uint.TryParse(regex.Groups["count"].Value, out var count);
        if (!canParse)
            return;
        var updatedGroup = await GroupController.UpdateGroupAsync(p =>
        {
            p.MaxMessagePerUser = count;
        }, CurrentGroup.GroupId, ct);
        if (updatedGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Updated Group Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, $"Max Message Per User has been Set to:({updatedGroup.MaxMessagePerUser})", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task EnableMessageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        await SetMessageLimitStatusAsync(message, true, ct);
    }

    internal async Task DisableMessageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        await SetMessageLimitStatusAsync(message, false, ct);
    }

    internal async Task MuteUserAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var userId = GetUserIdFromForwardOrDirectly(message);

        if (userId is 0)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "User Id Is Not Specified!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        await SetUserPermissionStatusAsync(userId, message.Chat.Id, true, ct);

    }

    internal async Task UnMuteUserAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var userId = GetUserIdFromForwardOrDirectly(message);

        if (userId is 0)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "User Id Is Not Specified!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        await SetUserPermissionStatusAsync(userId, message.Chat.Id, false, ct);
    }

    internal async Task EnableFloodAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }


        var setting = await FloodController.AddFloodSettingAsync(CurrentGroup.Id, Globals.DefaultFloodSettings, ct);
        if (setting is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Enable Anti Flood", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        try
        {
            AntiFloodService.Settings.Add(setting);
            await Client.SendTextMessageAsync(message.Chat.Id, "Anti Flood Added To Bot", replyToMessageId: message.MessageId, cancellationToken: ct);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Store In-Memory Anti Flood", replyToMessageId: message.MessageId, cancellationToken: ct);
        }

        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task DisableFloodAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var result = await FloodController.RemoveSettingsAsync(message.Chat.Id, ct);
        var response = result switch
        {
            0 => "Flood Has been Removed",
            1 => "Flood Is Already Disabled",
            _ => "Cant Remove Settings Now!"
        };
        //var memoryRemove = true;
        var memoryRemoveSetting = -1;
        try
        {
            //memoryRemove = AntiFloodService.Groups.Remove(message.Chat.Id);
            memoryRemoveSetting = AntiFloodService.Settings.RemoveAll(p => p.Group.GroupId == message.Chat.Id);
        }
        catch (Exception)
        {
            //memoryRemove = false;
            memoryRemoveSetting = -1;
        }

        var memoryResponse = memoryRemoveSetting switch
        {
            -1 => "Cant Clear memory Now",
            _ => "Memory Has Been Cleared"
        };
        await Client.SendTextMessageAsync(message.Chat.Id, $"Flood Status:\n{response}\nMemory Status:\nGroup:{memoryResponse}\nSetting:{memoryRemoveSetting}", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task FloodSettingsAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var regex = RegPatterns.Get.BaseCommandData(message.Text);
        if (regex is null)
            return;
        var settings = await FloodController.GetFloodSettingAsync(CurrentGroup.Id, ct);
        if (settings is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Flood Setting NotFound!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;

        }

        bool? mute = null;
        bool? ban = null;
        uint? interval = null;
        uint? messageCount = null;
        var restrictTime = TimeSpan.Zero;
        foreach (Match match in regex)
        {
            var command = match.Groups["name"].Value;
            var value = match.Groups["value"].Value;
            switch (command)
            {
                case "nomute":
                    mute = false;
                    break;
                case "noban":
                    ban = false;
                    break;
                case "mute":
                    mute = true;
                    break;
                case "ban":
                    ban = true;
                    break;
                case "i":
                    {
                        var canParse = uint.TryParse(value, out var result);
                        if (!canParse)
                        {
                            await Client.SendTextMessageAsync(message.Chat.Id, "Interval Value Is Not Valid!", replyToMessageId: message.MessageId, cancellationToken: ct);
                            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                            return;
                        }

                        interval = result;
                        break;
                    }
                case "c":
                    {
                        var canParse = uint.TryParse(value, out var count);
                        if (!canParse)
                        {
                            await Client.SendTextMessageAsync(message.Chat.Id, "Max Message Count Value Is Not Valid!", replyToMessageId: message.MessageId, cancellationToken: ct);
                            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
                            return;
                        }

                        messageCount = count;
                        break;
                    }
                case "day":
                    {
                        var canParse = short.TryParse(value, out var dayTime);
                        if (!canParse)
                        {
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Day Time!", cancellationToken: ct);
                            break;
                        }

                        restrictTime = restrictTime.Add(TimeSpan.FromDays(dayTime));
                        break;
                    }
                case "month":
                    {
                        var canParse = short.TryParse(value, out var monthTime);
                        if (!canParse)
                        {
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Month Time!", cancellationToken: ct);
                            break;
                        }
                        restrictTime = restrictTime.Add(TimeSpan.FromDays(monthTime * 30));
                        break;
                    }
                case "hour":
                    {
                        var canParse = short.TryParse(value, out var hourTime);
                        if (!canParse)
                        {
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Hour Time!", cancellationToken: ct);
                            break;
                        }
                        restrictTime = restrictTime.Add(TimeSpan.FromHours(hourTime));

                        break;
                    }
            }
        }


        await FloodController.UpdateSettingsAsync(p =>
        {
            if (!restrictTime.Equals(TimeSpan.Zero))
                p.RestrictTime = restrictTime;

            if (mute.HasValue)
                p.MuteOnDetect = mute.Value;

            if (ban.HasValue)
                p.BanOnDetect = ban.Value;

            if (interval.HasValue)
                p.Interval = interval.Value;

            if (messageCount.HasValue)
                p.MessageCountPerInterval = messageCount.Value;

        }, CurrentGroup.Id, ct);
        await AntiFloodService.LoadAntiFloodGroupsAsync(ct);
        await Client.SendTextMessageAsync(message.Chat.Id, "Flood Settings Updated Updated", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

    }

    internal async Task EnableMessageSizeLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        await GroupController.UpdateGroupAsync(p => p.LimitMessageSize = true, message.Chat.Id, ct);

        await Client.SendTextMessageAsync(message.Chat.Id, "Message Size Enabled", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task DisableMessageSizeLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        await GroupController.UpdateGroupAsync(p => p.LimitMessageSize = false, message.Chat.Id, ct);

        await Client.SendTextMessageAsync(message.Chat.Id, "Message Size Disabled", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task SetMessageSizeLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var matchCollection = RegPatterns.Get.BaseCommandData(message.Text);
        if (matchCollection is null or { Count: 0 })
            return;
        uint? charCount = default;

        foreach (Match match in matchCollection)
        {
            var command = match.Groups["name"].Value;
            var value = match.Groups["value"].Value;

            switch (command)
            {
                case "line":
                    {
                        var canParse = uint.TryParse(value, out var num);
                        if (!canParse)
                        {
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid -line Argument", cancellationToken: ct);
                            break;
                        }

                        charCount = num * 50;
                        break;
                    }

                case "char":
                    {
                        var canParse = uint.TryParse(value, out var num);
                        if (!canParse)
                        {
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid -line Argument", cancellationToken: ct);
                            break;
                        }

                        charCount = num;

                        break;
                    }
            }
        }

        if (charCount is null or default(uint))
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "no valid for Char Count Found!", cancellationToken: ct);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.MaxMessageSize = charCount.Value, message.Chat.Id, ct);

        var response = result is null ? "Cant Update group Message Limit" : $" message Size Limit Updated To {charCount} Char.";
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct);

    }

    internal async Task AddNewFilterWordAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        if (message.Text is null or "filter")
            return;

        var data = message.Text.Replace("filter", "");

        var result = await _textFilter.AddWordIfNotExistsAsync(data, ct);

        var response = result switch
        {
            0 => "Word Is Already Exists In Filters.",
            1 => "Word Added To Filters!",
            _ => "Cant Add Anything Right Now ):"
        };
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct);
    }

    internal async Task HelpAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var allCommands = Globals.Describers
            .Select(p => p.BuildParametersToString())
            .Aggregate("", (current, command) => current + (command + "\n"));
        await Client.SendTextMessageAsync(message.Chat.Id, allCommands is "" or " " ? "Nothing Found" : allCommands, ParseMode.MarkdownV2, cancellationToken: ct);
    }
    [Describer("set media limit", "this will set your limitations for media such as (gif,video,sticker,photo)",
        "-g: GIF LIMIT\n-s: STICKER LIMIT\n-v: VIDEO LIMIT\n-p: PHOTO LIMIT\n" +
        "-all: SET ALL TOGETHER\n-r: RESET USERS LIMIT(set all limits of all users to 0).\n")]
    internal async Task SetMediaLimitAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var regex = RegPatterns.Get.BaseCommandData(message.Text);
        if (regex is null or { Count: 0 })
            return;
        uint? video = 0;
        uint? photo = 0;
        uint? gif = 0;
        uint? sticker = 0;
        var resetUsers = false;

        foreach (Match match in regex)
        {
            var command = match.Groups["name"].Value;
            var value = match.Groups["value"].Value;
            if (command is "" or " ")
                continue;

            var canParseValue = uint.TryParse(value, out var num);
            if (!canParseValue)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, $"Invalid argument {value} given to {command}", cancellationToken: ct);
                continue;
            }
            switch (command)
            {
                case "r":
                    resetUsers = true;
                    break;
                case "g":
                    gif = num;
                    break;
                case "s":
                    sticker = num;
                    break;
                case "v":
                    video = num;
                    break;
                case "p":
                    photo = num;
                    break;
                case "all":
                    video = num;
                    photo = num;
                    sticker = num;
                    gif = num;
                    break;
            }
        }

        if (resetUsers)
        {
            var resetResult = await UserController.UpdateAllUsersAsync(user =>
             {
                 user.SentGif = 0;
                 user.SentPhotos = 0;
                 user.SentStickers = 0;
                 user.SentVideos = 0;
             }, ct);
            var resetResponse = resetResult switch
            {
                0 => "Users Has been updated",
                _ => "Cant Update Users"
            };
            await Client.SendTextMessageAsync(message.Chat.Id, resetResponse, replyToMessageId: message.MessageId, cancellationToken: ct);

        }
        var result = await GroupController.UpdateGroupAsync(p =>
        {
            if (video is not 0)
                p.VideoLimits = video.Value;

            if (photo is not 0)
                p.PhotoLimits = photo.Value;

            if (gif is not 0)
                p.GifLimits = gif.Value;

            if (sticker is not 0)
                p.StickerLimits = sticker.Value;

        }, message.Chat.Id, ct);
        await MediaLimitService.ReLoadGroupsAsync(ct);
        var response = result is null ? "Cant Update Group Right Now" : "Limitations Of Group Updated";
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task EnableMediaLimitAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.LimitMedia = true, message.Chat.Id, ct);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Media Limit Enabled"
        };
        await MediaLimitService.ReLoadGroupsAsync(ct);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);


    }

    internal async Task DisableMediaLimitAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.LimitMedia = false, message.Chat.Id, ct);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Media Limit Disabled"
        };
        await MediaLimitService.ReLoadGroupsAsync(ct);

        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);


    }

    internal async Task SetLanguageAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var regex = RegPatterns.Get.BaseCommandData(message.Text);
        if (regex is null or { Count: 0 })
            return;

        var command = regex.First().Groups["name"].Value;
        var isValid = Enum.TryParse<Language>(command, out var lang);
        if (!isValid)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Language", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.AllowedLanguage = lang, message.Chat.Id, ct);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => $"Language Changed To {lang}"
        };
        await MediaLimitService.ReLoadGroupsAsync(ct);

        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    internal async Task EnableLanguageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        var result = await GroupController.UpdateGroupAsync(p => p.LanguageLimit = true, message.Chat.Id, ct);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Language Limit Enabled"
        };
        await LanguageService.ReloadGroupsAsync(ct);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);

    }

    internal async Task DisableLanguageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        var result = await GroupController.UpdateGroupAsync(p => p.LanguageLimit = false, message.Chat.Id, ct);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Language Limit Disabled"
        };
        await LanguageService.ReloadGroupsAsync(ct);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }

    private static long GetUserIdFromForwardOrDirectly(Message message)
    {
        long userId;
        if (message.ReplyToMessage?.From is not null)
        {
            userId = message.ReplyToMessage.From.Id;
        }
        else
        {
            var regex = RegPatterns.Get.MuteUserData(message.Text);

            var canParse = long.TryParse(regex?.Groups["userId"].Value, out userId);
            if (!canParse)
                userId = 0;
        }
        return userId;
    }

    private async Task SetUserPermissionStatusAsync(long userId, long groupId, bool muteUser, CancellationToken ct = default)
    {
        try
        {
            await Client.RestrictChatMemberAsync(groupId, userId, muteUser ? Globals.MutePermissions : Globals.UnMutePermissions, cancellationToken: ct);
            var text = muteUser ? $"User {userId} Has been Muted!" : $"User {userId} Has been UnMuted!";
            await Client.SendTextMessageAsync(groupId, text, cancellationToken: ct);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(groupId, $"Cant Change User Permissions!\nThere Is Some Issues!", cancellationToken: ct);
        }
    }

    private async Task SetMessageLimitStatusAsync(Message message, bool limitStatus, CancellationToken ct = default)
    {
        var updatedGroup = await GroupController.UpdateGroupAsync(p =>
        {
            p.EnableMessageLimitPerUser = limitStatus;
        }, message.Chat.Id, ct);

        if (updatedGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Updated Group Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
            return;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, $"Message Limit Has been Enabled", replyToMessageId: message.MessageId, cancellationToken: ct);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
    }


}
