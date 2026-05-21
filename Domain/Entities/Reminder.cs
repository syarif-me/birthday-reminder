using BirthdayReminder.Domain.Enums;

namespace BirthdayReminder.Domain.Entities
{
    public class Reminder
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DateOnly ScheduledDate { get; private set; }
        public ReminderType Type { get; private set; } = default!;
        public ReminderStatus Status { get; private set; } = default!;
        public int RetryCount { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime? SentAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public Reminder() { }

        public Reminder(Guid userId, DateOnly scheduledDate, ReminderType type)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            ScheduledDate = scheduledDate;
            Type = type;
            Status = ReminderStatus.Pending;
            RetryCount = 0;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void MarkAsSent(DateTime sentAtUtc)
        {
            Status = ReminderStatus.Sent;
            SentAtUtc = sentAtUtc;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = ReminderStatus.Failed;
            ErrorMessage = errorMessage;
            RetryCount++;
        }
    }
}
