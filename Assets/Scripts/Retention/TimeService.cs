using System;

namespace KingSmash.Retention
{
    public static class TimeService
    {
        public static long UtcNow => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public static DateTimeOffset UtcNowDto => DateTimeOffset.UtcNow;

        public static bool IsSameUtcDay(long timestampA, long timestampB)
        {
            var a = DateTimeOffset.FromUnixTimeSeconds(timestampA).UtcDateTime;
            var b = DateTimeOffset.FromUnixTimeSeconds(timestampB).UtcDateTime;
            return a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;
        }

        public static bool IsPreviousUtcDay(long earlier, long later)
        {
            var a = DateTimeOffset.FromUnixTimeSeconds(earlier).UtcDateTime.Date;
            var b = DateTimeOffset.FromUnixTimeSeconds(later).UtcDateTime.Date;
            return (b - a).TotalDays >= 1.0 && a < b;
        }

        public static long StartOfUtcDay(long timestamp)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime.Date;
            return new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        public static string FormatLocalDate(long utcTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(utcTimestamp).LocalDateTime.ToShortDateString();
        }
    }
}
