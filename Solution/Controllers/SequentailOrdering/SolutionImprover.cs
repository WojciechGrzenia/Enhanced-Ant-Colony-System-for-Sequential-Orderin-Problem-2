using System.Collections.Generic;
using System.Linq;
using DTO;

namespace Controllers.SequentailOrdering
{
    /// <summary>
    ///     Allows to improve existing solution.
    /// </summary>
    public class SolutionImprover
    {
        /// <summary>
        ///     Indicates whether to eat pheromones on changed route, on whole path or don't eat them at all during improvement.
        /// </summary>
        private readonly EatPheromoneDuringLocalSearch _eatPheromone;

        /// <summary>
        ///     The problem.
        /// </summary>
        private readonly SequentialOrderingProblem _problem;

        /// <summary>
        ///     Indicates whether to use changed indexes in stack initialization.
        /// </summary>
        private readonly bool _useChangedIndexesInStackInitialization;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SolutionImprover" /> class.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="eatPheromone">
        ///     Indicates whether to eat pheromones on changed route, on whole path or don't eat them at all
        ///     during improvement.
        /// </param>
        /// <param name="useChangedIndexesInStackInitialization">
        ///     if set to <c>true</c> changed indexes will be used during stack
        ///     initialization.
        /// </param>
        public SolutionImprover(SequentialOrderingProblem problem, EatPheromoneDuringLocalSearch eatPheromone,
            bool useChangedIndexesInStackInitialization)
        {
            _problem = problem;
            _eatPheromone = eatPheromone;
            _useChangedIndexesInStackInitialization = useChangedIndexesInStackInitialization;
        }

        /// <summary>
        ///     Gets or sets the pheromone manipulator.
        /// </summary>
        /// <value>
        ///     The pheromone manipulator.
        /// </value>
        public PheromoneManipulator PheromoneManipulator { get; set; }

        /// <summary>
        ///     Improves the solution.
        /// </summary>
        /// <param name="solutionToImprove">The solution to improve.</param>
        /// <param name="bestKnownSolution">The best known solution.</param>
        public void ImproveSolution(
            Solution solutionToImprove,
            Solution bestKnownSolution)
        {
            var targets = solutionToImprove.GetTargets();
            var dontPushStack = GenerateDontPushStack(bestKnownSolution, targets, solutionToImprove);
            var countH = 0;
            var mark = new int[targets.Count];
            while (dontPushStack.Count != 0)
            {
                var h = dontPushStack.Pop();
                countH++;
                if (PerformSearch(true, h, countH, mark, ref targets, solutionToImprove, dontPushStack)) continue;
                countH++;
                PerformSearch(false, h, countH, mark, ref targets, solutionToImprove, dontPushStack);
            }
        }

        /// <summary>
        ///     Gets the next i.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>The next i.</returns>
        private static int GetNextI(int i, bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                return i + 1;
            }

            return i - 1;
        }

        /// <summary>
        ///     Gets the next j.
        /// </summary>
        /// <param name="j">The j.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>The next j.</returns>
        private static int GetNextJ(int j, bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                return j + 1;
            }

