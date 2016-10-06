using System;

namespace Controllers
{
    /// <summary>
    ///     Stores program configuration.
    /// </summary>
    [Serializable]
    public class Configuration
    {
        /// <summary>
        ///     Gets or sets the number of concurrent jobs.
        /// </summary>
        /// <value>
        ///     The number of concurrent jobs.
        /// </value>
        public int NumberOfConcurrentJobs { get; set; }
    }
}