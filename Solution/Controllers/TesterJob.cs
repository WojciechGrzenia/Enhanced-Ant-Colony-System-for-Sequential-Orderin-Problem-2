using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Controllers.DataLayer.Helpers;
using DTO;
using UniversalTester;
using UniversalTester.Parameters;

namespace Controllers
{
    /// <summary>
    ///     Testing parameters job.
    /// </summary>
    [Serializable]
    internal class TesterJob : SequentialOrderingJobParameters, IJob
    {
        /// <summary>
        ///     Indicates if tester actions are initialized.
        /// </summary>
        [NonSerialized] private bool _areTesterActionsInitialized;

        /// <summary>
        ///     The problem data.
        /// </summary>
        private SequentialOrderingProblem _problemData;

        /// <summary>
        ///     The stopwatch.
        /// </summary>
        [NonSerialized] private Stopwatch _stopwatch;

        /// <summary>
        ///     The universal tester.
        /// </summary>
        private Tester _tester;

        /// <summary>
        ///     The time elapsed
        /// </summary>
        private TimeSpan _timeElapsed;

        /// <summary>
        ///     The time offset. Used in case when job is rerun from temporary file.
        /// </summary>
        [NonSerialized] private TimeSpan _timeOffset;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TesterJob" /> class.
        /// </summary>
        /// <param name="jobParameters">The job parameters.</param>
        /// <param name="filePath">The file path.</param>
        public TesterJob(SequentialOrderingJobParameters jobParameters, string filePath)
        {
            FilePath = filePath;
            DoneJobs = new List<NormalJob>();
            foreach (var prop in jobParameters.GetType().GetProperties().Where(prop => prop.CanWrite))
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(jobParameters, null), null);
            }
        }

        /// <summary>
        ///     Gets the best job.
        /// </summary>
        /// <value>
        ///     The best job.
        /// </value>
        public NormalJob BestJob { get; private set; }

        /// <summary>
        ///     Gets the done jobs.
        /// </summary>
        /// <value>
        ///     The done jobs.
        /// </value>
        public List<NormalJob> DoneJobs { get; private set; }

        /// <summary>
        ///     Gets the left time.
        /// </summary>
        /// <value>
        ///     The left time.
        /// </value>
        public TimeSpan LeftTime
        {
            get
            {
                if (_stopwatch == null)
                {
                    return CalculationTimeOfTester - _timeElapsed;
                }

                if (_stopwatch.Elapsed + _timeOffset > CalculationTimeOfTester)
                {
                    return new TimeSpan(0);
                }

                return CalculationTimeOfTester - _stopwatch.Elapsed - _timeOffset;
            }
        }

        /// <summary>
        ///     Gets or sets the file path.
        /// </summary>
        /// <value>
        ///     The file path.
        /// </value>
        public string FilePath { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is finished.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is finished; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinished { get; private set; }

        /// <summary>
        ///     Gets the time of one step.
        /// </summary>
        /// <value>
        ///     The time of one step.
        /// </value>
        public TimeSpan OneStepTime
        {
            get { return TimeSpan.FromTicks(CalculationTime.Ticks*NumberOfTrials); }
        }

        /// <summary>
        ///     Gets the remaining time.
        /// </summary>
        /// <value>
        ///     The remaining time.
        /// </value>
        public TimeSpan RemainingTime
        {
            get { return LeftTime; }
        }

        /// <summary>
        ///     Gets the total time needed.
        /// </summary>
        /// <value>
        ///     The total time needed.
        /// </value>
        public TimeSpan TotalTimeNeeded
        {
            get { return CalculationTimeOfTester; }
        }

        /// <summary>
        ///     Does the one step of the job.
        /// </summary>
        public void DoOneStep()
        {
            if (_problemData == null)
            {
                _problemData = DataFileHelper.GetProblemDataFromFile(FilePath);
            }

            if (_stopwatch == null)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                InitializeTester();
            }

            // tester got deserialized from file.
            if (!_areTesterActionsInitialized && _tester != null)
            {
                InitializeTester();
            }

            if (_stopwatch.Elapsed + _timeOffset > CalculationTimeOfTester)
            {
                IsFinished = true;
                return;
            }

            Debug.Assert(_tester != null, "_tester == null");
            _tester.DoOneStep();

            _timeElapsed = _stopwatch.Elapsed + _timeOffset;
        }

        /// <summary>
        ///     Gets the name of the result file.
        /// </summary>
        /// <returns>
        ///     The name of the result file.
        /// </returns>
        public string GetResultFileName()
        {
            return String.Format("{0} {1} testing", ConfigurationName, Path.GetFileName(FilePath));
        }

        /// <summary>
        ///     Gets the results as string.
        /// </summary>
        /// <returns>
        ///     The results as string.
        /// </returns>
        public string GetResultsAsString()
        {
            var content =
                new StringBuilder(
                    string.Format(
                        "Configuration name: {0}{1}File name: {2}{1}Number of tests performed: {3}{1}Best parameters results: {1}{1}",
                        ConfigurationName, Environment.NewLine, Path.GetFileName(FilePath),
                        DoneJobs.Count));

            content.Append(BestJob.GetResultsAsString());
            return content.ToString();
        }

        /// <summary>
        ///     Gets the name of the temporary file.
        /// </summary>
        /// <returns>
        ///     The name of the temporary file.
        /// </returns>
        public string GetTmpFileName()
        {
            return String.Format("{0} {1}", ConfigurationName, Path.GetFileName(FilePath));
        }

        /// <summary>
        ///     Initializes the tester.
        /// </summary>
        private void InitializeTester()
        {
            var random = new Random();
            if (_tester == null)
            {
                _tester = new Tester(random)
                {
                    Data = new TesterDataSpace()
                };
            }
            else
            {
                // This job is rerun from file.
                _timeOffset = _timeElapsed;
            }

            var evaporatePheromones = new BoolParameter(random)
            {
                IsActive = true,
                Name = "EvaporatePheromones",
                Value = EvaporatePheromones
            };
            if (TestEvaporatePheromones)
            {
                evaporatePheromones = TryToAddParameterToTester(evaporatePheromones) as BoolParameter;
            }

            var coefficientOfEvaporation = new DoubleParameter(random)
            {
                IsActive = true,
                Name = "CoefficientOfEvaporation",
                Value = CoefficientOfEvaporation,
                MaxValue = 0.3,
                MinValue = 0.005
            };
            Debug.Assert(evaporatePheromones != null, "evaporatePheromones == null");
            evaporatePheromones.DependentParameters.Add(coefficientOfEvaporation);
            if (TestCoefficientOfEvaporation)
            {
                coefficientOfEvaporation = TryToAddParameterToTester(coefficientOfEvaporation) as DoubleParameter;
            }

            var useEarlySolutionForInitialPheromoneAmount = new BoolParameter(random)
            {
                IsActive = true,
                Name = "UseEarlySolutionForInitialPheromoneAmount",
                Value = UseEarlySolutionForInitialPheromoneAmount
            };
            if (TestUseEarlySolutionForInitialPheromoneAmount)
            {
                useEarlySolutionForInitialPheromoneAmount =
                    TryToAddParameterToTester(useEarlySolutionForInitialPheromoneAmount) as BoolParameter;
            }

            var initialPheromoneAmount = new DoubleParameter(random)
            {
                IsActive = !UseEarlySolutionForInitialPheromoneAmount,
                Name = "InitialPheromoneAmount",
                Value = InitialPheromoneAmount,
                MaxValue = 10,
                MinValue = 0.00000000001
            };
            Debug.Assert(useEarlySolutionForInitialPheromoneAmount != null,
                "useEarlySolutionForInitialPheromoneAmount == null");
            useEarlySolutionForInitialPheromoneAmount.ToActivateWhenFalse.Add(initialPheromoneAmount);
            if (TestInitialPheromoneAmount)
            {
                initialPheromoneAmount = TryToAddParameterToTester(initialPheromoneAmount) as DoubleParameter;
            }

            var numberOfAnts = new IntParameter(random)
            {
                IsActive = true,
                Name = "NumberOfAnts",
                Value = NumberOfAnts,
                MaxValue = 15,
                MinValue = 1
            };
            if (TestNumberOfAnts)
            {
                numberOfAnts = TryToAddParameterToTester(numberOfAnts) as IntParameter;
            }

            var expectedNumberOfProbabilisticRuleUses = new DoubleParameter(random)
            {
                IsActive = true,
                Name = "ExpectedNumberOfProbabilisticRuleUses",
                Value = ExpectedNumberOfProbabilisticRuleUses,
                MaxValue = 20,
                MinValue = 0.01
            };
            if (TestExpectedNumberOfProbabilisticRuleUses)
            {
                expectedNumberOfProbabilisticRuleUses =
                    TryToAddParameterToTester(expectedNumberOfProbabilisticRuleUses) as DoubleParameter;
            }

            var useCandidateList = new BoolParameter(random)
            {
                IsActive = true,
                Name = "UseCandidateList",
                Value = CandidateListParameters.UseCandidateList
            };
            if (TestUseCandidateList)
            {
                useCandidateList = TryToAddParameterToTester(useCandidateList) as BoolParameter;
            }

            var useFixedSize = new BoolParameter(random)
            {
                IsActive = CandidateListParameters.UseCandidateList,
                Name = "UseFixedSize",
                Value = CandidateListParameters.UseFixedSize
            };
            if (TestUseFixedSize)
            {
                useFixedSize = TryToAddParameterToTester(useFixedSize) as BoolParameter;
            }
            Debug.Assert(useCandidateList != null, "useCandidateList == null");
            useCandidateList.DependentParameters.Add(useFixedSize);

            var fractionOfMaxSize = new DoubleParameter(random)
            {
                IsActive = !CandidateListParameters.UseFixedSize,
                Name = "FractionOfMaxSize",
                Value = CandidateListParameters.FractionOfMaxSize,
                MaxValue = 0.4,
                MinValue = 0.05
            };
            if (TestFractionOfMaxSize)
            {
                fractionOfMaxSize = TryToAddParameterToTester(fractionOfMaxSize) as DoubleParameter;
            }
            Debug.Assert(useFixedSize != null, "useFixedSize == null");
            useFixedSize.ToActivateWhenFalse.Add(fractionOfMaxSize);

            var ifNoneIsFeasibleTakeNextBest = new BoolParameter(random)
            {
                IsActive = CandidateListParameters.UseCandidateList,
                Name = "IfNoneIsFeasibleTakeNextBest",
                Value = CandidateListParameters.IfNoneIsFeasibleTakeNextBest
            };
            if (TestIfNoneIsFeasibleTakeNextBest)
            {
                ifNoneIsFeasibleTakeNextBest = TryToAddParameterToTester(ifNoneIsFeasibleTakeNextBest) as BoolParameter;
            }
            useCandidateList.DependentParameters.Add(ifNoneIsFeasibleTakeNextBest);

            var sizeOfCandidateList = new IntParameter(random)
            {
                IsActive = CandidateListParameters.UseFixedSize,
                Name = "SizeOfCandidateList",
                Value = CandidateListParameters.Size,
                MaxValue = 40,
                MinValue = 2
            };
            if (TestFixedSize)
            {
                sizeOfCandidateList = TryToAddParameterToTester(sizeOfCandidateList) as IntParameter;
            }
            useFixedSize.DependentParameters.Add(sizeOfCandidateList);

            var pheromoneEatParameter = new DoubleParameter(random)
            {
                IsActive = true,
                Name = "PheromoneEatParameter",
                Value = PheromoneEatParameter,
                MaxValue = 0.9,
                MinValue = 0.0001
            };
            if (TestPheromoneEatParameter)
            {
                pheromoneEatParameter = TryToAddParameterToTester(pheromoneEatParameter) as DoubleParameter;
            }

            var breakExpensiveSolutions = new BoolParameter(random)
            {
                IsActive = true,
                Name = "BreakExpensiveSolutions",
                Value = BreakExpensiveSolutions
            };
            if (TestBreakExpensiveSolution)
            {
                breakExpensiveSolutions = TryToAddParameterToTester(breakExpensiveSolutions) as BoolParameter;
            }

            var useMetRestrictionsCounter = new BoolParameter(random)
            {
                IsActive = true,
                Name = "UseMetRestrictionsCounter",
                Value = UseMetRestrictionsCounter
            };
            if (TestUseMetRestrictionsCounter)
            {
                useMetRestrictionsCounter = TryToAddParameterToTester(useMetRestrictionsCounter) as BoolParameter;
            }

            var eatPheromoneDuringLocalSearch = new EnumParameter(random)
            {
                IsActive = true,
                Name = "EatPheromoneDuringLocalSearch",
                Value = EatPheromoneDuringLocalSearch
            };
            if (TestEatPheromoneDuringLocalSearch)
            {
                eatPheromoneDuringLocalSearch =
                    TryToAddParameterToTester(eatPheromoneDuringLocalSearch) as EnumParameter;
            }

            var useRouteCostInAddingPheromone = new BoolParameter(random)
            {
                IsActive = true,
                Name = "UseRouteCostInAddingPheromone",
                Value = UseRouteCostInAddingPheromone
            };
            if (TestUseRouteCostInAddingPheromone)
            {
                useRouteCostInAddingPheromone =
                    TryToAddParameterToTester(useRouteCostInAddingPheromone) as BoolParameter;
            }

            var useRouteCostInEvaporation = new BoolParameter(random)
            {
                IsActive = EvaporatePheromones,
                Name = "UseRouteCostInEvaporation",
                Value = UseRouteCostInEvaporation
            };
            evaporatePheromones.DependentParameters.Add(useRouteCostInEvaporation);
            if (TestUseRouteCostInEvaporation)
            {
                useRouteCostInEvaporation = TryToAddParameterToTester(useRouteCostInEvaporation) as BoolParameter;
            }

            var allowToPickBestRouteOnlyByDeterministicRule = new BoolParameter(random)
            {
                IsActive = true,
                Name = "AllowToPickBestRouteOnlyByDeterministicRule",
                Value = AllowToPickBestRouteOnlyByDeterministicRule
            };
            if (TestAllowToPickBestRouteOnlyByDeterministicRule)
            {
                allowToPickBestRouteOnlyByDeterministicRule =
                    TryToAddParameterToTester(allowToPickBestRouteOnlyByDeterministicRule) as BoolParameter;
            }

            var dontUseHeuristic = new BoolParameter(random)
            {
                IsActive = true,
                Name = "DontUseHeuristic",
                Value = DontUseHeuristic
            };
            if (TestDontUseHeuristic)
            {
                dontUseHeuristic = TryToAddParameterToTester(dontUseHeuristic) as BoolParameter;
            }

            var decreaseHeuristicMeaningInTime = new BoolParameter(random)
            {
                IsActive = !DontUseHeuristic,
                Name = "DecreaseHeuristicMeaningInTime",
                Value = DecreaseHeuristicMeaningInTime
            };
            if (TestDecreaseHeuristicMeaningInTime)
            {
                decreaseHeuristicMeaningInTime =
                    TryToAddParameterToTester(decreaseHeuristicMeaningInTime) as BoolParameter;
            }
            dontUseHeuristic.ToActivateWhenFalse.Add(decreaseHeuristicMeaningInTime);

            var initialHeuristicMeaning = new DoubleParameter(random)
            {
                IsActive = !DontUseHeuristic && DecreaseHeuristicMeaningInTime,
                Name = "InitialHeuristicMeaning",
                Value = InitialHeuristicMeaning,
                MaxValue = 2,
                MinValue = 0.01
            };
            if (TestInitialHeuristicMeaning)
            {
                initialHeuristicMeaning = TryToAddParameterToTester(initialHeuristicMeaning) as DoubleParameter;
            }
            decreaseHeuristicMeaningInTime.DependentParameters.Add(initialHeuristicMeaning);

            var heuristicMeaningDescreaseFactor = new DoubleParameter(random)
            {
                IsActive = !DontUseHeuristic && DecreaseHeuristicMeaningInTime,
                Name = "HeuristicMeaningDecreaseFactor",
                Value = HeuristicMeaningDecreaseFactor,
                MaxValue = 0.2,
                MinValue = 0.000000001
            };
            if (TestHeuristicMeaningDecreaseFactor)
            {
                heuristicMeaningDescreaseFactor =
                    TryToAddParameterToTester(heuristicMeaningDescreaseFactor) as DoubleParameter;
            }
            decreaseHeuristicMeaningInTime.DependentParameters.Add(heuristicMeaningDescreaseFactor);

            var useRouteCostInSettingInitialPheromoneAmount = new BoolParameter(random)
            {
                IsActive = true,
                Name = "UseRouteCostInSettingInitialPheromoneAmount",
                Value = UseRouteCostInSettingInitialPheromoneAmount
            };
            if (TestUseRouteCostInSettingInitialPheromoneAmount)
            {
                useRouteCostInSettingInitialPheromoneAmount =
                    TryToAddParameterToTester(useRouteCostInSettingInitialPheromoneAmount) as BoolParameter;
            }

            var useDynamicNumberOfProbabilisticRuleUses = new BoolParameter(random)
            {
                IsActive = true,
                Name = "UseDynamicNumberOfProbabilisticRuleUses",
                Value = UseDynamicNumberOfProbabilisticRuleUses
            };
            if (TestUseDynamicNumberOfProbabilisticRuleUses)
            {
                useDynamicNumberOfProbabilisticRuleUses =
                    TryToAddParameterToTester(useDynamicNumberOfProbabilisticRuleUses) as BoolParameter;
            }

            var changeNumberOfProbabilisticRuleUsesInTime = new BoolParameter(random)
            {
                IsActive = UseDynamicNumberOfProbabilisticRuleUses,
                Name = "ChangeNumberOfProbabilisticRuleUsesInTime",
                Value = ChangeNumberOfProbabilisticRuleUsesInTime
            };
            if (TestChangeNumberOfProbabilisticRuleUsesInTime)
            {
                changeNumberOfProbabilisticRuleUsesInTime =
                    TryToAddParameterToTester(changeNumberOfProbabilisticRuleUsesInTime) as BoolParameter;
            }
            useDynamicNumberOfProbabilisticRuleUses.DependentParameters.Add(changeNumberOfProbabilisticRuleUsesInTime);

            var timeProbabilisticChangeFactor = new DoubleParameter(random)
            {
                IsActive = UseDynamicNumberOfProbabilisticRuleUses && ChangeNumberOfProbabilisticRuleUsesInTime,
                Name = "TimeProbabilisticChangeFactor",
                Value = TimeProbabilisticChangeFactor,
                MaxValue = 1.2,
                MinValue = 1.000000000001
            };
            if (TestTimeProbabilisticChangeFactor)
            {
                timeProbabilisticChangeFactor =
                    TryToAddParameterToTester(timeProbabilisticChangeFactor) as DoubleParameter;
            }
            changeNumberOfProbabilisticRuleUsesInTime.DependentParameters.Add(timeProbabilisticChangeFactor);

            var probabilisticChangeBorder = new DoubleParameter(random)
            {
                IsActive = UseDynamicNumberOfProbabilisticRuleUses,
                Name = "ProbabilisticChangeBorder",
                Value = ProbabilisticChangeBorder,
                MaxValue = 0.4,
                MinValue = 0.00001
            };
            if (TestProbabilisticChangeBorder)
            {
                probabilisticChangeBorder = TryToAddParameterToTester(probabilisticChangeBorder) as DoubleParameter;
            }
            useDynamicNumberOfProbabilisticRuleUses.DependentParameters.Add(probabilisticChangeBorder);

            var increaseNumberOfProbabilisticRuleUsesWhenNoImprovement = new BoolParameter(random)
            {
                IsActive = UseDynamicNumberOfProbabilisticRuleUses,
                Name = "IncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement",
                Value = IncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement
            };
            if (TestIncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement)
            {
                increaseNumberOfProbabilisticRuleUsesWhenNoImprovement =
                    TryToAddParameterToTester(increaseNumberOfProbabilisticRuleUsesWhenNoImprovement) as BoolParameter;
            }
            useDynamicNumberOfProbabilisticRuleUses.DependentParameters.Add(
                increaseNumberOfProbabilisticRuleUsesWhenNoImprovement);

            var noImprovementProbabilisticIncreaseFactor = new DoubleParameter(random)
            {
                IsActive =
                    UseDynamicNumberOfProbabilisticRuleUses && IncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement,
                Name = "NoImprovementProbabilisticIncreaseFactor",
                Value = NoImprovementProbabilisticIncreaseFactor,
                MaxValue = 0.1,
                MinValue = 0.000000001
            };
            if (TestNoImprovementProbabilisticIncreaseFactor)
            {
                noImprovementProbabilisticIncreaseFactor =
                    TryToAddParameterToTester(noImprovementProbabilisticIncreaseFactor) as DoubleParameter;
            }
            increaseNumberOfProbabilisticRuleUsesWhenNoImprovement.DependentParameters.Add(
                noImprovementProbabilisticIncreaseFactor);

            var resetExpectedNumberOfProbabilisticRuleOnBestSolutionFound = new BoolParameter(random)
            {
                IsActive = UseDynamicNumberOfProbabilisticRuleUses,
                Name = "ResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound",
                Value = ResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound
            };
            if (TestResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound)
            {
                resetExpectedNumberOfProbabilisticRuleOnBestSolutionFound =
                    TryToAddParameterToTester(resetExpectedNumberOfProbabilisticRuleOnBestSolutionFound) as
                        BoolParameter;
            }
            useDynamicNumberOfProbabilisticRuleUses.DependentParameters.Add(
                resetExpectedNumberOfProbabilisticRuleOnBestSolutionFound);

            var acceptSolutionWithTheSameCost = new BoolParameter(random)
            {
                IsActive = true,
                Name = "AcceptSolutionWithTheSameCost",
                Value = AcceptSolutionWithTheSameCost
            };
            if (TestAcceptSolutionWithTheSameCost)
            {
                acceptSolutionWithTheSameCost =
                    TryToAddParameterToTester(acceptSolutionWithTheSameCost) as BoolParameter;
            }

            var forceAtLeastOneRouteChange = new BoolParameter(random)
            {
                IsActive = true,
                Name = "ForceAtLeastOneRouteChange",
                Value = ForceAtLeastOneRouteChange
            };
            if (TestForceAtLeastOneRouteChange)
            {
                forceAtLeastOneRouteChange = TryToAddParameterToTester(forceAtLeastOneRouteChange) as BoolParameter;
            }

            var useChangedIndexesInStackInitialization = new BoolParameter(random)
            {
                IsActive = true,
                Name = "UseChangedIndexesInStackInitialization",
                Value = TestUseChangedIndexesInStackInitialization
            };
            if (TestUseChangedIndexesInStackInitialization)
            {
                useChangedIndexesInStackInitialization =
                    TryToAddParameterToTester(useChangedIndexesInStackInitialization) as BoolParameter;
            }

            _tester.Action = () =>
            {
                var job = new NormalJob(_problemData)
                {
                    EvaporatePheromones = evaporatePheromones.Value,
                    CoefficientOfEvaporation = coefficientOfEvaporation.Value,
                    InitialPheromoneAmount = initialPheromoneAmount.Value,
                    NumberOfAnts = numberOfAnts.Value,
                    CalculationTime = CalculationTime,
                    ExpectedNumberOfProbabilisticRuleUses = expectedNumberOfProbabilisticRuleUses.Value,
                    CandidateListParameters = new CandidateListParameters
                    {
                        UseCandidateList = useCandidateList.Value,
                        UseFixedSize = useFixedSize.Value,
                        FractionOfMaxSize = fractionOfMaxSize.Value,
                        IfNoneIsFeasibleTakeNextBest = ifNoneIsFeasibleTakeNextBest.Value,
                        Size = sizeOfCandidateList.Value
                    },
                    PheromoneEatParameter = pheromoneEatParameter.Value,
                    UseEarlySolutionForInitialPheromoneAmount = useEarlySolutionForInitialPheromoneAmount.Value,
                    BreakExpensiveSolutions = breakExpensiveSolutions.Value,
                    UseMetRestrictionsCounter = useMetRestrictionsCounter.Value,
                    EatPheromoneDuringLocalSearch = (EatPheromoneDuringLocalSearch) eatPheromoneDuringLocalSearch.Value,
                    UseRouteCostInAddingPheromone = useRouteCostInAddingPheromone.Value,
                    UseRouteCostInEvaporation = useRouteCostInEvaporation.Value,
                    AllowToPickBestRouteOnlyByDeterministicRule = allowToPickBestRouteOnlyByDeterministicRule.Value,
                    DecreaseHeuristicMeaningInTime = decreaseHeuristicMeaningInTime.Value,
                    InitialHeuristicMeaning = initialHeuristicMeaning.Value,
                    HeuristicMeaningDecreaseFactor = heuristicMeaningDescreaseFactor.Value,
                    DontUseHeuristic = dontUseHeuristic.Value,
                    UseRouteCostInSettingInitialPheromoneAmount = useRouteCostInSettingInitialPheromoneAmount.Value,
                    UseDynamicNumberOfProbabilisticRuleUses = useDynamicNumberOfProbabilisticRuleUses.Value,
                    ChangeNumberOfProbabilisticRuleUsesInTime = changeNumberOfProbabilisticRuleUsesInTime.Value,
                    TimeProbabilisticChangeFactor = timeProbabilisticChangeFactor.Value,
                    ProbabilisticChangeBorder = probabilisticChangeBorder.Value,
                    IncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement =
                        increaseNumberOfProbabilisticRuleUsesWhenNoImprovement.Value,
                    NoImprovementProbabilisticIncreaseFactor = noImprovementProbabilisticIncreaseFactor.Value,
                    ResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound =
                        resetExpectedNumberOfProbabilisticRuleOnBestSolutionFound.Value,
                    AcceptSolutionWithTheSameCost = acceptSolutionWithTheSameCost.Value,
                    ForceAtLeastOneRouteChange = forceAtLeastOneRouteChange.Value,
                    ConfigurationName = ConfigurationName,
                    FilePath = FilePath,
                    NumberOfTrials = NumberOfTrials,
                    UseChangedIndexesInStackInitialization = useChangedIndexesInStackInitialization.Value
                };

                while (!job.IsFinished)
                {
                    job.DoOneStep();
                }

                DoneJobs.Add(job);
                if (BestJob == null || BestJob.AverageSolutionsCost > job.AverageSolutionsCost)
                {
                    BestJob = job;
                }

                ((TesterDataSpace) _tester.Data).Job = job;
            };

            _tester.Assessment = () =>
            {
                var job = ((TesterDataSpace) _tester.Data).Job;
                return job != null ? job.AverageSolutionsCost : double.PositiveInfinity;
            };

            _areTesterActionsInitialized = true;
        }

        /// <summary>
        ///     Tries to add parameter to tester.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///     Parameter passed as argument when adding is successful; otherwise: parameter with the same name taken from
        ///     tester.
        /// </returns>
        private Parameter TryToAddParameterToTester(Parameter parameter)
        {
            if (!_tester.Parameters.ContainsKey(parameter.Name))
            {
                 _tester.Parameters.Add(parameter.Name, parameter);
                 return parameter;
            }

            return _tester.Parameters[parameter.Name];
        }

        /// <summary>
        ///     Data object used in universal tester.
        /// </summary>
        [Serializable]
        private class TesterDataSpace
        {
            /// <summary>
            ///     Gets or sets the job.
            /// </summary>
            /// <value>
            ///     The job.
            /// </value>
            public NormalJob Job { get; set; }
        }
    }
}