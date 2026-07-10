using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.Configuration
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 1025;
        public bool UseStartTls { get; set; } = false;

        public string SmtpUsername { get; set;} = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;

        public string FromEmail { get; set;} = string.Empty;
        public string FromName  { get; set;} = string.Empty;

        public string ClientBaseUrl { get; set;} = string.Empty;

        public int ConfirmationTokenLifetimeHours { get; set; } = 24;

    }
}
