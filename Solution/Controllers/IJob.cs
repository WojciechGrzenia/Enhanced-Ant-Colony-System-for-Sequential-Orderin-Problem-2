using System;

namespace Controllers
{
    /// <summary>
    ///     Interface for a computational job.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is finished.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is finished; otherwise, <c>false</c>.
        /// </value>
        bool IsFinished { get; }

        /// <summary>
        ///     Gets or sets the file path.
        /// </summary>
        /// <value>
        ///     The file path.
        /// </value>
        string FilePath { get; set; }

        /// <summary>
        ///     Gets the remaining time.
        /// </summary>
        /// <value>
        ///     The remaining time.
        /// </value>
        TimeSpan RemainingTime { get; }

        /// <summary>
        ///     Gets the total time needed.
        /// </summary>
        /// <value>
        ///     The total time needed.
        /// </value>
        TimeSpan TotalTimeNeeded { get; }

        /// <summary>
        ///     Gets the time of one step.
        /// </summary>
        /// <value>
        ///     The time of one step.
        /// </value>
        TimeSpan OneStepTime { get; }

        /// <summary>
        ///     Does the one step of the job.
        /// </summary>
        void DoOneStep();

        /// <summary>
        ///     Gets the name of the result file.
        /// </summary>
        /// <returns>The name of the result file.</returns>
        string GetResultFileName();

        /// <summary>
        ///     Gets the name of the temporary file.
        /// </summary>
        /// <returns>The name of the temporary file.</returns>
        string GetTmpFileName();

        /// <summary>
        ///     Gets the results as string.
        /// </summary>
        /// <returns>The results as string.</returns>
        string GetResultsAsString();
    }
}