using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DTO;

namespace Controllers.SequentailOrdering
{
    /// <summary>
    ///     Represents ant.
    /// </summary>
    public class Ant
    {
        /// <summary>
        ///     The flag representing information about whether to allow to pick route from best solution only by deterministic
        ///     rule.
        /// </summary>
        private readonly bool _allowToPickRouteFromBestSolutionOnlyByDeterministicRule;

        /// <summary>
        ///     The flag representing information about whether to break expensive solutions.
        /// </summary>
        private readonly bool _breakExpensiveSolutions;

        /// <summary>
        ///     The flag representing information about whether to decrease heuristic meaning in time.
        /// </summary>
        private readonly bool _decreaseHeuristicMeaningInTime;

        /// <summary>
        ///     The flag representing information about whether to don't use heuristic.
        /// </summary>
        private readonly bool _dontUseHeuristic;

        /// <summary>
        ///     The _The flag representing information about whether to force at least one route change.
        /// </summary>
        private readonly bool _forceAtLeastOneRouteChange;

        /// <summary>
        ///     Represents the problem data.
        /// </summary>
        private readonly SequentialOrderingProblem _problem;

        /// <summary>
        ///     The random generator instance.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        ///     The flag representing information about whether to use candidate list.
        /// </summary>
        private readonly bool _useCandidateList;

        /// <summary>
        ///     The flag representing information about whether to use met restrictions counter
        /// </summary>
        private readonly bool _useMetRestrictionsCounter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Ant" /> class.
        /// </summary>
        /// <param name="random">The random generator instance.</param>
        /// <param name="allowToPickRouteFromBestSolutionOnlyByDeterministicRule">
        ///     If set to <c>true</c> routes from best solution
        ///     will get picked only by deterministic rule.
        ///     The only exception from this rule is case when route from the best solution is the only feasible route.
        /// </param>
        /// <param name="breakExpensiveSolutions">
        ///     if set to <c>true</c> then when builded solution will exceed cost of (1.2 * [best
        ///     solution cost]) further building won't take place.
        /// </param>
        /// <param name="decreaseHeuristicInTime">if set to <c>true</c> then heuristic importance will decrease in time.</param>
        /// <param name="doNotUseHeuristic">if set to <c>true</c> heuristic won't be used.</param>
        /// <param name="forceRouteChange">
        ///     if set to <c>true</c> then ant will try to force at least one route change in relation to
        ///     the best solution found.
        /// </param>
        /// <param name="useCandidateList">if set to <c>true</c> candidate list will be used.</param>
        /// <param name="useMetRestrictionsCounter">
        ///     if set to <c>true</c> met restrictions counter will be used to determine
        ///     whether route is feasible.
        /// </param>
        /// <param name="problem">The problem.</param>
        public Ant(Random random, bool allowToPickRouteFromBestSolutionOnlyByDeterministicRule,
            bool breakExpensiveSolutions,
            bool decreaseHeuristicInTime, bool doNotUseHeuristic, bool forceRouteChange, bool useCandidateList,
            bool useMetRestrictionsCounter, SequentialOrderingProblem problem)
        {
            _random = random;
            _allowToPickRouteFromBestSolutionOnlyByDeterministicRule =
                allowToPickRouteFromBestSolutionOnlyByDeterministicRule;
            _breakExpensiveSolutions = breakExpensiveSolutions;
            _decreaseHeuristicMeaningInTime = decreaseHeuristicInTime;
            _dontUseHeuristic = doNotUseHeuristic;
            _forceAtLeastOneRouteChange = forceRouteChange;
            _useCandidateList = useCandidateList;
            _useMetRestrictionsCounter = useMetRestrictionsCounter;
            _problem = problem;
        }

        /// <summary>
        ///     Gets or sets the best known solution.
        /// </summary>
        /// <value>
        ///     The best known solution.
        /// </value>
        public Solution BestKnownSolution { get; set; }

        /// <summary>
        ///     Gets or sets the length of the candidate list.
        /// </summary>
        /// <value>
        ///     The length of the candidate list.
        /// </value>
        public int CandidateListLength { get; set; }

        /// <summary>
        ///     Gets or sets the heuristic meaning.
        /// </summary>
        /// <value>
        ///     The heuristic meaning.
        /// </value>
        public double HeuristicMeaning { get; set; }

        /// <summary>
        ///     Gets or sets the heuristic meaning decrease factor.
        /// </summary>
        /// <value>
        ///     The heuristic meaning decrease factor.
        /// </value>
        public double HeuristicMeaningDecreaseFactor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to take next best route if none is feasible in candidate list.
        /// </summary>
        /// <value>
        ///     <c>true</c> if take next best route if none is feasible in candidate list; otherwise, <c>false</c>.
        /// </value>
        public bool IfNoneIsFeasibleInCandidateListTakeNextBest { get; set; }

