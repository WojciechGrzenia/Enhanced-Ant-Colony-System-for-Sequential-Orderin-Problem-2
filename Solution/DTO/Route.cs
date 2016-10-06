using System;

namespace DTO
{
    /// <summary>
    ///     Represents the route.
    /// </summary>
    [Serializable]
    public class Route
    {
        /// <summary>
        ///     Gets or sets the cost.
        /// </summary>
        /// <value>
        ///     The cost.
        /// </value>
        public double Cost { get; set; }

        /// <summary>
        ///     Gets or sets the origin.
        /// </summary>
        /// <value>
        ///     The origin.
        /// </value>
        public Target Origin { get; set; }

        /// <summary>
        ///     Gets or sets the pheromone amount.
        /// </summary>
        /// <value>
        ///     The pheromone amount.
        /// </value>
        public double PheromoneAmount { get; set; }

        /// <summary>
        ///     Gets or sets the target.
        /// </summary>
        /// <value>
        ///     The target.
        /// </value>
        public Target Target { get; set; }
    }
}