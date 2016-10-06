using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Controllers.DataLayer
{
    /// <summary>
    ///     Provides methods to operate on files produced by this application.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        ///     Reads the binary data from given file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static byte[] ReadBinary(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bytes = new byte[file.Length];
                file.Read(bytes, 0, (int) file.Length);
                return bytes;
            }
        }

        /// <summary>
        ///     Saves the binary data as a file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        public static void SaveBinary(string path, byte[] data)
        {
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                file.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        ///     Saves the job results as text file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="job">The job.</param>
        public static void SaveResultsTxt(string path, IJob job)
        {
            using (var outfile = new StreamWriter(String.Format("{0}{1}.txt", path, job.GetResultFileName())))
            {
                outfile.Write(job.GetResultsAsString());
            }
        }

        /// <summary>
        ///     Gets the saved jobs as binary data.
        /// </summary>
        /// <param name="tmpPath">The temporary files path.</param>
        /// <param name="fileExtension">The files extension.</param>
        /// <returns>List of jobs binary data saved in given path.</returns>
        internal static List<byte[]> GetSavedJobsAsBytes(string tmpPath, string fileExtension)
        {
            var files = Directory.GetFiles(tmpPath, "*" + fileExtension);
            var fileInfos = files.Select(file => new FileInfo(file));
            files = fileInfos.OrderByDescending(fileInfo => fileInfo.LastAccessTime).Select(fileInfo => tmpPath + fileInfo.Name).ToArray();
            return files.Select(ReadBinary).ToList();
        }

        /// <summary>
        /// Creates the non existing directories.
        /// </summary>
        public static void CreateNonExistingDirectories(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        internal static List<ResultFileInfo> GetAllResults(string resultsPath)
        {
            var result = new List<ResultFileInfo>();
            var files = Directory.GetFiles(resultsPath, "*.txt");
            foreach (var file in files)
            {
                var fileContent = File.ReadLines(file).ToArray();
                if (IsResultFileContent(fileContent))
                {
                    result.Add(new ResultFileInfo()
                    {
                        ConfigurationName = fileContent[0].Replace("Configuration name: ",""),
                        FileName = fileContent[1].Replace("File name: ", ""),
                        AverageSolutionCost = Convert.ToDouble(fileContent[3].Replace("Average solution cost: ", "").Replace(",", ".")),
                        AverageNumberOfIterations = Convert.ToDouble(fileContent[4].Replace("Average iterations: ", "")),
                        BestSolutionCost = Convert.ToDouble(fileContent[5].Replace("Best solution cost: ", "").Replace(",","."))
                    });
                }
            }

            return result;
        }

        private static bool IsResultFileContent(string[] fileContent)
        {
            return fileContent[0].StartsWith("Configuration name: ")
                   && fileContent[1].StartsWith("File name: ")
                   && fileContent[2].StartsWith("Number of trials: ")
                   && fileContent[3].StartsWith("Average solution cost: ")
                   && fileContent[4].StartsWith("Average iterations: ")
                   && fileContent[5].StartsWith("Best solution cost: ");
        }
    }
}