        /// <summary>
        ///     Gets or sets the index.
        /// </summary>
        /// <value>
        ///     The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        ///     Gets or sets the pheromone manipulator.
        /// </summary>
        /// <value>
        ///     The pheromone manipulator.
        /// </value>
        public PheromoneManipulator PheromoneManipulator { get; set; }

        /// <summary>
        ///     Gets or sets the probability of deterministic rule use.
        /// </summary>
        /// <value>
        ///     The probability of deterministic rule use.
        /// </value>
        public double ProbabilityOfDeterministicRuleUse { get; set; }

        /// <summary>
        ///     Gets or sets the solution.
        /// </summary>
        /// <value>
        ///     The solution.
        /// </value>
        public Solution Solution { get; set; }

        /// <summary>
        ///     Gets how many times used best solution route.
        /// </summary>
        /// <value>
        ///     How many times used best solution route.
        /// </value>
        public int TimesUsedBestSolutionRoute { get; private set; }

        /// <summary>
        ///     Finds the path.
        /// </summary>
        internal void FindPath()
        {
            Solution = new Solution(_problem);

            var indexOfRouteToForceChange = GetIndexOfRouteToForceChange(_problem);
            Route currentRoute = null;
            TimesUsedBestSolutionRoute = 0;
            for (var i = 0; i < _problem.NumberOfEdges; i++)
            {
                // Force route change if no changes (comparing to the best solution so far) were made up to this index.
                var forceRouteChange = i == indexOfRouteToForceChange && TimesUsedBestSolutionRoute == i;
                currentRoute = PickRoute(currentRoute, forceRouteChange);
                // Forcing route change may fail when the route from best solution is the only feasible way.
                var didSuccessfulySkipBestSolutionRoute = !forceRouteChange || (TimesUsedBestSolutionRoute == i);
                // If forcing route change failed try to force it again in next iteration.
                if (!didSuccessfulySkipBestSolutionRoute)
                {
                    indexOfRouteToForceChange++;
                }

                FollowTheRoute(currentRoute);

                if (_breakExpensiveSolutions && BestKnownSolution != null &&
                    BestKnownSolution.Cost*1.2 < Solution.Cost)
                {
                    break;
                }
            }

            if (_decreaseHeuristicMeaningInTime)
            {
                HeuristicMeaning *= (1 - HeuristicMeaningDecreaseFactor);
            }

            // Reset "IsVisited" flag for all targets.
            _problem.Targets.ForEach(target => target.IsVisited = false);
            MarkAllRestrictionsAsNotMet();
        }

        /// <summary>
        ///     Resets this instance.
        /// </summary>
        internal void Reset()
        {
            Solution = null;
        }

        /// <summary>
        ///     Gets the route with highest rank.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The route with highest rank.</returns>
        private static Route GetRouteWithHighestRank(IEnumerable<AntOption> options)
        {
            return options.Aggregate((o1, o2) => o1.Rank > o2.Rank ? o1 : o2).Route;
        }

        /// <summary>
        ///     Appraises the route.
        /// </summary>
        /// <param name="possibleRoute">The possible route.</param>
        /// <returns>The rank of the route.</returns>
        private double AppraiseRoute(Route possibleRoute)
        {
            return possibleRoute.PheromoneAmount*GetQualityMeasurement(possibleRoute);
        }

        /// <summary>
        ///     Ares all restrictions met.
        /// </summary>
        /// <param name="possibleRoute">The possible route.</param>
        /// <returns><c>true</c> if all restrictions are met</returns>
        private bool AreAllRestrictionsMet(Route possibleRoute)
        {
            if (_useMetRestrictionsCounter)
            {
                return possibleRoute.Target.RestrictionsToMeet == 0;
            }

            return (possibleRoute.Target.Restrictions.All(restriction => restriction.IsMet));
        }

        /// <summary>
        ///     Creates the ant option.
        /// </summary>
        /// <param name="possibleRoute">The possible route.</param>
        /// <returns>Option for ant.</returns>
        private AntOption CreateAntOption(Route possibleRoute)
        {
            if (!possibleRoute.Target.IsVisited)
            {
                var areAllRestrictionsMet = AreAllRestrictionsMet(possibleRoute);

                if (areAllRestrictionsMet)
                {
                    var rank = AppraiseRoute(possibleRoute);

                    return (new AntOption
                    {
                        Route = possibleRoute,
                        Rank = rank
                    });
                }
            }

            return null;
        }

