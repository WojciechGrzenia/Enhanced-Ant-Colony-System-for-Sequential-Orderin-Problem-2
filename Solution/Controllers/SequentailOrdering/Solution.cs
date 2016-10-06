using System;
using System.Collections.Generic;
using System.Linq;
using DTO;

namespace Controllers.SequentailOrdering
{
    /// <summary>
    ///     Represents solution for sequential ordering problem.
    /// </summary>
    [Serializable]
    public class Solution
    {
        /// <summary>
        ///     The problem.
        /// </summary>
        private readonly SequentialOrderingProblem _problem;

        /// <summary>
        ///     The routes of the solution.
        /// </summary>
        private readonly List<Route> _routes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Solution" /> class.
        /// </summary>
        public Solution()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Solution" /> class.
        /// </summary>
        /// <param name="problem">The problem.</param>
        public Solution(SequentialOrderingProblem problem)
        {
            _routes = new List<Route>();
            Cost = 0;
            _problem = problem;
        }

        /// <summary>
        ///     Gets the average route cost.
        /// </summary>
        /// <value>
        ///     The average route cost.
        /// </value>
        public double AverageRouteCost
        {
            get { return Cost/_routes.Count; }
        }

        /// <summary>
        ///     Gets or sets the cost.
        /// </summary>
        /// <value>
        ///     The cost.
        /// </value>
        public double Cost { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is complete.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete
        {
            get { return _problem.NumberOfEdges == _routes.Count; }
        }

        /// <summary>
        ///     Gets the routes.
        /// </summary>
        /// <value>
        ///     The routes.
        /// </value>
        public IReadOnlyList<Route> Routes
        {
            get { return _routes; }
        }

        /// <summary>
        ///     Gets the targets.
        /// </summary>
        /// <returns>Returns solution as list of targets.</returns>
        public List<Target> GetTargets()
        {
            var result = _routes.Select(route => route.Origin).ToList();
            result.Add(_routes.Last().Target);
            return result;
        }

        /// <summary>
        ///     Performs the sop3 exchange.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <remarks>Points are left to right.</remarks>
        public void PerformSop3Exchange(int p1, int p2, int p3)
        {
            var targets = GetTargets();
            var routeFromP1 = _problem.GetRoute(targets[p1], targets[p2 + 1]);
            var pathRight = _routes.GetRange(p2 + 1, p3 - (p2 + 1));
            var routeFromP3 = _problem.GetRoute(targets[p3], targets[p1 + 1]);
            var pathLeft = _routes.GetRange(p1 + 1, p2 - (p1 + 1));
            var routeFromP2 = _problem.GetRoute(targets[p2], targets[p3 + 1]);

            // Remove routes that won't exist in new solution. Use method that updates total cost of solution.
            RemoveAt(p3);
            RemoveAt(p2);
            RemoveAt(p1);

            // Remove all routes between first point of cut up to the last one. The minus three because three elements were already removed.
            _routes.RemoveRange(p1, p3 + 1 - p1 - 3);

            //
            // Insert new routes and swapped paths in right order.
            //

            // Currently the list looks like this: ooooo||oooo, where o - not changed routes; | - marks the place where all the change in order appears.
            Insert(p1, routeFromP2);

            // Currently the list looks like this: ooooo|2|oooo, where 2 - route from p2.
            _routes.InsertRange(p1, pathLeft);

            // Currently the list looks like this: ooooo|lllll2|oooo, where l - route in pathLeft.
            Insert(p1, routeFromP3);

            // Currently the list looks like this: ooooo|3lllll2|oooo, where 3 - route from p3.
            _routes.InsertRange(p1, pathRight);

            // Currently the list looks like this: ooooo|rrrrr3lllll2|oooo, where r - route in pathRight.
            Insert(p1, routeFromP1);

            // Currently the list looks like this: ooooo|1rrrrr3lllll2|oooo, where 1 - route from p1.
        }

        /// <summary>
        ///     Adds the specified route to the solution.
        /// </summary>
        /// <param name="route">The route the solution.</param>
        internal void Add(Route route)
        {
            _routes.Add(route);
            Cost += route.Cost;
        }

        /// <summary>
        ///     Inserts the specified route at specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="route">The route.</param>
        internal void Insert(int index, Route route)
        {
            Cost += route.Cost;
            _routes.Insert(index, route);
        }

        /// <summary>
        ///     Removes route at specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void RemoveAt(int index)
        {
            Cost -= _routes[index].Cost;
            _routes.RemoveAt(index);
        }
    }
}