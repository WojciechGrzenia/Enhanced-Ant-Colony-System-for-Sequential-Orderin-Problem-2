using DTO;

namespace Controllers.SequentailOrdering
{
    /// <summary>
    ///     Represents option for ant to go to.
    /// </summary>
    internal class AntOption
    {
        /// <summary>
        ///     Gets or sets all options rank sum. Used to determine probability.
        /// </summary>
        /// <value>
        ///     All options rank sum.
        /// </value>
        public double AllOptionsRankSum { get; set; }

        /// <summary>
        ///     Gets the probability of choosing this option.
        /// </summary>
        /// <value>
        ///     The probability.
        /// </value>
        public double Probability
        {
            get { return Rank/AllOptionsRankSum; }
        }

        /// <summary>
        ///     Gets or sets the rank.
        /// </summary>
        /// <value>
        ///     The rank.
        /// </value>
        public double Rank { get; set; }

        /// <summary>
        ///     Gets or sets the route.
        /// </summary>
        /// <value>
        ///     The route.
        /// </value>
        public Route Route { get; set; }
    }
}