namespace AMStock.Core.Models.Enumerations
{
    /// <summary>Enumeration of Logging Methods</summary>
    public enum LogMethod
    {
        /// <summary>Use the system default logging method.</summary>
        Default = 0,
        /// <summary>Use the database logging method.</summary>
        Database = 1,
        /// <summary>Use the file logging method.</summary>
        File = 2,
        /// <summary>Use the system event logging method.</summary>
        EventLog = 3,
        /// <summary>Use the email logging method.</summary>
        Email = 4,
        /// <summary>Use the server logging method.</summary>
        Server = 5,
        /// <summary>Use the mail service logging method.</summary>
        MailService = 6,
    }
}
