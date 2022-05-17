using System.Timers;
using Timer = System.Timers.Timer;

namespace GroupManager.Common.Models;

public class BetterTimer : Timer
{
    public BetterTimer(Action<object?, ElapsedEventArgs> onElapsed)
    {
        Elapsed += (sender, e) => onElapsed?.Invoke(sender, e);

    }
}