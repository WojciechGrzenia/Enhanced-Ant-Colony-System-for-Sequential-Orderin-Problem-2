using System;
using System.Collections.Generic;
using System.Diagnostics;
using DTO;

namespace Controllers.SequentailOrdering
{
    /// <summary>
    ///     Controller for sequential ordering problem.
    /// </summary>
    public class SequentialOrderingController
    {
        /// <summary>
        ///     The best solution found.
        /// </summary>
        private Solution _bestSolution;

        /// <summary>
        ///     The expected number of probabilistic rule uses. In some cases may be dynamic.
        /// </summary>
        private double _expectedNumberOfProbabilisticRuleUses;

        /// <summary>
        ///     The number of iterations without improvement.
        /// </summary>
        private int _numberOfIterationsWithoutImprovement;

        /// <summary>
        ///     The pheromone manipulator.
        /// </summary>
        private PheromoneManipulator _pheromoneManipulator;

        /// <summary>
        ///     The problem.
        /// </summary>
        private SequentialOrderingProblem _problem;

        /// <summary>
        ///     The solution improver.
        /// </summary>
        private SolutionImprover _solutionImprover;

        /// <summary>
        ///     Gets the number of iterations.
        /// </summary>
        /// <value>
        ///     The number of iterations.
        /// </value>
        public int NumberOfIterations { get; private set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public SequentialOrderingParameters Parameters { get; set; }

        /// <summary>
        ///     Gets or sets the stopwatch.
        /// </summary>
        /// <value>
        ///     The stopwatch used to check if computation time is up.
        /// </value>
        private Stopwatch Stopwatch { get; set; }

        /// <summary>
        ///     Gets or sets the ants.
        /// </summary>
        /// <value>
        ///     The ants.
        /// </value>
        private List<Ant> Ants { get; set; }

        /// <summary>
        ///     Resolves the specified problem.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <returns>Best found solution for the problem.</returns>
        public Solution Resolve(SequentialOrderingProblem problem)
        {
            Initialize(problem);
            if (Parameters.UseEarlySolutionForInitialPheromoneAmount)
            {
                SendAnts(false);
                // May be null when time was up before first solution was build.
                if (_bestSolution != null)
                {
                    SetPheromoneParametersFromEarlySolution();
                }
            }

            while (Stopwatch.Elapsed < Parameters.CalculationTime)
            {
                ResetAnts();
                var isBetterSolutionFound = SendAnts();
                if (isBetterSolutionFound)
                {
                    _numberOfIterationsWithoutImprovement = 0;
                }
                else
                {
                    _numberOfIterationsWithoutImprovement++;
                }

                _pheromoneManipulator.ApplyGlobalUpdatingRule(Parameters.EvaporatePheromones,
                    _bestSolution, Parameters.UseRouteCostInEvaporation,
                    Parameters.UseRouteCostInAddingPheromone);

                if (Parameters.UseDynamicNumberOfProbabilisticRuleUses)
                {
                    if (Parameters.ChangeNumberOfProbabilisticRuleUsesInTime)
                    {
                        _expectedNumberOfProbabilisticRuleUses *=
                            Parameters.TimeProbabilisticChangeFactor;
                    }

                    var probability = GetProbabilityOfChoosingTheBestRoute();
                    foreach (var ant in Ants)
                    {
                        ant.ProbabilityOfDeterministicRuleUse = probability;
                    }
                }

                NumberOfIterations++;
            }

            Stopwatch.Stop();

            return _bestSolution;
        }

        /// <summary>
        ///     Gets the probability of choosing the best route.
        /// </summary>
        /// <returns>The probability of choosing the best route.</returns>
        private double GetProbabilityOfChoosingTheBestRoute()
        {
            double finalExpectedNumberOfProbabilisticRuleUses = 0;
            if (Parameters.UseDynamicNumberOfProbabilisticRuleUses)
            {
                if (Parameters.IncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement)
                {
                    finalExpectedNumberOfProbabilisticRuleUses = Parameters.NoImprovementProbabilisticIncreaseFactor*
                                                                 _numberOfIterationsWithoutImprovement;
                }
            }

            finalExpectedNumberOfProbabilisticRuleUses += _expectedNumberOfProbabilisticRuleUses;

            // Check if the expected number of edges did exceed the border value.
            if (Parameters.UseDynamicNumberOfProbabilisticRuleUses &&
                finalExpectedNumberOfProbabilisticRuleUses > _problem.NumberOfEdges*Parameters.ProbabilisticChangeBorder)
                finalExpectedNumberOfProbabilisticRuleUses = _problem.NumberOfEdges*Parameters.ProbabilisticChangeBorder;

            return 1 - (finalExpectedNumberOfProbabilisticRuleUses/_problem.NumberOfEdges);
        }

        /// <summary>
        ///     Initializes instance basing on specified problem.
        /// </summary>
        /// <param name="problem">The problem.</param>
        private void Initialize(SequentialOrderingProblem problem)
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            _pheromoneManipulator = new PheromoneManipulator(problem)
            {
                CoefficientOfEvaporation = Parameters.CoefficientOfEvaporation,
                EatParameter = Parameters.PheromoneEatParameter,
                InitialValueOfTrails = Parameters.InitialPheromoneAmount
            };

            NumberOfIterations = 0;
            _numberOfIterationsWithoutImprovement = 0;
            _expectedNumberOfProbabilisticRuleUses = Parameters.ExpectedNumberOfProbabilisticRuleUses;
            _problem = problem;
            ResetPheromoneAmount();
            _bestSolution = null;
            Ants = null;
            _solutionImprover = new SolutionImprover(problem, Parameters.EatPheromoneDuringLocalSearch,
                Parameters.UseChangedIndexesInStackInitialization)
            {
                PheromoneManipulator = _pheromoneManipulator
            };

            ResetAnts();
        }

