using System;

namespace DigitalArena.Helpers
{
    public static class DateHelper
    {
        public static string GetPublishedAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.Now - createdAt;

            if (timeSpan.TotalDays >= 365)
            {
                int years = (int)(timeSpan.TotalDays / 365);
                return $"{years} year{(years > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalDays >= 30)
            {
                int months = (int)(timeSpan.TotalDays / 30);
                return $"{months} month{(months > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalDays >= 1)
            {
                int days = (int)timeSpan.TotalDays;
                return $"{days} day{(days > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                int hours = (int)timeSpan.TotalHours;
                return $"{hours} hour{(hours > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                int minutes = (int)timeSpan.TotalMinutes;
                return $"{minutes} minute{(minutes > 1 ? "s" : "")} ago";
            }
            else
            {
                int seconds = (int)timeSpan.TotalSeconds;
                return $"{seconds} second{(seconds > 1 ? "s" : "")} ago";
            }
        }
    }
}
