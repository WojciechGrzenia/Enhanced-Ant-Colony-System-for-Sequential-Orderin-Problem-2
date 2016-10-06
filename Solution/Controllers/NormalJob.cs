using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Controllers.DataLayer.Helpers;
using Controllers.SequentailOrdering;
using DTO;
using System.Globalization;

namespace Controllers
{
    /// <summary>
    ///     Represents normal computational job.
    /// </summary>
    [Serializable]
    public class NormalJob : SequentialOrderingJobParameters, IJob
    {
        /// <summary>
        ///     The problem data.
        /// </summary>
        private SequentialOrderingProblem _problemData;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NormalJob" /> class.
        /// </summary>
        public NormalJob(SequentialOrderingProblem problemData)
        {
            DoneTrials = 0;
            Solutions = new List<Solution>();
            _problemData = problemData;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NormalJob" /> class.
        /// </summary>
        /// <param name="jobParameters">The job parameters.</param>
        /// <param name="filePath">The file path.</param>
        public NormalJob(SequentialOrderingJobParameters jobParameters, string filePath)
        {
            FilePath = filePath;
            DoneTrials = 0;
            Solutions = new List<Solution>();
            foreach (var prop in typeof (SequentialOrderingJobParameters).GetProperties().Where(prop => prop.CanWrite))
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(jobParameters, null), null);
            }
        }

        /// <summary>
        ///     Gets the average number of iterations.
        /// </summary>
        /// <value>
        ///     The average number of iterations.
        /// </value>
        public double AverageIterations
        {
            get
            {
                Debug.Assert(Solutions != null, "Solutions == null");

                return ((double)TotalNumberOfIterations) / NumberOfTrials;
            }
        }

        /// <summary>
        ///     Gets the average solutions cost.
        /// </summary>
        /// <value>
        ///     The average solutions cost.
        /// </value>
        public double AverageSolutionsCost
        {
            get
            {
                return Solutions.Count == 0 ? double.PositiveInfinity : Solutions.Average(solution => solution.Cost);
            }
        }

        /// <summary>
        ///     Gets the done trials.
        /// </summary>
        /// <value>
        ///     The done trials.
        /// </value>
        public int DoneTrials { get; private set; }

        /// <summary>
        ///     Gets the solutions.
        /// </summary>
        /// <value>
        ///     The solutions.
        /// </value>
        public List<Solution> Solutions { get; private set; }

        /// <summary>
        ///     Gets the total number of iterations.
        /// </summary>
        /// <value>
        ///     The total number of iterations.
        /// </value>
        public int TotalNumberOfIterations { get; private set; }

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
        public bool IsFinished
        {
            get { return DoneTrials == NumberOfTrials; }
        }

        /// <summary>
        ///     Gets the time of one step.
        /// </summary>
        /// <value>
        ///     The time of one step.
        /// </value>
        public TimeSpan OneStepTime
        {
            get { return CalculationTime; }
        }

        /// <summary>
        ///     Gets the remaining time.
        /// </summary>
        /// <value>
        ///     The remaining time.
        /// </value>
        public TimeSpan RemainingTime
        {
            get { return TimeSpan.FromTicks(CalculationTime.Ticks*(NumberOfTrials - DoneTrials)); }
        }

        /// <summary>
        ///     Gets the total time needed.
        /// </summary>
        /// <value>
        ///     The total time needed.
        /// </value>
        public TimeSpan TotalTimeNeeded
        {
            get { return TimeSpan.FromTicks(CalculationTime.Ticks*NumberOfTrials); }
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
            var sopController = new SequentialOrderingController
            {
                Parameters = this
            };

            var result = sopController.Resolve(_problemData);
            if (result != null)
            {
                Solutions.Add(result);
                TotalNumberOfIterations += sopController.NumberOfIterations;
            }

            DoneTrials++;
        }

        /// <summary>
        ///     Gets the name of the result file.
        /// </summary>
        /// <returns>
        ///     The name of the result file.
        /// </returns>
        public string GetResultFileName()
        {
            return String.Format("{0} {1}", ConfigurationName, Path.GetFileName(FilePath));
        }

        /// <summary>
        ///     Gets the results as string.
        /// </summary>
        /// <returns>
        ///     The results as string.
        /// </returns>
        public string GetResultsAsString()
        {
            double averageSolutionCost = double.NaN;
            double bestSolutionCost = double.NaN;
            if (Solutions.Count != 0)
            {
                averageSolutionCost = Solutions.Average(solution => solution.Cost);
                bestSolutionCost = Solutions.Min(solution => solution.Cost);
            }
            var content =
                new StringBuilder(
                    string.Format(CultureInfo.InvariantCulture.NumberFormat,
                        "Configuration name: {0}{4}File name: {1}{4}Number of trials: {6}{4}Average solution cost: {2}{4}Average iterations: {5}{4}Best solution cost: {3}{4}Best solution: ",
                        ConfigurationName, Path.GetFileName(FilePath), averageSolutionCost, bestSolutionCost,
                        Environment.NewLine, AverageIterations, NumberOfTrials));

            if (Solutions.Count != 0)
            {
                var bestSolution = Solutions.Aggregate((s1, s2) => s1.Cost > s2.Cost ? s1 : s2);
                foreach (var route in bestSolution.Routes)
                {
                    content.Append(route.Origin.Index);
                    content.Append(' ');
                }
                content.Append(bestSolution.Routes.Last().Target.Index);
            }
            content.Append(Environment.NewLine);
            content.Append(Environment.NewLine);
            content.Append(GetParametersAsString());
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
        ///     Gets the parameters as string.
        /// </summary>
        /// <returns>The parameters.</returns>
        public string GetParametersAsString()
        {
            var sb = new StringBuilder();

            foreach (var prop in Type.GetType("DTO.SequentialOrderingParameters, DTO").GetProperties())
            {
                var value = prop.GetValue(this);
                var valueAsCandidateListParameters = value as CandidateListParameters;
                if (valueAsCandidateListParameters == null)
                {
                    AppendParameter(sb, prop, value);
                }
                else
                {
                    foreach (
                        var propertyInCandidateListParameters in
                            Type.GetType("DTO.CandidateListParameters, DTO").GetProperties())
                    {
                        AppendParameter(sb, propertyInCandidateListParameters,
                            propertyInCandidateListParameters.GetValue(valueAsCandidateListParameters));
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Appends the parameter name and value.
        /// </summary>
        /// <param name="stringBuilder">The stringBuilder.</param>
        /// <param name="prop">The property.</param>
        /// <param name="value">The value.</param>
        private static void AppendParameter(StringBuilder stringBuilder, PropertyInfo prop, object value)
        {
            stringBuilder.Append(prop.Name);
            stringBuilder.Append(": ");
            stringBuilder.Append(value);
            stringBuilder.Append(Environment.NewLine);
        }
    }
}