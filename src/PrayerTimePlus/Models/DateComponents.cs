namespace PrayerTimePlus;

/// <summary>An immutable Gregorian civil date without a time or time zone.</summary>
/// <remarks>The default struct value is invalid and is rejected by <see cref="PrayerTimes"/>.</remarks>
public readonly record struct DateComponents
{
    /// <summary>Creates a validated Gregorian civil date.</summary>
    /// <param name="year">Year in [1, 9999].</param>
    /// <param name="month">Month in [1, 12].</param>
    /// <param name="day">Day valid for the specified month and year.</param>
    /// <exception cref="ArgumentOutOfRangeException">The civil date is invalid.</exception>
    public DateComponents(int year, int month, int day)
    {
        _ = new DateOnly(year, month, day);
        Year = year;
        Month = month;
        Day = day;
    }

    /// <summary>Gets the Gregorian year.</summary>
    public int Year { get; }

    /// <summary>Gets the month, from 1 to 12.</summary>
    public int Month { get; }

    /// <summary>Gets the day of the month.</summary>
    public int Day { get; }

    /// <summary>Copies a Gregorian civil date from a <see cref="DateOnly"/>.</summary>
    /// <param name="date">The civil date to copy.</param>
    /// <returns>Its immutable year, month and day components.</returns>
    public static DateComponents From(DateOnly date) => new(date.Year, date.Month, date.Day);

    /// <summary>Converts the components to a Gregorian <see cref="DateOnly"/>.</summary>
    /// <returns>The represented civil date.</returns>
    /// <exception cref="ArgumentOutOfRangeException">This is the invalid default struct value.</exception>
    public DateOnly ToDateOnly() => new(Year, Month, Day);
}
