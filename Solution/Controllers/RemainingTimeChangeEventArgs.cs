using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controllers
{
    /// <summary>
    /// Contains the remaining time change event data.
    /// </summary>
    public class RemainingTimeChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the remaining time.
        /// </summary>
        /// <value>
        /// The remaining time.
        /// </value>
        public TimeSpan RemainingTime{ get; set; }
    }
}
