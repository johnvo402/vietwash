using System.Globalization;

namespace Application.Feature.Reports.Common;

/// <summary>
/// Standardizes report filters as [UTC start inclusive, UTC end exclusive).
/// Unix APIs represent absolute instants; local-date APIs are converted once
/// using the requested Time-Zone header.
/// </summary>
public static class ReportTimeRange
{
    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    public static DateOnly ParseLocalDate(string? value, string parameterName)
    {
        if (
            DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                out DateTimeOffset dateTimeOffset
            )
        )
            return DateOnly.FromDateTime(dateTimeOffset.DateTime);

        if (
            DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        )
            return date;

        throw new ArgumentException($"Invalid local date '{value}'.", parameterName);
    }

    public static ReportUtcRange ForLocalDay(DateOnly localDate, TimeZoneInfo timeZone) =>
        ForLocalDates(localDate, localDate, timeZone);

    public static ReportUtcRange ForLocalDates(
        DateOnly fromInclusive,
        DateOnly toInclusive,
        TimeZoneInfo timeZone
    )
    {
        if (toInclusive < fromInclusive)
            throw new ArgumentOutOfRangeException(nameof(toInclusive));

        DateTime localStart = DateTime.SpecifyKind(
            fromInclusive.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified
        );
        DateTime localEndExclusive = DateTime.SpecifyKind(
            toInclusive.AddDays(1).ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified
        );

        return new ReportUtcRange(
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone), TimeSpan.Zero),
            new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(localEndExclusive, timeZone),
                TimeSpan.Zero
            )
        );
    }

    public static ReportUtcRange ForUnixSeconds(long fromInclusive, long toInclusive)
    {
        if (toInclusive < fromInclusive)
            throw new ArgumentOutOfRangeException(nameof(toInclusive));

        return new ReportUtcRange(
            DateTimeOffset.FromUnixTimeSeconds(fromInclusive),
            DateTimeOffset.FromUnixTimeSeconds(toInclusive).AddSeconds(1)
        );
    }

    public static IEnumerable<DateOnly> EnumerateLocalDates(
        DateOnly fromInclusive,
        DateOnly toInclusive
    )
    {
        if (toInclusive < fromInclusive)
            throw new ArgumentOutOfRangeException(nameof(toInclusive));

        return Enumerable
            .Range(0, toInclusive.DayNumber - fromInclusive.DayNumber + 1)
            .Select(fromInclusive.AddDays);
    }
}

public readonly record struct ReportUtcRange(
    DateTimeOffset UtcStartInclusive,
    DateTimeOffset UtcEndExclusive
);
