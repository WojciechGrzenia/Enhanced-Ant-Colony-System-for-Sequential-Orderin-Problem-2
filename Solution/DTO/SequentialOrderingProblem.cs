using System;
using System.Collections.Generic;

namespace DTO
{
    /// <summary>
    ///     Represents the sequential ordering problem.
    /// </summary>
    [Serializable]
    public class SequentialOrderingProblem
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SequentialOrderingProblem" /> class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public SequentialOrderingProblem(List<Target> targets)
        {
            Targets = targets;
            Routes = new Route[targets.Count, targets.Count];
            FillCostsTable();
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the number of edges.
        /// </summary>
        /// <value>
        ///     The number of edges.
        /// </value>
        public int NumberOfEdges
        {
            get { return Targets.Count - 1; }
        }

        /// <summary>
        ///     Gets or sets the routes.
        /// </summary>
        /// <value>
        ///     The routes.
        /// </value>
        public Route[,] Routes { get; set; }

        /// <summary>
        ///     Gets or sets the targets.
        /// </summary>
        /// <value>
        ///     The targets.
        /// </value>
        public List<Target> Targets { get; set; }

        /// <summary>
        ///     Gets the cost of a route between two targets.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>The cost of a route between two targets.</returns>
        public double GetCost(Target origin, Target destination)
        {
            return Routes[origin.Index, destination.Index].Cost;
        }

        /// <summary>
        ///     Gets the route between two targets.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>The route between two targets.</returns>
        public Route GetRoute(Target origin, Target destination)
        {
            return Routes[origin.Index, destination.Index];
        }

        /// <summary>
        ///     Fills the costs table.
        /// </summary>
        private void FillCostsTable()
        {
            foreach (var target in Targets)
            {
                foreach (var route in target.Routes)
                {
                    Routes[target.Index, route.Target.Index] = route;
                }
            }
        }
    }
}