            return j - 1;
        }

        /// <summary>
        ///     Gets the initial value for i.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>The initial value for i.</returns>
        private static int InitializeI(int h, bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                return h + 1;
            }

            return h - 1;
        }

        /// <summary>
        ///     Gets the initial value for j.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>The initial value for j.</returns>
        private static int InitializeJ(int i, bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                return i + 1;
            }

            return i - 1;
        }

        /// <summary>
        ///     Marks the targets for feasibility check.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="i">The i.</param>
        /// <param name="mark">The mark table.</param>
        /// <param name="countH">The count of h.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        private static void MarkTargetsForFeasibilityCheck(IList<Target> targets, int i, IList<int> mark, int countH,
            bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                foreach (var successor in targets[i].RequiredBy.Select(restriction => restriction.TargetToVisit))
                {
                    mark[targets.IndexOf(successor)] = countH;
                }
            }
            else
            {
                foreach (var predecessor in targets[i + 1].Restrictions.Select(restriction => restriction.NeededTarget))
                {
                    mark[targets.IndexOf(predecessor)] = countH;
                }
            }
        }

        /// <summary>
        ///     Performs the exchange.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <param name="solutionToImprove">The solution to improve.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>Solution after exchange in form of list of targets.</returns>
        private static List<Target> PerformExchange(int h, int i, int j, Solution solutionToImprove,
            bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                solutionToImprove.PerformSop3Exchange(h, i, j);
            }
            else
            {
                solutionToImprove.PerformSop3Exchange(j, i, h);
            }

            return solutionToImprove.GetTargets();
        }

        /// <summary>
        ///     Pushes values on the don't push stack.
        /// </summary>
        /// <param name="dontPushStack">The don't push stack.</param>
        /// <param name="bh">The best h.</param>
        /// <param name="bi">The best i.</param>
        /// <param name="bj">The best j.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        private static void PushOnStack(UniqueItemsStack<int> dontPushStack, int bh, int bi, int bj,
            bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                dontPushStack.Push(bj + 1);
                dontPushStack.Push(bj);
                dontPushStack.Push(bi + 1);
                dontPushStack.Push(bi);
                dontPushStack.Push(bh + 1);
                dontPushStack.Push(bh);
            }
            else
            {
                dontPushStack.Push(bh + 1);
                dontPushStack.Push(bh);
                dontPushStack.Push(bi + 1);
                dontPushStack.Push(bi);
                dontPushStack.Push(bj + 1);
                dontPushStack.Push(bj);
            }
        }

        /// <summary>
        ///     Computes the gain.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <param name="targets">The targets.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>Potential gain from exchange.</returns>
        private double ComputeGain(int h, int i, int j, IReadOnlyList<Target> targets, bool isForwardSearch)
        {
            return isForwardSearch ? ComputeGain(h, i, j, targets) : ComputeGain(j, i, h, targets);
        }

        /// <summary>
        ///     Computes the gain.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="targets">The targets.</param>
        /// <returns>Potential gain from exchange.</returns>
        private double ComputeGain(int p1, int p2, int p3, IReadOnlyList<Target> targets)
        {
            double gain = 0;
            gain += _problem.GetCost(targets[p1], targets[p1 + 1]);
            gain += _problem.GetCost(targets[p2], targets[p2 + 1]);
            gain += _problem.GetCost(targets[p3], targets[p3 + 1]);
            gain -= _problem.GetCost(targets[p1], targets[p2 + 1]);
            gain -= _problem.GetCost(targets[p2], targets[p3 + 1]);
            gain -= _problem.GetCost(targets[p3], targets[p1 + 1]);

            return gain;
        }

        /// <summary>
        ///     Gets value indicating whether i loop should continue.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="i">The i.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>Value indicating whether i loop should continue.</returns>
        private bool ContinueILoop(int h, int i, bool isForwardSearch)
        {
            var n = _problem.NumberOfEdges;
            if (isForwardSearch)
            {
                return i < n && h < n;
            }

            return i > 0 && h < n;
        }

        /// <summary>
        ///     Gets value indicating whether j loop should continue.
        /// </summary>
        /// <param name="j">The j.</param>
        /// <param name="isFeasible">if set to <c>true</c> [is feasible].</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns>Value indicating whether j loop should continue.</returns>
        private bool ContinueJLoop(int j, bool isFeasible, bool isForwardSearch)
        {
            if (!isForwardSearch) return j >= 0 && isFeasible;

            var n = _problem.NumberOfEdges;
            return j < n && isFeasible;
        }

        /// <summary>
        ///     Eats the pheromones.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="h">The h.</param>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        private void EatPheromones(IReadOnlyList<Target> targets, int h, int i, int j,
            bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                EatPheromones(targets, h, i, j);
            }
            else
            {
                EatPheromones(targets, j, i, h);
            }
        }

        /// <summary>
        ///     Eats the pheromones.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        private void EatPheromones(IReadOnlyList<Target> targets, int p1, int p2, int p3)
        {
            switch (_eatPheromone)
            {
                case EatPheromoneDuringLocalSearch.EatOnWholePath:
                    EatPheromonesOnWholePath(targets, p1, p2, p3);
                    break;

                case EatPheromoneDuringLocalSearch.EatOnlyOnChangedRoutes:
                    EatPheromonesOnChangedRoutes(targets, p1, p2, p3);
                    break;
            }
        }

        /// <summary>
        ///     Eats the pheromones between indexes.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        private void EatPheromonesBetweenIndexes(IReadOnlyList<Target> targets, int startIndex, int endIndex)
        {
            for (var i = startIndex + 1; i <= endIndex; i++)
            {
                var route = _problem.GetRoute(targets[i - 1], targets[i]);
                PheromoneManipulator.EatPheromone(route);
            }
        }

        /// <summary>
        ///     Eats the pheromones on changed routes.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        private void EatPheromonesOnChangedRoutes(IReadOnlyList<Target> targets, int p1, int p2, int p3)
        {
            // Route from p1 goes to (p2 + 1).
            var routeFromP1 = _problem.GetRoute(targets[p1], targets[p2 + 1]);
            // Route from p2 goes to (p3 + 1).
            var routeFromP2 = _problem.GetRoute(targets[p2], targets[p3 + 1]);
            // Route form p3 goes to (p1 + 1).
            var routeFromP3 = _problem.GetRoute(targets[p3], targets[p1 + 1]);

            PheromoneManipulator.EatPheromone(routeFromP1);
            PheromoneManipulator.EatPheromone(routeFromP2);
            PheromoneManipulator.EatPheromone(routeFromP3);
        }

        /// <summary>
        ///     Eats the pheromones on whole path.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        private void EatPheromonesOnWholePath(IReadOnlyList<Target> targets, int p1, int p2, int p3)
        {
            EatPheromonesOnChangedRoutes(targets, p1, p2, p3);
            EatPheromonesBetweenIndexes(targets, 0, p1);
            EatPheromonesBetweenIndexes(targets, p1 + 1, p2);
            EatPheromonesBetweenIndexes(targets, p2 + 1, p3);
            EatPheromonesBetweenIndexes(targets, p3, targets.Count - 1);
        }

        /// <summary>
        ///     Generates the don't push stack.
        /// </summary>
        /// <param name="bestKnownSolution">The best known solution.</param>
        /// <param name="targets">The targets.</param>
        /// <param name="solutionToImprove">The solution to improve.</param>
        /// <returns>Don't push stack.</returns>
        private UniqueItemsStack<int> GenerateDontPushStack(Solution bestKnownSolution, IList<Target> targets,
            Solution solutionToImprove)
        {
            if (_useChangedIndexesInStackInitialization)
            {
                var toStack = new List<int>();
                var targetsInBestSolution = bestKnownSolution.GetTargets();
                for (var i = 0; i < targets.Count; i++)
                {
                    if (targets[i] != targetsInBestSolution[i])
                    {
                        toStack.Add(i);
                    }
                }

                return new UniqueItemsStack<int>(toStack);
            }
            var routesNotUsedInBestSolution =
                solutionToImprove.Routes.Where(route => !bestKnownSolution.Routes.Contains(route));
            var idexes = new HashSet<int>();
            foreach (var route in routesNotUsedInBestSolution)
            {
                idexes.Add(targets.IndexOf(route.Origin));
                idexes.Add(targets.IndexOf(route.Target));
            }

            return new UniqueItemsStack<int>(idexes);
        }

        /// <summary>
        ///     Determines whether [is swap feasible] [the specified j].
        /// </summary>
        /// <param name="j">The j.</param>
        /// <param name="mark">The mark.</param>
        /// <param name="countH">The count h.</param>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <returns></returns>
        private static bool IsSwapFeasible(int j, IList<int> mark, int countH, bool isForwardSearch)
        {
            if (isForwardSearch)
            {
                return mark[j] != countH;
            }
            return mark[j + 1] != countH;
        }

        /// <summary>
        ///     Performs the search.
        /// </summary>
        /// <param name="isForwardSearch">if set to <c>true</c> the search direction is forward.</param>
        /// <param name="h">The h.</param>
        /// <param name="countH">The count h.</param>
        /// <param name="mark">The mark.</param>
        /// <param name="targets">The targets.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="dontPushStack">The don't push stack.</param>
        /// <returns></returns>
        private bool PerformSearch(bool isForwardSearch, int h, int countH, IList<int> mark, ref List<Target> targets,
            Solution solution, UniqueItemsStack<int> dontPushStack)
        {
            var isSolutionFound = false;
            var i = InitializeI(h, isForwardSearch);

            int bh = -1, bi = -1, bj = -1;
            while (ContinueILoop(h, i, isForwardSearch))
            {
                var j = InitializeJ(i, isForwardSearch);

                double bestGain = 0;

                MarkTargetsForFeasibilityCheck(targets, i, mark, countH, isForwardSearch);

                var isFeasible = true;
                while (ContinueJLoop(j, isFeasible, isForwardSearch))
                {
                    isFeasible = IsSwapFeasible(j, mark, countH, isForwardSearch);
                    if (isFeasible)
                    {
                        EatPheromones(targets, h, i, j, isForwardSearch);
                        var gain = ComputeGain(h, i, j, targets, isForwardSearch);
                        if (gain > bestGain)
                        {
                            isSolutionFound = true;
                            bh = h;
                            bi = i;
                            bj = j;
                            bestGain = gain;
                        }
                    }

                    j = GetNextJ(j, isForwardSearch);
                }

                if (isSolutionFound)
                {
                    targets = PerformExchange(bh, bi, bj, solution, isForwardSearch);
                    PushOnStack(dontPushStack, bh, bi, bj, isForwardSearch);
                    return true;
                }

                i = GetNextI(i, isForwardSearch);
            }

            return false;
        }
    }
}