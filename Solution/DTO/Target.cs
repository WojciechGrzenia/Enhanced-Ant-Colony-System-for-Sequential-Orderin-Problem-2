using System;
using System.Collections.Generic;

namespace DTO
{
    /// <summary>
    ///     Represents one target (node) from sequential ordering problem.
    /// </summary>
    [Serializable]
    public class Target
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Target" /> class.
        /// </summary>
        public Target()
        {
            Routes = new List<Route>();
            Restrictions = new List<Restriction>();
            RequiredBy = new List<Restriction>();
        }

        /// <summary>
        ///     Gets or sets the index.
        /// </summary>
        /// <value>
        ///     The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is visited.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is visited; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisited { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the required by list which contains restrictions where current instance is needed target.
        /// </summary>
        /// <value>
        ///     The required by list which contains restrictions where current instance is needed target.
        /// </value>
        public List<Restriction> RequiredBy { get; set; }

        /// <summary>
        ///     Gets or sets the restrictions.
        /// </summary>
        /// <value>
        ///     The restrictions.
        /// </value>
        public List<Restriction> Restrictions { get; set; }

        /// <summary>
        ///     Gets or sets the number of restrictions to meet.
        /// </summary>
        /// <value>
        ///     The number of restrictions to meet.
        /// </value>
        public int RestrictionsToMeet { get; set; }

        /// <summary>
        ///     Gets or sets the routes.
        /// </summary>
        /// <value>
        ///     The routes.
        /// </value>
        public List<Route> Routes { get; set; }
    }
}