        /// <summary>
        ///     Resets the ants.
        /// </summary>
        private void ResetAnts()
        {
            if (Ants == null)
            {
                Ants = new List<Ant>(Parameters.NumberOfAnts);

                var random = new Random();
                var probability = GetProbabilityOfChoosingTheBestRoute();
                // If we should use early solution length we must wait until it's found and then set the pheromone eating parameters.
                if (Parameters.UseEarlySolutionForInitialPheromoneAmount)
                {
                    _pheromoneManipulator.InitialValueOfTrails = 0;
                    _pheromoneManipulator.EatParameter = 0;
                }
                else
                {
                    _pheromoneManipulator.InitialValueOfTrails = Parameters.InitialPheromoneAmount;
                    _pheromoneManipulator.EatParameter = Parameters.PheromoneEatParameter;
                }

                for (var i = 0; i < Parameters.NumberOfAnts; i++)
                {
                    var ant = new Ant(random, Parameters.AllowToPickBestRouteOnlyByDeterministicRule,
                        Parameters.BreakExpensiveSolutions, Parameters.DecreaseHeuristicMeaningInTime,
                        Parameters.DontUseHeuristic, Parameters.ForceAtLeastOneRouteChange,
                        Parameters.CandidateListParameters.UseCandidateList,
                        Parameters.UseMetRestrictionsCounter, _problem)
                    {
                        ProbabilityOfDeterministicRuleUse = probability,
                        Index = i,
                        IfNoneIsFeasibleInCandidateListTakeNextBest =
                            Parameters.CandidateListParameters.IfNoneIsFeasibleTakeNextBest,
                        HeuristicMeaning = Parameters.InitialHeuristicMeaning,
                        HeuristicMeaningDecreaseFactor = Parameters.HeuristicMeaningDecreaseFactor,
                        PheromoneManipulator = _pheromoneManipulator
                    };

                    if (Parameters.CandidateListParameters.UseFixedSize)
                    {
                        ant.CandidateListLength = Parameters.CandidateListParameters.Size;
                    }
                    else
                    {
                        ant.CandidateListLength =
                            (int)
                                Math.Round(Parameters.CandidateListParameters.FractionOfMaxSize*_problem.NumberOfEdges);
                    }

                    if (ant.CandidateListLength < 2)
                    {
                        ant.CandidateListLength = 2;
                    }

                    Ants.Add(ant);
                }
            }
            else
            {
                foreach (var existingAnt in Ants)
                {
                    existingAnt.Reset();
                }
            }
        }

