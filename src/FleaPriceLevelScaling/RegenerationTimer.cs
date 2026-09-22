namespace FleaPriceLevelScaling;

/// <summary>
/// Encapsulates timing and elapsed duration calculations for periodic flea market regenerations.
/// </summary>
public sealed class RegenerationTimer
{
    private DateTimeOffset _lastRegenerationUtc = DateTimeOffset.MinValue;

    /// <summary>Wall-clock hour index used by the price walk.</summary>
    public static double CurrentMarketHour => DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 3600.0;

    /// <summary>
    /// Checks whether the specified regeneration interval has elapsed since the last regeneration.
    /// Returns true on the very first check (when no regeneration has occurred yet).
    /// </summary>
    public bool IsDue(TimeSpan interval, out TimeSpan elapsed)
    {
        var now = DateTimeOffset.UtcNow;
        if (_lastRegenerationUtc == DateTimeOffset.MinValue)
        {
            elapsed = TimeSpan.Zero;
            return true;
        }

        elapsed = now - _lastRegenerationUtc;
        return elapsed >= interval;
    }

    /// <summary>
    /// Records that a periodic regeneration occurred, resetting the elapsed timer to the current time.
    /// </summary>
    public void RecordRegeneration()
    {
        _lastRegenerationUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Formats a human-readable descriptor for the interval (e.g. "Hourly" or "5-minute").
    /// </summary>
    public static string FormatIntervalDescription(int intervalMinutes) =>
        intervalMinutes == 60 ? "Hourly" : $"{intervalMinutes}-minute";
}
