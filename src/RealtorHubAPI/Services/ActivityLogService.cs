using RealtorHubAPI.Entities;
using RealtorHubAPI.Entities.Enums;

namespace CryptoProject.Services
{
    public static class ActivityLogService
    {
        public static ActivityLog CreateLogEntry(int? userId, string userEmail, ActivityType activityType, params object[] additionalInfo)
        {
            var logEntry = new ActivityLog
            {
                UserId = userId,
                UserEmail = userEmail,
                ActivityType = activityType,
                Details = GetDetailsMessage(activityType, additionalInfo)
            };

            return logEntry;
        }

        private static string GetDetailsMessage(ActivityType activityType, object[] additionalInfo)
        {
            switch (activityType)
            {
                //case ActivityType.UserRegistered:
                //    return $"User with ID {additionalInfo[0]} registered.";
            default:
                    return "Activity occurred.";
            }
        }
    }

}