        /// <summary>
        ///     Resets the pheromone amount.
        /// </summary>
        private void ResetPheromoneAmount()
        {
            _pheromoneManipulator.SetPheromoneAmount(
                Parameters.UseEarlySolutionForInitialPheromoneAmount
                    ? 1
                    : Parameters.InitialPheromoneAmount);
        }

        /// <summary>
        ///     Sends the ants.
        /// </summary>
        /// <param name="useHasSop">if set to <c>true</c> HAS-SOP algorithm will be used.</param>
        /// <returns><c>true</c> if better solution is found.</returns>
        private bool SendAnts(bool useHasSop = true)
        {
            var isBetterSolutionFound = false;
            var antsToSend = new Queue<Ant>(Ants);
            for (var i = 0; i < Ants.Count; i++)
            {
                var ant = antsToSend.Dequeue();
                ant.FindPath();
                if (!ant.Solution.IsComplete) continue;

                if (_bestSolution == null || ant.Solution.Cost <= _bestSolution.Cost*1.2)
                {
                    if (useHasSop)
                    {
                        _solutionImprover.ImproveSolution(ant.Solution, _bestSolution);
                    }

                    if (Stopwatch.Elapsed > Parameters.CalculationTime)
                    {
                        break;
                    }

                    var didAntFindBetterSolution = _bestSolution == null ||
                                                   ant.Solution.Cost < _bestSolution.Cost;
                    var didAntFindAnotherSolutionWithTheSameCost = Parameters.AcceptSolutionWithTheSameCost &&
                                                                   _bestSolution != null &&
                                                                   ant.Solution.Cost == _bestSolution.Cost &&
                                                                   ant.TimesUsedBestSolutionRoute != _problem.NumberOfEdges;

                    if (didAntFindBetterSolution || didAntFindAnotherSolutionWithTheSameCost)
                    {
                        if (didAntFindBetterSolution)
                        {
                            isBetterSolutionFound = true;
                        }

                        _bestSolution = ant.Solution;
                        UpdateAntsWithBestSolutionObject();
                    }
                }
            }

            return isBetterSolutionFound;
        }

        /// <summary>
        ///     Sets the pheromone parameters from early solution.
        /// </summary>
        private void SetPheromoneParametersFromEarlySolution()
        {
            var initialPheromoneAmount = 1 / (_bestSolution.Cost*_problem.NumberOfEdges);
            if (Parameters.UseRouteCostInSettingInitialPheromoneAmount)
            {
                _pheromoneManipulator.SetPheromoneAmount(initialPheromoneAmount,
                    _bestSolution.AverageRouteCost);
            }
            else
            {
                _pheromoneManipulator.SetPheromoneAmount(initialPheromoneAmount);
            }

            _pheromoneManipulator.EatParameter = Parameters.PheromoneEatParameter;
            _pheromoneManipulator.InitialValueOfTrails = initialPheromoneAmount;
        }

        /// <summary>
        ///     Updates the ants with best solution object.
        /// </summary>
        private void UpdateAntsWithBestSolutionObject()
        {
            if (Parameters.ResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound)
            {
                _expectedNumberOfProbabilisticRuleUses = Parameters.ExpectedNumberOfProbabilisticRuleUses;
            }
            foreach (var ant in Ants)
            {
                ant.BestKnownSolution = _bestSolution;
                if (Parameters.ResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound)
                {
                    ant.ProbabilityOfDeterministicRuleUse = GetProbabilityOfChoosingTheBestRoute();
                }
            }
        }
    }
}