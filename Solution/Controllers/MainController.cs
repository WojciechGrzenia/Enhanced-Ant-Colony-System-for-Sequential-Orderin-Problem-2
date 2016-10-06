using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Controllers.DataLayer;
using DTO;

namespace Controllers
{
    /// <summary>
    ///     Main controller of the application.
    /// </summary>
    public class MainController
    {
        /// <summary>
        ///     The configuration file name.
        /// </summary>
        private const string ConfigFileName = "config";

        /// <summary>
        ///     The file extension
        /// </summary>
        private const string FileExtension = ".bin";

        /// <summary>
        ///     The results path.
        /// </summary>
        public const string ResultsPath = "results\\";

        /// <summary>
        ///     The temporary files path.
        /// </summary>
        private const string TmpPath = "tmp\\";

        /// <summary>
        ///     The jobs queue.
        /// </summary>
        private readonly ConcurrentQueue<IJob> _jobsQueue = new ConcurrentQueue<IJob>();

        /// <summary>
        ///     The save job lock.
        /// </summary>
        private readonly object _saveJobLock = new object();

        /// <summary>
        ///     The _save result lock.
        /// </summary>
        private readonly object _saveResultLock = new object();


        /// <summary>
        /// The remaining time change event lock.
        /// </summary>
        private readonly object _remainingTimeChangeEventLock = new object();

        /// <summary>
        /// The remaining time lock.
        /// </summary>
        private readonly object _remainingTimeLock = new object();
        
        /// <summary>
        /// The task counter semaphore
        /// </summary>
        private readonly Semaphore _taskCounterSemaphore = new Semaphore(1, 1);

        /// <summary>
        ///     The number of concurrent tasks.
        /// </summary>
        private int _numberOfConcurrentTasks;

        /// <summary>
        ///     The running tasks count.
        /// </summary>
        private int _runningTasksCount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainController" /> class.
        /// </summary>
        public MainController()
        {
            FileHelper.CreateNonExistingDirectories(TmpPath);
            FileHelper.CreateNonExistingDirectories(ResultsPath);
            if (File.Exists(ConfigFileName + FileExtension))
            {
                var configBytes = FileHelper.ReadBinary(ConfigFileName + FileExtension);
                var config = (Configuration) Serializer.DeserializeObj(configBytes);
                NumberOfConcurrentTasks = config.NumberOfConcurrentJobs;
            }
            else
            {
                NumberOfConcurrentTasks = 1;
            }
        }

        /// <summary>
        ///     Gets or sets the number of concurrent tasks.
        /// </summary>
        /// <value>
        ///     The number of concurrent tasks.
        /// </value>
        public int NumberOfConcurrentTasks
        {
            get { return _numberOfConcurrentTasks; }
            set
            {
                var config = new Configuration {NumberOfConcurrentJobs = value};
                var configBytes = Serializer.SerializeObj(config);
                FileHelper.SaveBinary(ConfigFileName + FileExtension, configBytes);
                _numberOfConcurrentTasks = value;
                StartProcessing();
            }
        }


        /// <summary>
        ///     Gets the remaining time.
        /// </summary>
        /// <value>
        ///     The remaining time.
        /// </value>
        public TimeSpan RemainingTime { get; private set; }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            var initializationTask = new Task(() =>
            {
                var jobsBytes = FileHelper.GetSavedJobsAsBytes(TmpPath, FileExtension);
                foreach (var jobBytes in jobsBytes)
                {
                    var job = (IJob)Serializer.DeserializeObj(jobBytes);
                    _jobsQueue.Enqueue(job);
                    StartProcessing();
                    UpdateRemainingTime(job, RemainingTimeUpdateType.AddContinuedJobTime);
                }
            });
            
            initializationTask.Start();
        }

        /// <summary>
        /// Occurs when remaining time changes.
        /// </summary>
        public event EventHandler<RemainingTimeChangeEventArgs> OnRemainingTimeChangeEvent;

        /// <summary>
        /// Occurs when remaining time changes.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnExceptionEvent;

        /// <summary>
        ///     Adds the jobs.
        /// </summary>
        /// <param name="jobParameters">The job parameters.</param>
        /// <param name="files">The files.</param>
        public void AddJobs(SequentialOrderingJobParameters jobParameters, List<string> files)
        {
            foreach (var file in files)
            {
                IJob newJob;
                if (jobParameters.IsTestJob)
                {
                    newJob = new TesterJob(jobParameters, file);
                }
                else
                {
                    newJob = new NormalJob(jobParameters, file);
                }
                SaveJob(newJob, false);
                _jobsQueue.Enqueue(newJob);
                StartProcessing();
                UpdateRemainingTime(newJob, RemainingTimeUpdateType.AddNewJobTime);
            }
        }

        /// <summary>
        ///     Deletes the temporary file for job.
        /// </summary>
        /// <param name="job">The job.</param>
        private static void DeleteTmpFileForJob(IJob job)
        {
            File.Delete(TmpPath + job.GetTmpFileName() + FileExtension);
        }

