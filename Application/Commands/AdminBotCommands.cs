using System.Text.RegularExpressions;
using Castle.Core.Internal;
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
    [Describer("!!is active", "Check If Group Is Under bot Protection Or Not", null)]
    internal Task IsActiveAsync(Message message, CancellationToken ct = default)
    {
        var response = CurrentGroup is null
            ? "Bot Is Not Active in This group"
            : "Bot Is Active Here";
        return Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct);
    }
    [Describer("!!remove gp", "Remove Group From Protection Of Bot", null)]
    internal async Task RemoveGroupAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Registered For Bot",
                cancellationToken: ct).ConfigureAwait(false);
            return;
        }

        var removeGroupResult = await GroupController.RemoveGroupAsync(message.Chat.Id, ct).ConfigureAwait(false);
        var removeForceChannelsResult = await ForceJoinController.RemoveAllRelatedChannelsAsync(message.Chat.Id, ct).ConfigureAwait(false);
        var removeFloodSettingsResult = await FloodController.RemoveSettingsAsync(message.Chat.Id, ct).ConfigureAwait(false);
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
        await Client.SendTextMessageAsync(message.Chat.Id, $"{removeGroupResponse}\n{removeForceChannelsResponse}\n{removeSettingResponse}", cancellationToken: ct).ConfigureAwait(false);
    }
    [Describer("!!add gp", "Add Group To List!", null)]
    internal async Task AddGroupAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is not null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Already added", cancellationToken: ct).ConfigureAwait(false);
            return;
        }

        var addedGp = await GroupController.AddGroupAsync(message.Chat.Id, ct).ConfigureAwait(false);
        if (addedGp is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Add Group Right Now", cancellationToken: ct).ConfigureAwait(false);
            return;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, "Group Has Been Added To Bot", cancellationToken: ct).ConfigureAwait(false);
    }
    [Describer("!!sett", "Open Inline Panel For Settings", null)]
    internal async Task SettingAsync(Message message, CancellationToken ct = default)
    {
        if (message.From is null)
            return;
        await Client.SendTextMessageAsync(message.Chat.Id, ConstData.MessageOfMainMenu, replyMarkup: InlineButtons.Admin.SettingMenu,
            cancellationToken: ct, replyToMessageId: message.MessageId).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!set welcome", "set Welcome Message", "TEXT")]
    internal async Task SetWelcomeAsync(Message message, CancellationToken ct = default)
    {
        try
        {

            var group = await GroupController.UpdateGroupAsync(p =>
            {
                var messageToUpdate = message.Text?.Replace("!!set welcome", "");
                p.WelcomeMessage = messageToUpdate;
            }, message.Chat.Id, ct).ConfigureAwait(false);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", cancellationToken: ct).ConfigureAwait(false);
                return;
            }

            await Client.SendTextMessageAsync(message.Chat.Id, $"Welcome Message Set To :\n({group.WelcomeMessage})", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(SetWelcomeAsync));
        }
    }
    [Describer("!!disable welcome", "Disable Welcome Message", null)]
    internal async Task DisableWelcomeAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.SayWelcome = false;
            }, message.Chat.Id, ct).ConfigureAwait(false);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

                return;
            }

            await Client.SendTextMessageAsync(message.Chat.Id, "Welcome Message Disabled!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(DisableWelcomeAsync));
        }

    }
    [Describer("!!enable welcome", "Enable Welcome Message", null)]
    internal async Task EnableWelcomeAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p =>
            {
                p.SayWelcome = true;

            }, message.Chat.Id, ct).ConfigureAwait(false);
            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Welcome Message Enabled!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(EnableWelcomeAsync));
        }

    }
    [Describer("!!unban", "unban User With Specified UserId OR Replied To.", "USERID | Replay")]
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

            await Client.UnbanChatMemberAsync(message.Chat.Id, userIdFromCommand, true, ct).ConfigureAwait(false);
            await Client.SendTextMessageAsync(message.Chat.Id, $"User {userIdFromCommand} Has Been Unbanned", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UnBanUserAsync));
        }
    }
    [Describer("!!ban", "ban User With Specified UserId OR Replied To.", "USERID | Replay")]
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

            await Client.BanChatMemberAsync(message.Chat.Id, userIdFromCommand, cancellationToken: ct).ConfigureAwait(false);
            await Client.SendTextMessageAsync(message.Chat.Id, $"User {userIdFromCommand} Has Been banned", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UnBanUserAsync));
        }
    }
    [Describer("!!enable force", "Enable Force Join WhenEver A User Joined In Chat Or Sent Message.", null)]
    internal async Task EnableForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p => { p.ForceJoin = true; }, message.Chat.Id, ct).ConfigureAwait(false);

            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Force Join Enabled", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(EnableForceJoinAsync));
        }
    }
    [Describer("!!disable force", "Disable Force Join.", null)]
    internal async Task DisableForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var group = await GroupController.UpdateGroupAsync(p => { p.ForceJoin = false; }, message.Chat.Id, ct).ConfigureAwait(false);

            if (group is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }
            await Client.SendTextMessageAsync(message.Chat.Id, "Force Join Disabled", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(DisableForceJoinAsync));
        }
    }
    [Describer("!!add force", "Add Channel For ForceJoin.", "ChannelID")]
    internal async Task AddForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var channelId = message.Text?.Replace("!!add force", "");
            if (channelId is null or "" or " ")
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Channel Id Is Wrong!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }

            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }

            await ForceJoinController.AddChannelAsync(CurrentGroup.Id, channelId.Trim(), ct).ConfigureAwait(false);


            await Client.SendTextMessageAsync(message.Chat.Id, $"Channel @{channelId?.Replace("@", "")} Added To Force Join List.", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddForceJoinAsync));
        }
    }
    [Describer("!!rem force", "Remove Specified Channel From Force Joins.", "ChannelID")]
    internal async Task RemoveForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            var channelId = message.Text?.Replace("!!add force", "");
            if (channelId is null or "" or " ")
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Channel Id Is Wrong!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }

            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }
            var result = await ForceJoinController.RemoveChannelAsync(CurrentGroup.Id, channelId.Replace("!!rem force ", ""), ct).ConfigureAwait(false);
            var outputText = result switch
            {
                0 => "Channel Has been Removed",
                1 => "Channel Not Found",
                2 => "There Is Some Issues during Remove Channel Operation",
                _ => "-",
            };
            await Client.SendTextMessageAsync(message.Chat.Id, outputText, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddForceJoinAsync));
        }
    }
    [Describer("!!list force", "List Of All Force Join Channels For This Group", null)]
    internal async Task GetListOfForceJoinAsync(Message message, CancellationToken ct = default)
    {
        try
        {
            if (CurrentGroup is null)
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }

            var channelsList = await ForceJoinController.GetAllChannelsAsync(CurrentGroup.Id, ct).ConfigureAwait(false);
            if (channelsList is null or { Count: 0 })
            {
                await Client.SendTextMessageAsync(message.Chat.Id, "No Channel Found!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
                return;
            }

            var outputText = "";
            channelsList.ForEach(p =>
            {
                outputText += $"@{p.ChannelId}\n";
            });
            await Client.SendTextMessageAsync(message.Chat.Id, $"List Of All Force-Join-Channels:\n{outputText}",
                cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetListOfForceJoinAsync));
        }
    }
    [Describer("!!mute all", "Mute All Members Of Chat", null)]
    internal async Task MuteAllChatAsync(Message message, CancellationToken ct = default)
    {

        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        try
        {
            await Client.SetChatPermissionsAsync(message.Chat.Id, Globals.MutePermissions, ct).ConfigureAwait(false);
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Has been Muted", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Bot Doesn't Have Permissions To Do This\nOr Just Can Do it Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        }
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

    }
    [Describer("!!unmute all", "UnMute All Members Of Chat", null)]
    internal async Task UnMuteAllChatAsync(Message message, CancellationToken ct = default)
    {

        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        try
        {
            var chat = await Client.GetChatAsync(message.Chat.Id, ct).ConfigureAwait(false);

            await Client.SetChatPermissionsAsync(message.Chat.Id, Globals.UnMutePermissions, ct).ConfigureAwait(false);
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Has been UnMuted", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Bot Doesn't Have Permissions To Do This\nOr Just Can Do it Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        }
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

    }
    [Describer("!!set tbm", "Set Time-Based-Mute For Group.\nThis Will Mute Group In Specified Time Period.", "-from:TIME=>12:43 PM\n-until=>14:12 PM")]
    internal async Task SetTimeBasedMuteAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
        }, message.Chat.Id, ct).ConfigureAwait(false);
        await Client.SendTextMessageAsync(message.Chat.Id,
            $"Time-Based-Mute has been set to:\n<b>[{fromTime.TimeOfDay.Humanize(3)}]</b> until <b>[{untilTime.TimeOfDay.Humanize(3)}]</b>." +
            $"\nGroup Will Be muted for <b>[{muteTime.Humanize(3)}]</b>", ParseMode.Html,
            replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!enable tbm", "Enable Time-Based-Mute.", null)]
    internal async Task EnableTimeBasedMuteAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var muteHashId = $"Mute{CurrentGroup.GroupId}";
        var unmuteHashId = $"UnMute{CurrentGroup.GroupId}";

        await GroupController.UpdateGroupAsync(p =>
        {
            p.TimeBasedMute = true;
            p.TimeBasedMuteFuncHashId = muteHashId;
            p.TimeBasedUnmuteFuncHashId = unmuteHashId;
        }, message.Chat.Id, ct).ConfigureAwait(false);


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


        await Client.SendTextMessageAsync(message.Chat.Id, "Time-Based-Mute Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!disable tbm", "Disable Time-Based-Mute.", null)]
    internal async Task DisableTimeBasedMuteAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
        }, message.Chat.Id, ct).ConfigureAwait(false);

        await Client.SendTextMessageAsync(message.Chat.Id, "Time-Based-Mute Deactivated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!monitor tbm", "This Is For Developer Monitoring!\nDon't Touch It!", "-s Scheduled\n-q Queues")]
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
        await Client.SendTextMessageAsync(message.Chat.Id, data, cancellationToken: ct).ConfigureAwait(false);
    }
    [Describer("!!set ml", "Set Message Count For Each user Per Day.\nAfter This Amount User Will Be Muted", "COUNT")]
    internal async Task SetMessageLimitPerUserDayAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
        }, CurrentGroup.GroupId, ct).ConfigureAwait(false);
        if (updatedGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Updated Group Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, $"Max Message Per User has been Set to:({updatedGroup.MaxMessagePerUser})", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!enable ml", "Enable Message Count Limit.", null)]
    internal async Task EnableMessageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        await SetMessageLimitStatusAsync(message, true, ct).ConfigureAwait(false);
    }
    [Describer("!!disable ml", "Disable Message Count Limit.", null)]
    internal async Task DisableMessageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        await SetMessageLimitStatusAsync(message, false, ct).ConfigureAwait(false);
    }
    [Describer("!!mute", "Mute User With Specified Id Or Replied.", "USERID | Replay")]
    internal async Task MuteUserAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var userId = GetIdForMuteUser(message);

        if (userId is 0)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "User Id Is Not Specified!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        await SetUserPermissionStatusAsync(userId, message.Chat.Id, true, ct).ConfigureAwait(false);

    }
    [Describer("!!unmute", "UnMute User With Specified Id Or Replied.", "USERID | Replay")]
    internal async Task UnMuteUserAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var userId = GetIdForMuteUser(message);

        if (userId is 0)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "User Id Is Not Specified!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        await SetUserPermissionStatusAsync(userId, message.Chat.Id, false, ct).ConfigureAwait(false);
    }
    [Describer("!!enable flood", "Enable Anti-Flood System With Default Setups.", null)]
    internal async Task EnableFloodAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }


        var setting = await FloodController.AddFloodSettingAsync(CurrentGroup.Id, Globals.DefaultFloodSettings, ct).ConfigureAwait(false);
        if (setting is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Enable Anti Flood", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        try
        {
            AntiFloodService.Settings.Add(setting);
            await Client.SendTextMessageAsync(message.Chat.Id, "Anti Flood Added To Bot", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Store In-Memory Anti Flood", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        }

        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!disable flood", "Disable Anti-Flood System.", null)]
    internal async Task DisableFloodAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var result = await FloodController.RemoveSettingsAsync(message.Chat.Id, ct).ConfigureAwait(false);
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
        await Client.SendTextMessageAsync(message.Chat.Id, $"Flood Status:\n{response}\nMemory Status:\nGroup:{memoryResponse}\nSetting:{memoryRemoveSetting}", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!set flood", "Configure Anti-Flood System.", "--mute:This Will Mute User On Detection.\n--ban:This Will Ban User On Detection.\n" +
                                                            "--noban,--nomute : Reversed The --ban And --mute Commands.\n-i Seconds For Allowed Message Count[Default Is 10].\n" +
                                                            "Avoid Setting This Randomly It If You Don't Know How To Use it!.\n" +
                                                            "-c Message Count In Each Interval.[Default Is 7].\n" +
                                                            "--day:Days Of ban/Mute User[Default Is 3].\n" +
                                                            "--hour: Hours of ban/Mute User.\n" +
                                                            "--month: Month Of ban/Mute User.")]
    internal async Task FloodSettingsAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var regex = RegPatterns.Get.BaseCommandData(message.Text);
        if (regex is null)
            return;
        var settings = await FloodController.GetFloodSettingAsync(CurrentGroup.Id, ct).ConfigureAwait(false);
        if (settings is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Flood Setting NotFound!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
                            await Client.SendTextMessageAsync(message.Chat.Id, "Interval Value Is Not Valid!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
                            await Client.SendTextMessageAsync(message.Chat.Id, "Max Message Count Value Is Not Valid!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
                            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Day Time!", cancellationToken: ct).ConfigureAwait(false);
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
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Month Time!", cancellationToken: ct).ConfigureAwait(false);
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
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Hour Time!", cancellationToken: ct).ConfigureAwait(false);
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

        }, CurrentGroup.Id, ct).ConfigureAwait(false);
        await AntiFloodService.LoadAntiFloodGroupsAsync(ct).ConfigureAwait(false);
        await Client.SendTextMessageAsync(message.Chat.Id, "Flood Settings Updated Updated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

    }
    [Describer("!!enable ms", "Enable Message Size Limit.", null)]
    internal async Task EnableMessageSizeLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        await GroupController.UpdateGroupAsync(p => p.LimitMessageSize = true, message.Chat.Id, ct).ConfigureAwait(false);

        await Client.SendTextMessageAsync(message.Chat.Id, "Message Size Enabled", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!disable ms", "Disable Message Size Limit.", null)]
    internal async Task DisableMessageSizeLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        await GroupController.UpdateGroupAsync(p => p.LimitMessageSize = false, message.Chat.Id, ct).ConfigureAwait(false);

        await Client.SendTextMessageAsync(message.Chat.Id, "Message Size Disabled", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!set ms", "Set Message Size Limit!", "--line:Lines Of Size.\nThis Will Convert To Char.[Each Line ~= 50 Char].\n--char Count Of Chars")]
    internal async Task SetMessageSizeLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid -line Argument", cancellationToken: ct).ConfigureAwait(false);
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
                            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid -line Argument", cancellationToken: ct).ConfigureAwait(false);
                            break;
                        }

                        charCount = num;

                        break;
                    }
            }
        }

        if (charCount is null or default(uint))
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "no valid for Char Count Found!", cancellationToken: ct).ConfigureAwait(false);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.MaxMessageSize = charCount.Value, message.Chat.Id, ct).ConfigureAwait(false);

        var response = result is null ? "Cant Update group Message Limit" : $" message Size Limit Updated To {charCount} Char.";
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct).ConfigureAwait(false);

    }
    [Describer("!!filter", "Filter Specified Words.\nThis Bot Has A Base-Default Filters For Bad Words.", "Word")]
    internal async Task AddNewFilterWordAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        if (message.Text is null or "filter")
            return;

        var data = message.Text.Replace("filter", "");

        var result = await _textFilter.AddWordIfNotExistsAsync(data, ct).ConfigureAwait(false);

        var response = result switch
        {
            0 => "Word Is Already Exists In Filters.",
            1 => "Word Added To Filters!",
            _ => "Cant Add Anything Right Now ):"
        };
        await Client.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: ct).ConfigureAwait(false);
    }
    [Describer("!!help", "All Cli Commands", null)]
    internal async Task HelpAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var allCommands = Globals.Describers
            .Select(p => p.BuildParametersToString())
            .Aggregate("", (current, command) => current + (command + "\n"));
        await Client.SendTextMessageAsync(message.Chat.Id, allCommands is "" or " " ? "Nothing Found" : allCommands, ParseMode.Html, cancellationToken: ct).ConfigureAwait(false);
    }
    [Describer("set media limit", "this will set your limitations for media such as (gif,video,sticker,photo)",
        "-g: GIF LIMIT\n-s: STICKER LIMIT\n-v: VIDEO LIMIT\n-p: PHOTO LIMIT\n" +
        "-all: SET ALL TOGETHER\n-r: RESET USERS LIMIT(set all limits of all users to 0).\n")]
    internal async Task SetMediaLimitAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
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
                await Client.SendTextMessageAsync(message.Chat.Id, $"Invalid argument {value} given to {command}", cancellationToken: ct).ConfigureAwait(false);
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
            }, ct).ConfigureAwait(false);
            var resetResponse = resetResult switch
            {
                0 => "Users Has been updated",
                _ => "Cant Update Users"
            };
            await Client.SendTextMessageAsync(message.Chat.Id, resetResponse, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);

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

        }, message.Chat.Id, ct).ConfigureAwait(false);
        await MediaLimitService.ReLoadGroupsAsync(ct).ConfigureAwait(false);
        var response = result is null ? "Cant Update Group Right Now" : "Limitations Of Group Updated";
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!enable media limit", "Enable Limitations For Media.", null)]
    internal async Task EnableMediaLimitAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.LimitMedia = true, message.Chat.Id, ct).ConfigureAwait(false);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Media Limit Enabled"
        };
        await MediaLimitService.ReLoadGroupsAsync(ct).ConfigureAwait(false);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);


    }
    [Describer("!!disable media limit", "Disable Limitations For Media.", null)]
    internal async Task DisableMediaLimitAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.LimitMedia = false, message.Chat.Id, ct).ConfigureAwait(false);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Media Limit Disabled"
        };
        await MediaLimitService.ReLoadGroupsAsync(ct).ConfigureAwait(false);

        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);


    }
    [Describer("!!set lang", "Set Specified Language as a Allowed Language In Group!\nEnglish Added By Default", "Language:TEXT")]
    internal async Task SetLanguageAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated",
                replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var regex = RegPatterns.Get.BaseCommandData(message.Text);
        if (regex is null or { Count: 0 })
            return;

        var command = regex.First().Groups["name"].Value;
        var isValid = Enum.TryParse<Language>(command, out var lang);
        if (!isValid)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Invalid Language", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var result = await GroupController.UpdateGroupAsync(p => p.AllowedLanguage = lang, message.Chat.Id, ct).ConfigureAwait(false);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => $"Language Changed To {lang}"
        };
        await MediaLimitService.ReLoadGroupsAsync(ct).ConfigureAwait(false);

        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }
    [Describer("!!enable lang", "Enable Language Limit For Group.", null)]
    internal async Task EnableLanguageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        var result = await GroupController.UpdateGroupAsync(p => p.LanguageLimit = true, message.Chat.Id, ct).ConfigureAwait(false);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Language Limit Enabled"
        };
        await LanguageService.ReloadGroupsAsync(ct).ConfigureAwait(false);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);

    }
    [Describer("!!disable lang", "Disable Language Limit For Group.", null)]
    internal async Task DisableLanguageLimitAsync(Message message, CancellationToken ct = default)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        var result = await GroupController.UpdateGroupAsync(p => p.LanguageLimit = false, message.Chat.Id, ct).ConfigureAwait(false);
        var response = result switch
        {
            null => "Cant Update Group Right Now",
            _ => "Language Limit Disabled"
        };
        await LanguageService.ReloadGroupsAsync(ct).ConfigureAwait(false);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }

    internal async Task SetAdminAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var id = GetIdForAdminHandler(message);
        if (id is 0)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Id Not Specified", cancellationToken: ct).ConfigureAwait(false);
            return;
        }

        var result = await AdminController.CreateAdminIfNotExistsAsync(id, CurrentGroup.Id, ct).ConfigureAwait(false);
        var response = result switch
        {
            0 => "Admin Already Exists",
            1 => "Admin Added Successfully",
            _ => "Cant Create Admins Right Now"
        };
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
    }

    internal async Task RemoveAdminAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var id = GetIdForAdminHandler(message);
        if (id is 0)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Id Not Specified", cancellationToken: ct).ConfigureAwait(false);
            return;
        }

        var result = await AdminController.RemoveAdminAsync(id, CurrentGroup.Id, ct).ConfigureAwait(false);
        var response = result switch
        {
            0 => "Id Not Found.",
            1 => "Admin Removed Successfully.",
            _ => "Cant Remove Admins Right Now."
        };
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
    }

    internal async Task AdminListAsync(Message message, CancellationToken ct)
    {
        if (CurrentGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Group Is Not Activated", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }

        var admins = await AdminController.GetAllAdminsAsync(message.Chat.Id, ct).ConfigureAwait(false);
        if (admins is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Find Any Admin But You!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;

        }

        var response = "";
        var counter = 1;
        foreach (var ad in admins)
        {
            response += $"{counter}-{ad.UserId}\n";
            counter++;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, response, replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }

    private static long GetIdForMuteUser(Message message)
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

    private static long GetIdForAdminHandler(Message message)
    {
        long userId;
        if (message.ReplyToMessage?.From is not null)
        {
            userId = message.ReplyToMessage.From.Id;
        }
        else
        {
            var regex = RegPatterns.Get.GetUserIdData(message.Text);

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
            await Client.RestrictChatMemberAsync(groupId, userId, muteUser ? Globals.MutePermissions : Globals.UnMutePermissions, cancellationToken: ct).ConfigureAwait(false);
            var text = muteUser ? $"User {userId} Has been Muted!" : $"User {userId} Has been UnMuted!";
            await Client.SendTextMessageAsync(groupId, text, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await Client.SendTextMessageAsync(groupId, $"Cant Change User Permissions!\nThere Is Some Issues!", cancellationToken: ct).ConfigureAwait(false);
        }
    }

    private async Task SetMessageLimitStatusAsync(Message message, bool limitStatus, CancellationToken ct = default)
    {
        var updatedGroup = await GroupController.UpdateGroupAsync(p =>
        {
            p.EnableMessageLimitPerUser = limitStatus;
        }, message.Chat.Id, ct).ConfigureAwait(false);

        if (updatedGroup is null)
        {
            await Client.SendTextMessageAsync(message.Chat.Id, "Cant Updated Group Right Now!", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
            await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
            return;
        }
        await Client.SendTextMessageAsync(message.Chat.Id, $"Message Limit Has been Enabled", replyToMessageId: message.MessageId, cancellationToken: ct).ConfigureAwait(false);
        await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct).ConfigureAwait(false);
    }

}
