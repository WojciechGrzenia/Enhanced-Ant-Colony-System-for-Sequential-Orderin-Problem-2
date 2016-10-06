using System;

namespace DTO
{
    /// <summary>
    ///     Represent sequential ordering parameters.
    /// </summary>
    [Serializable]
    public class SequentialOrderingParameters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SequentialOrderingParameters" /> class.
        /// </summary>
        public SequentialOrderingParameters()
        {
            CandidateListParameters = new CandidateListParameters();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to accept solution with the same cost.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should accept solution with the same cost; otherwise, <c>false</c>.
        /// </value>
        public bool AcceptSolutionWithTheSameCost { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to allow to pick best route only by deterministic rule.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should pick best route only by deterministic rule; otherwise, <c>false</c>.
        /// </value>
        public bool AllowToPickBestRouteOnlyByDeterministicRule { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to break expensive solutions.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ant should break expensive solutions; otherwise, <c>false</c>.
        /// </value>
        public bool BreakExpensiveSolutions { get; set; }

        /// <summary>
        ///     Gets or sets the calculation time.
        /// </summary>
        /// <value>
        ///     The calculation time.
        /// </value>
        public TimeSpan CalculationTime { get; set; }

        /// <summary>
        ///     Gets or sets the candidate list parameters.
        /// </summary>
        /// <value>
        ///     The candidate list parameters.
        /// </value>
        public CandidateListParameters CandidateListParameters { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to change number of probabilistic rule uses in time.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should change number of probabilistic rule uses in time; otherwise, <c>false</c>.
        /// </value>
        public bool ChangeNumberOfProbabilisticRuleUsesInTime { get; set; }

        /// <summary>
        ///     Gets or sets the coefficient of evaporation.
        /// </summary>
        /// <value>
        ///     The coefficient of evaporation.
        /// </value>
        public double CoefficientOfEvaporation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to decrease heuristic meaning in time.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should decrease heuristic meaning in time; otherwise, <c>false</c>.
        /// </value>
        public bool DecreaseHeuristicMeaningInTime { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to don't use heuristic].
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should not use heuristic; otherwise, <c>false</c>.
        /// </value>
        public bool DontUseHeuristic { get; set; }

        /// <summary>
        ///     Gets or sets the eat pheromone during local search parameter.
        /// </summary>
        /// <value>
        ///     The eat pheromone during local search parameter.
        /// </value>
        public EatPheromoneDuringLocalSearch EatPheromoneDuringLocalSearch { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to evaporate pheromones.
        /// </summary>
        /// <value>
        ///     <c>true</c> if pheromones should evaporate; otherwise, <c>false</c>.
        /// </value>
        public bool EvaporatePheromones { get; set; }

        /// <summary>
        ///     Gets or sets the expected number of probabilistic rule uses.
        /// </summary>
        /// <value>
        ///     The expected number of probabilistic rule uses.
        /// </value>
        public double ExpectedNumberOfProbabilisticRuleUses { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to force at least one route change.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should force at least one route change; otherwise, <c>false</c>.
        /// </value>
        public bool ForceAtLeastOneRouteChange { get; set; }

        /// <summary>
        ///     Gets or sets the heuristic meaning decrease factor.
        /// </summary>
        /// <value>
        ///     The heuristic meaning decrease factor.
        /// </value>
        public double HeuristicMeaningDecreaseFactor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to increase number of probabilistic rule uses when there is no improvement.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should increase number of probabilistic rule uses when there is no improvement; otherwise,
        ///     <c>false</c>.
        /// </value>
        public bool IncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement { get; set; }

        /// <summary>
        ///     Gets or sets the initial heuristic meaning.
        /// </summary>
        /// <value>
        ///     The initial heuristic meaning.
        /// </value>
        public double InitialHeuristicMeaning { get; set; }

        /// <summary>
        ///     Gets or sets the initial pheromone amount.
        /// </summary>
        /// <value>
        ///     The initial pheromone amount.
        /// </value>
        public double InitialPheromoneAmount { get; set; }

        /// <summary>
        ///     Gets or sets the no improvement probabilistic increase factor.
        /// </summary>
        /// <value>
        ///     The no improvement probabilistic increase factor.
        /// </value>
        public double NoImprovementProbabilisticIncreaseFactor { get; set; }

        /// <summary>
        ///     Gets or sets the number of ants.
        /// </summary>
        /// <value>
        ///     The number of ants.
        /// </value>
        public int NumberOfAnts { get; set; }


        /// <summary>
        ///     Gets or sets the pheromone eat parameter.
        /// </summary>
        /// <value>
        ///     The pheromone eat parameter.
        /// </value>
        public double PheromoneEatParameter { get; set; }

        /// <summary>
        ///     Gets or sets the probabilistic change border.
        /// </summary>
        /// <value>
        ///     The probabilistic change border.
        /// </value>
        public double ProbabilisticChangeBorder { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to reset expected number of probabilistic rule when best solution is found.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should reset expected number of probabilistic rule when best solution is found; otherwise,
        ///     <c>false</c>.
        /// </value>
        public bool ResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound { get; set; }

        /// <summary>
        ///     Gets or sets the time probabilistic change factor.
        /// </summary>
        /// <value>
        ///     The time probabilistic change factor.
        /// </value>
        public double TimeProbabilisticChangeFactor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use changed indexes in stack initialization.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should use changed indexes in stack initialization; otherwise, <c>false</c>.
        /// </value>
        public bool UseChangedIndexesInStackInitialization { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use dynamic number of probabilistic rule uses.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should use dynamic number of probabilistic rule uses; otherwise, <c>false</c>.
        /// </value>
        public bool UseDynamicNumberOfProbabilisticRuleUses { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use early solution for initial pheromone amount.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should use early solution for initial pheromone amount; otherwise, <c>false</c>.
        /// </value>
        public bool UseEarlySolutionForInitialPheromoneAmount { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use met restrictions counter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ant should use met restrictions counter; otherwise, <c>false</c>.
        /// </value>
        public bool UseMetRestrictionsCounter { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use route cost in adding pheromone.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should use route cost in adding pheromone; otherwise, <c>false</c>.
        /// </value>
        public bool UseRouteCostInAddingPheromone { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use route cost in evaporation.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should use route cost in evaporation; otherwise, <c>false</c>.
        /// </value>
        public bool UseRouteCostInEvaporation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use route cost in setting initial pheromone amount.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ants should use route cost in setting initial pheromone amount; otherwise, <c>false</c>.
        /// </value>
        public bool UseRouteCostInSettingInitialPheromoneAmount { get; set; }
    }
}