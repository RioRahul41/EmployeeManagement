using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Model.Entities
{
    public class EmailLog
    {
        public int Id { get; set; }

        public string ToEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string EmailType { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string? ErrorMessage { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public DateTime? SentTime { get; set; }
    }
}