        /// <summary>
        ///     Saves the job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="saveToResult">if set to <c>true</c> save as result; otherwise save to temporary location.</param>
        private void SaveJob(IJob job, bool saveToResult)
        {
            lock (_saveJobLock)
            {
                var path = saveToResult
                    ? ResultsPath + job.GetResultFileName() + FileExtension
                    : TmpPath + job.GetTmpFileName() + FileExtension;
                var jobAsBytes = Serializer.SerializeObj(job);
                FileHelper.SaveBinary(path, jobAsBytes);
            }
        }

        /// <summary>
        ///     Saves the result.
        /// </summary>
        /// <param name="job">The job.</param>
        private void SaveResult(IJob job)
        {
            lock (_saveResultLock)
            {
                FileHelper.SaveResultsTxt(ResultsPath, job);
                SaveJob(job, true);
                DeleteTmpFileForJob(job);
            }
        }

        /// <summary>
        ///     Called when remaining time changes.
        /// </summary>
        private void OnRemainingTimeChange()
        {
            lock (_remainingTimeChangeEventLock)
            {
                var onRemainingTimeChange = OnRemainingTimeChangeEvent;
                if (onRemainingTimeChange != null)
                {
                    var divisor = _runningTasksCount == 0 ? 1 : _runningTasksCount;
                    onRemainingTimeChange(this, new RemainingTimeChangeEventArgs() { RemainingTime = TimeSpan.FromTicks(RemainingTime.Ticks / divisor) });
                }
            }
        }

        /// <summary>
        ///     Starts the processing.
        /// </summary>
        private void StartProcessing()
        {
            _taskCounterSemaphore.WaitOne();
            while (_runningTasksCount < NumberOfConcurrentTasks && !_jobsQueue.IsEmpty)
            {
                
                _runningTasksCount++;
                _taskCounterSemaphore.Release();
                OnRemainingTimeChange();
                var task = new Task(() =>
                {
                    _taskCounterSemaphore.WaitOne();
                    while (!_jobsQueue.IsEmpty && _runningTasksCount <= NumberOfConcurrentTasks)
                    {
                        _taskCounterSemaphore.Release();
                        
                        IJob job;
                        if (_jobsQueue.TryDequeue(out job))
                        {
                            try
                            {
                                while (!job.IsFinished)
                                {
                                    job.DoOneStep();
                                    if (!job.IsFinished)
                                    {
                                        SaveJob(job, false);
                                        UpdateRemainingTime(job, RemainingTimeUpdateType.SubstractOneStepTime);
                                    }
                                }

                                UpdateRemainingTime(job, RemainingTimeUpdateType.SubstractOneStepTime);
                                SaveResult(job);
                            }
                            catch(Exception e)
                            {
                                e.Data.Add("job", job);
                                throw;
                            }
                        }

                        _taskCounterSemaphore.WaitOne();
                    }

                    _runningTasksCount--;

                    _taskCounterSemaphore.Release();

                    OnRemainingTimeChange();
                });

                task.ContinueWith((t) =>
                {
                    _taskCounterSemaphore.WaitOne();
                    _runningTasksCount--;
                    _taskCounterSemaphore.Release();

                    var onExceptionEvent = OnExceptionEvent;
                    if (onExceptionEvent != null)
                    {
                        onExceptionEvent(this, new ExceptionEventArgs() { Exception = t.Exception.InnerException });
                    }

                    ReloadJob((IJob)t.Exception.InnerException.Data["job"]);
                    StartProcessing();
                }, TaskContinuationOptions.OnlyOnFaulted);
                
                task.Start();
                _taskCounterSemaphore.WaitOne();
            }
            _taskCounterSemaphore.Release();
        }

        /// <summary>
        /// Reloads the job.
        /// </summary>
        /// <param name="reloadedJob">The reloaded job.</param>
        private void ReloadJob(IJob reloadedJob)
        {
            var binaryJob = FileHelper.ReadBinary(TmpPath + reloadedJob.GetTmpFileName() + FileExtension);
            var job = (IJob)Serializer.DeserializeObj(binaryJob);
            _jobsQueue.Enqueue(job);
            StartProcessing();
            UpdateRemainingTime(job, RemainingTimeUpdateType.AddContinuedJobTime);
            UpdateRemainingTime(reloadedJob, RemainingTimeUpdateType.SubstractJobTime);
        }

        /// <summary>
        ///     Updates the remaining time.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="updateType">Type of the update.</param>
        private void UpdateRemainingTime(IJob job, RemainingTimeUpdateType updateType)
        {
            lock (_remainingTimeLock)
            {
                switch (updateType)
                {
                    case RemainingTimeUpdateType.AddNewJobTime:
                        RemainingTime += job.TotalTimeNeeded;
                        break;

                    case RemainingTimeUpdateType.SubstractOneStepTime:
                        RemainingTime -= job.OneStepTime;
                        break;

                    case RemainingTimeUpdateType.AddContinuedJobTime:
                        RemainingTime += job.RemainingTime;
                        break;
                    case RemainingTimeUpdateType.SubstractJobTime:
                        RemainingTime -= job.RemainingTime;
                        break;
                }

                OnRemainingTimeChange();
            }
        }

        /// <summary>
        ///     RemainingTimeUpdateTypes.
        /// </summary>
        private enum RemainingTimeUpdateType
        {
            AddNewJobTime,
            AddContinuedJobTime,
            SubstractOneStepTime,
            SubstractJobTime
        };
    }
}