using System;

namespace DTO
{
    /// <summary>
    ///     Represents restriction.
    /// </summary>
    [Serializable]
    public class Restriction
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this restriction is met.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this restriction is met; otherwise, <c>false</c>.
        /// </value>
        public bool IsMet { get; set; }

        /// <summary>
        ///     Gets or sets the needed target.
        /// </summary>
        /// <value>
        ///     The needed target.
        /// </value>
        public Target NeededTarget { get; set; }

        /// <summary>
        ///     Gets or sets the target to visit.
        /// </summary>
        /// <value>
        ///     The target to visit.
        /// </value>
        public Target TargetToVisit { get; set; }
    }
}