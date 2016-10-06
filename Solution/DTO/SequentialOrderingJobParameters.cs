using System;
using System.Linq;

namespace DTO
{
    /// <summary>
    ///     Parameters for computational job.
    /// </summary>
    [Serializable]
    public class SequentialOrderingJobParameters : SequentialOrderingParameters
    {
        /// <summary>
        ///     Gets or sets the calculation time of tester.
        /// </summary>
        /// <value>
        ///     The calculation time of tester.
        /// </value>
        public TimeSpan CalculationTimeOfTester { get; set; }

        /// <summary>
        ///     Gets or sets the name of the configuration.
        /// </summary>
        /// <value>
        ///     The name of the configuration.
        /// </value>
        public string ConfigurationName { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is test job.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is test job; otherwise, <c>false</c>.
        /// </value>
        public bool IsTestJob
        {
            get
            {
                var testProperties = GetType().GetProperties().Where(property => property.Name.StartsWith("Test"));
                return testProperties.Any(property => (bool) property.GetValue(this));
            }
        }

        /// <summary>
        ///     Gets or sets the number of trials.
        /// </summary>
        /// <value>
        ///     The number of trials.
        /// </value>
        public int NumberOfTrials { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [accept solution with the same cost] parameter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [accept solution with the same cost] parameter; otherwise, <c>false</c>.
        /// </value>
        public bool TestAcceptSolutionWithTheSameCost { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [allow to pick best route only by deterministic rule] parameter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [allow to pick best route only by deterministic rule] parameter; otherwise,
        ///     <c>false</c>.
        /// </value>
        public bool TestAllowToPickBestRouteOnlyByDeterministicRule { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [break expensive solution] parameter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [break expensive solution] parameter; otherwise, <c>false</c>.
        /// </value>
        public bool TestBreakExpensiveSolution { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [change number of probabilistic rule uses in time] parameter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [change number of probabilistic rule uses in time] parameter; otherwise,
        ///     <c>false</c>.
        /// </value>
        public bool TestChangeNumberOfProbabilisticRuleUsesInTime { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [coefficient of evaporation] parameter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [coefficient of evaporation] parameter; otherwise, <c>false</c>.
        /// </value>
        public bool TestCoefficientOfEvaporation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [decrease heuristic meaning in time] parameter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [decrease heuristic meaning in time] parameter; otherwise, <c>false</c>.
        /// </value>
        public bool TestDecreaseHeuristicMeaningInTime { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [don't use heuristic].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [don't use heuristic]; otherwise, <c>false</c>.
        /// </value>
        public bool TestDontUseHeuristic { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [eat pheromone during local search].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [eat pheromone during local search]; otherwise, <c>false</c>.
        /// </value>
        public bool TestEatPheromoneDuringLocalSearch { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [evaporate pheromones].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [evaporate pheromones]; otherwise, <c>false</c>.
        /// </value>
        public bool TestEvaporatePheromones { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [expected number of probabilistic rule uses].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [expected number of probabilistic rule uses]; otherwise, <c>false</c>.
        /// </value>
        public bool TestExpectedNumberOfProbabilisticRuleUses { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [fixed size].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [fixed size]; otherwise, <c>false</c>.
        /// </value>
        public bool TestFixedSize { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [force at least one route change].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [force at least one route change]; otherwise, <c>false</c>.
        /// </value>
        public bool TestForceAtLeastOneRouteChange { get; set; }


        /// <summary>
        ///     Gets or sets a value indicating whether to test [fraction of maximum size].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [fraction of maximum size]; otherwise, <c>false</c>.
        /// </value>
        public bool TestFractionOfMaxSize { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [heuristic meaning decrease factor].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [heuristic meaning decrease factor]; otherwise, <c>false</c>.
        /// </value>
        public bool TestHeuristicMeaningDecreaseFactor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [if none is feasible take next best].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [if none is feasible take next best]; otherwise, <c>false</c>.
        /// </value>
        public bool TestIfNoneIsFeasibleTakeNextBest { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [increase number of probabilistic rule uses when no improvement].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [increase number of probabilistic rule uses when no improvement]; otherwise,
        ///     <c>false</c>.
        /// </value>
        public bool TestIncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [initial heuristic meaning].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [initial heuristic meaning]; otherwise, <c>false</c>.
        /// </value>
        public bool TestInitialHeuristicMeaning { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [initial pheromone amount].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [initial pheromone amount]; otherwise, <c>false</c>.
        /// </value>
        public bool TestInitialPheromoneAmount { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [no improvement probabilistic increase factor].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [no improvement probabilistic increase factor]; otherwise, <c>false</c>.
        /// </value>
        public bool TestNoImprovementProbabilisticIncreaseFactor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [number of ants].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [number of ants]; otherwise, <c>false</c>.
        /// </value>
        public bool TestNumberOfAnts { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [pheromone eat parameter].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [pheromone eat parameter]; otherwise, <c>false</c>.
        /// </value>
        public bool TestPheromoneEatParameter { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [probabilistic change border].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [probabilistic change border]; otherwise, <c>false</c>.
        /// </value>
        public bool TestProbabilisticChangeBorder { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [reset expected number of probabilistic rule on best solution
        ///     found].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [reset expected number of probabilistic rule on best solution found]; otherwise,
        ///     <c>false</c>.
        /// </value>
        public bool TestResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [time probabilistic change factor].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [time probabilistic change factor]; otherwise, <c>false</c>.
        /// </value>
        public bool TestTimeProbabilisticChangeFactor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use candidate list].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use candidate list]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseCandidateList { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use changed indexes in stack initialization].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use changed indexes in stack initialization]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseChangedIndexesInStackInitialization { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use dynamic number of probabilistic rule uses].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use dynamic number of probabilistic rule uses]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseDynamicNumberOfProbabilisticRuleUses { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use early solution for initial pheromone amount].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use early solution for initial pheromone amount]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseEarlySolutionForInitialPheromoneAmount { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use fixed size].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use fixed size]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseFixedSize { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use met restrictions counter].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use met restrictions counter]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseMetRestrictionsCounter { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use route cost in adding pheromone].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use route cost in adding pheromone]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseRouteCostInAddingPheromone { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use route cost in evaporation].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use route cost in evaporation]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseRouteCostInEvaporation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to test [use route cost in setting initial pheromone amount].
        /// </summary>
        /// <value>
        ///     <c>true</c> if job should test [use route cost in setting initial pheromone amount]; otherwise, <c>false</c>.
        /// </value>
        public bool TestUseRouteCostInSettingInitialPheromoneAmount { get; set; }
    }
}