namespace WebInterface.Models.API.v1
{
    /// <summary>
	/// Summary description for UpdateSettingsViewModel
	/// </summary>
    public class UpdateSettingsViewModel
    {
        /// <summary>
		/// Seconds before connection idle timeout.
		/// </summary>
        public int ClientIdleTimeout { get; set; }

        /// <summary>
		/// Seconds before connection receive timeout.
		/// </summary>
        public int ReceiveTimeout { get; set; }

        /// <summary>
		/// Seconds before connection send timeout.
		/// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
		/// ID of the VirtualMTA Group to use if not specified in email headers.
		/// </summary>
        public int DefaultVirtualMtaGroupID { get; set; }

        /// <summary>
		/// URL to post events to.
		/// </summary>
        public string EventUrl { get; set; }

        /// <summary>
		/// Days to keep smtp logs defore deleing them.
		/// </summary>
        public int DaysToKeepSmtpLogsFor { get; set; }

        /// <summary>
        /// Max time in hours that an email can be in the queue for before it is timed out.
        /// </summary>
        public int MaxTimeInQueueHours { get; set; }

        /// <summary>
		/// The time in minutes to use as the base for the retry interval calculation.
		/// </summary>
        public int RetryIntervalBase { get; set; }

        /// <summary>
		/// Array of the IP Addresses that are allowed to relay through MantaMTA.
		/// </summary>
        public string[] IpAddressesForRelaying { get; set; }

        /// <summary>
		/// ID of the local domain that should be used as the hostname for the returnpath if none is specified in the email headers.
		/// </summary>
        public int ReturnPathLocalDomainID { get; set; }

        /// <summary>
		/// Array of the localdomains.
		/// </summary>
        public string[] LocalDomains { get; set; }
    }
}