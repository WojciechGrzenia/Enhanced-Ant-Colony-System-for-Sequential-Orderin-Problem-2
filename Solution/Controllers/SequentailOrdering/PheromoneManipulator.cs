using System.Linq;
using DTO;

namespace Controllers.SequentailOrdering
{
    /// <summary>
    ///     Provides methods to manipulate pheromones.
    /// </summary>
    public class PheromoneManipulator
    {
        /// <summary>
        ///     The sequential ordering problem.
        /// </summary>
        private readonly SequentialOrderingProblem _problem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PheromoneManipulator" /> class.
        /// </summary>
        /// <param name="problem">The problem.</param>
        public PheromoneManipulator(SequentialOrderingProblem problem)
        {
            _problem = problem;
        }

        /// <summary>
        ///     Gets or sets the coefficient of evaporation.
        /// </summary>
        /// <value>
        ///     The coefficient of evaporation.
        /// </value>
        public double CoefficientOfEvaporation { get; set; }

        /// <summary>
        ///     Gets or sets the pheromone eat parameter.
        /// </summary>
        /// <value>
        ///     The eat pheromone parameter.
        /// </value>
        public double EatParameter { get; set; }

        /// <summary>
        ///     Gets or sets the initial value of trails.
        /// </summary>
        /// <value>
        ///     The initial value of trails.
        /// </value>
        public double InitialValueOfTrails { get; set; }

        /// <summary>
        ///     Eats the pheromone.
        /// </summary>
        /// <param name="route">The route.</param>
        public void EatPheromone(Route route)
        {
            route.PheromoneAmount = (1 - EatParameter) * route.PheromoneAmount + (EatParameter * InitialValueOfTrails);
        }

        /// <summary>
        ///     Sets the pheromone amount.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="averageCost">The average cost.</param>
        public void SetPheromoneAmount(double amount, double averageCost = -1)
        {
            foreach (var route in _problem.Targets.SelectMany(target => target.Routes))
            {
                var cost = route.Cost == 0 ? 0.5 : route.Cost;
                if (averageCost > 0)
                {
                    route.PheromoneAmount = amount * (cost / averageCost);
                }
                else
                {
                    route.PheromoneAmount = amount;
                }
            }
        }

        /// <summary>
        ///     Applies the global updating rule.
        /// </summary>
        /// <param name="evaporatePheromones">if set to <c>true</c> evaporation of pheromones occur.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="useRouteCostInEvaporation">if set to <c>true</c> route cost will be used in evaporation.</param>
        /// <param name="useRouteCostInAddingPheromone">if set to <c>true</c> route cost will be used in adding pheromone.</param>
        internal void ApplyGlobalUpdatingRule(bool evaporatePheromones, Solution solution,
            bool useRouteCostInEvaporation, bool useRouteCostInAddingPheromone)
        {
            foreach (var route in solution.Routes)
            {
                var cost = route.Cost == 0 ? 0.5 : route.Cost;
                if (evaporatePheromones)
                {
                    if (useRouteCostInEvaporation)
                    {
                        var newCoefficient = CoefficientOfEvaporation * (cost / solution.AverageRouteCost);
                        if (newCoefficient > 1)
                        {
                            newCoefficient = 1;
                        }

                        route.PheromoneAmount *= (1 - newCoefficient);
                    }
                    else
                    {
                        route.PheromoneAmount *= (1 - CoefficientOfEvaporation);
                    }
                }

                if (useRouteCostInAddingPheromone)
                {
                    route.PheromoneAmount += (cost / solution.AverageRouteCost) * CoefficientOfEvaporation /
                                             solution.Cost;
                }
                else
                {
                    route.PheromoneAmount += CoefficientOfEvaporation/solution.Cost;
                }
            }
        }
    }
}