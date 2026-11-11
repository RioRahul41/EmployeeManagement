using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.Configuration
{
    public class MailSettings
    {

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
