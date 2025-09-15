using System;
using System.ComponentModel.DataAnnotations;

namespace ClientNotifier.Core.Models
{
    public enum NotificationChannel
    {
        Email = 1
    }

    public enum NotificationType
    {
        Birthday = 1,
        Nameday = 2
    }

    public class NotificationLog
    {
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }

        [Required]
        public NotificationChannel Channel { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [StringLength(255)]
        public string? Subject { get; set; }

        public string? Error { get; set; }

        [Required]
        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    }
}