        /// <summary>
        ///     Draws the route.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Picked route.</returns>
        private Route DrawRoute(IReadOnlyList<AntOption> options)
        {
            double probabilitySum = 0;
            var i = 0;
            var randomNumber = _random.NextDouble();
            while (probabilitySum < randomNumber)
            {
                probabilitySum += options[i].Probability;
                i++;
            }

            return options[i - 1].Route;
        }

        /// <summary>
        ///     Follows the route.
        /// </summary>
        /// <param name="routeToGo">The route to go.</param>
        private void FollowTheRoute(Route routeToGo)
        {
            Solution.Add(routeToGo);
            PheromoneManipulator.EatPheromone(routeToGo);
            routeToGo.Target.IsVisited = true;
            MarkConditionsDependingOnTargetAsMet(routeToGo.Target);
        }

        /// <summary>
        ///     Gets the index of route to force change.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <returns>Index of route to force change.</returns>
        private int GetIndexOfRouteToForceChange(SequentialOrderingProblem problem)
        {
            if (_forceAtLeastOneRouteChange)
            {
                return (int) Math.Round(_random.NextDouble()*(problem.NumberOfEdges - 1));
            }

            return -1;
        }

        /// <summary>
        ///     Gets the next shortest route as options list.
        /// </summary>
        /// <param name="openingTarget">The opening target.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="routeToSkip">The route to skip.</param>
        /// <returns>Next feasible route as list of options.</returns>
        /// <remarks>May return <see cref="routeToSkip" /> if it is the only feasible route.</remarks>
        private List<AntOption> GetNextShortestRouteAsOptionsList(Target openingTarget, int startIndex,
            Route routeToSkip)
        {
            var options = new List<AntOption>();
            var currentIndex = startIndex;
            while (options.Count == 0 && currentIndex < openingTarget.Routes.Count)
            {
                options = GetOptions(openingTarget, routeToSkip, currentIndex, currentIndex);
                currentIndex++;
            }

            return options;
        }

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <param name="openingTarget">The opening target.</param>
        /// <param name="routeToSkip">The route to skip.</param>
        /// <returns>List of ant options.</returns>
        /// <remarks>May return <see cref="routeToSkip" /> if it is the only feasible route.</remarks>
        private List<AntOption> GetOptions(Target openingTarget, Route routeToSkip = null)
        {
            List<AntOption> result;
            if (_useCandidateList)
            {
                result = GetOptionsUsingCandidateList(openingTarget, routeToSkip);
            }
            else
            {
                result = GetOptions(openingTarget, routeToSkip, 0, openingTarget.Routes.Count - 1);
            }

            // If the routeToSkip is the only feasible solution.
            if (result.Count == 0)
            {
                result.Add(CreateAntOption(routeToSkip));
                TimesUsedBestSolutionRoute++;
                return result;
            }

            return result;
        }

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <param name="openingTarget">The opening target.</param>
        /// <param name="routeToSkip">The route to skip.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="maxIndex">The maximum index.</param>
        /// <returns>List of ant options.</returns>
        /// <remarks>May return empty list if <see cref="routeToSkip" /> is the only feasible route.</remarks>
        private List<AntOption> GetOptions(Target openingTarget, Route routeToSkip, int startIndex, int maxIndex)
        {
            var result = new List<AntOption>();
            double allOptionsRankSum = 0;
            for (var i = startIndex; i <= maxIndex; i++)
            {
                var possibleRoute = openingTarget.Routes[i];
                if (possibleRoute == routeToSkip)
                    continue;
                var option = CreateAntOption(possibleRoute);
                if (option == null) continue;
                allOptionsRankSum += option.Rank;
                result.Add(option);
            }

            foreach (var antOption in result)
            {
                antOption.AllOptionsRankSum = allOptionsRankSum;
            }

            return result;
        }

        /// <summary>
        ///     Gets the options using candidate list.
        /// </summary>
        /// <param name="openingTarget">The opening target.</param>
        /// <param name="routeToSkip">The route to skip.</param>
        /// <returns>List of ant options.</returns>
        /// <remarks>May return <see cref="routeToSkip" /> if it is the only feasible route.</remarks>
        private List<AntOption> GetOptionsUsingCandidateList(Target openingTarget, Route routeToSkip)
        {
            int maxIndex;
            if (CandidateListLength < openingTarget.Routes.Count)
            {
                maxIndex = CandidateListLength - 1;
            }
            else
            {
                maxIndex = openingTarget.Routes.Count - 1;
            }

            var options = GetOptions(openingTarget, routeToSkip, 0, maxIndex);
            // If there were no feasible routes in candidate list (or the only one was the one to skip).
            if (options.Count == 0)
            {
                if (IfNoneIsFeasibleInCandidateListTakeNextBest)
                {
                    options = GetNextShortestRouteAsOptionsList(openingTarget, maxIndex + 1, routeToSkip);
                }
                else
                {
                    options = GetOptions(openingTarget, routeToSkip, maxIndex + 1, openingTarget.Routes.Count - 1);
                }
            }

            return options;
        }

