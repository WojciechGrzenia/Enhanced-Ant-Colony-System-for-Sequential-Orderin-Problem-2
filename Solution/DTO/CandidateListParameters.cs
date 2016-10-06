using System;

namespace DTO
{
    /// <summary>
    ///     Parameters for candidate list.
    /// </summary>
    [Serializable]
    public class CandidateListParameters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CandidateListParameters" /> class.
        /// </summary>
        public CandidateListParameters()
        {
            UseCandidateList = false;
        }

        /// <summary>
        ///     Gets or sets the fraction of the maximum size.
        /// </summary>
        /// <value>
        ///     The fraction of the maximum size.
        /// </value>
        public double FractionOfMaxSize { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to take next best route if none is feasible in candidate list.
        /// </summary>
        /// <value>
        ///     A value indicating whether to take next best route if none is feasible in candidate list.
        /// </value>
        public bool IfNoneIsFeasibleTakeNextBest { get; set; }

        /// <summary>
        ///     Gets or sets the size.
        /// </summary>
        /// <value>
        ///     The size.
        /// </value>
        public int Size { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use candidate list.
        /// </summary>
        /// <value>
        ///     A value indicating whether to use candidate list.
        /// </value>
        public bool UseCandidateList { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use fixed size.
        /// </summary>
        /// <value>
        ///     A value indicating whether to use fixed size.
        /// </value>
        public bool UseFixedSize { get; set; }
    }
}