        /// <summary>
        ///     Gets the quality measurement of route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>Quality measurement of route</returns>
        private double GetQualityMeasurement(Route route)
        {
            Debug.Assert(route != null, "route == null");

            if (_dontUseHeuristic) return 1;

            if (route.Cost == 0) return 1/0.5;

            if (_decreaseHeuristicMeaningInTime)
            {
                return 1/Math.Pow(route.Cost, HeuristicMeaning);
            }

            return 1/route.Cost;
        }

        /// <summary>
        ///     Gets the route by probabilistic rule.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="forceRouteChange">
        ///     if set to <c>true</c> ant will try to skip the route from best known solution unless
        ///     it's the only feasible route.
        /// </param>
        /// <returns>Route picked by probabilistic rule.</returns>
        private Route GetRouteByProbabilisticRule(Target origin, bool forceRouteChange)
        {
            Route routeFromBestKnownSolution = null;
            if (_allowToPickRouteFromBestSolutionOnlyByDeterministicRule || forceRouteChange)
            {
                routeFromBestKnownSolution = GetRouteFromBestKnownSolution(origin);
            }

            var options = GetOptions(origin, routeFromBestKnownSolution);

            return DrawRoute(options);
        }

        /// <summary>
        ///     Gets the route from best known solution.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns>The route from best known solution.</returns>
        private Route GetRouteFromBestKnownSolution(Target origin)
        {
            if (BestKnownSolution == null) return null;
            return BestKnownSolution.Routes.SingleOrDefault(route => route.Origin == origin);
        }

        /// <summary>
        ///     Determines whether the specified route is feasible.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns><c>true</c> if specified route is feasible.</returns>
        private bool IsRouteFeasible(Route route)
        {
            return route != null && !route.Target.IsVisited && AreAllRestrictionsMet(route);
        }

        /// <summary>
        ///     Marks all restrictions as not met.
        /// </summary>
        private void MarkAllRestrictionsAsNotMet()
        {
            if (_useMetRestrictionsCounter)
            {
                foreach (var target in _problem.Targets)
                {
                    target.RestrictionsToMeet = target.Restrictions.Count;
                }
            }
            else
            {
                foreach (var restriction in _problem.Targets.SelectMany(target => target.Restrictions))
                {
                    restriction.IsMet = false;
                }
            }
        }

        /// <summary>
        ///     Marks the conditions depending on target as met.
        /// </summary>
        /// <param name="target">The target.</param>
        private void MarkConditionsDependingOnTargetAsMet(Target target)
        {
            if (_useMetRestrictionsCounter)
            {
                target.RequiredBy.ForEach(restriction => restriction.TargetToVisit.RestrictionsToMeet--);
            }
            else
            {
                target.RequiredBy.ForEach(restriction => restriction.IsMet = true);
            }
        }

        /// <summary>
        ///     Picks the route.
        /// </summary>
        /// <param name="previousRoute">The previous route.</param>
        /// <param name="forceRouteChange">
        ///     If set to <c>true</c> ant will try to skip the route from best known solution unless
        ///     it's the only feasible route.
        /// </param>
        /// <returns>Picked route.</returns>
        private Route PickRoute(Route previousRoute, bool forceRouteChange)
        {
            Target origin;
            if (previousRoute == null)
            {
                origin = _problem.Targets[0];
                origin.IsVisited = true;
            }
            else
            {
                origin = previousRoute.Target;
            }

            var routeByDeterministicRule = forceRouteChange ? null : TryToGetRouteByDeterministicRule(origin);
            return routeByDeterministicRule ?? GetRouteByProbabilisticRule(origin, forceRouteChange);
        }

        /// <summary>
        ///     Tries to get route by deterministic rule.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns>Route picked by deterministic rule. Null if the deterministic rule was not meant to be used.</returns>
        private Route TryToGetRouteByDeterministicRule(Target origin)
        {
            var useDeterministicRule = _random.NextDouble() <= ProbabilityOfDeterministicRuleUse;
            if (!useDeterministicRule) return null;

            var routeFromBestKnownSolution = GetRouteFromBestKnownSolution(origin);

            if (IsRouteFeasible(routeFromBestKnownSolution))
            {
                TimesUsedBestSolutionRoute++;
                return routeFromBestKnownSolution;
            }

            var options = GetOptions(origin);
            return GetRouteWithHighestRank(options);
        }
    }
}