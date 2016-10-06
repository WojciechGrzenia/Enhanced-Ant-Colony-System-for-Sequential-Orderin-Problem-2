using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers.DataLayer.Helpers.DataFileClasses;
using DTO;

namespace Controllers.DataLayer.Helpers
{
    /// <summary>
    ///     Provides methods to operate on data files.
    /// </summary>
    public static class DataFileHelper
    {
        /// <summary>
        ///     Gets the problem data from file.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <returns></returns>
        public static SequentialOrderingProblem GetProblemDataFromFile(string sourcePath)
        {
            var data = new List<Target>();
            var fileSource = GetFileSource(sourcePath);
            FileContent fileContent;
            if (fileSource == FileSource.TspLib)
            {
                fileContent = new TspLibDataFileContent(sourcePath);
            }
            else
            {
                fileContent = new SopLibDataFileContent(sourcePath);
            }

            for (var i = 0; i < fileContent.NumberOfDimensions; i++)
            {
                data.Add(new Target {Index = i});
            }

            for (var i = 0; i < fileContent.NumberOfDimensions; i++)
            {
                var consideredTarget = data[i];
                for (var j = 1; j < fileContent.NumberOfDimensions; j++)
                {
                    if (i != j)
                    {
                        if (fileContent.Values[i, j] == -1)
                        {
                            var restriction = new Restriction
                            {
                                TargetToVisit = consideredTarget,
                                NeededTarget = data[j]
                            };

                            data[j].RequiredBy.Add(restriction);
                            consideredTarget.Restrictions.Add(restriction);
                            consideredTarget.RestrictionsToMeet++;
                        }
                        else
                        {
                            consideredTarget.Routes.Add(new Route
                            {
                                Cost = fileContent.Values[i, j],
                                Origin = consideredTarget,
                                Target = data[j]
                            });
                        }
                    }
                }
            }

            foreach (var target in data)
            {
                target.Routes = target.Routes.OrderBy(route => route.Cost).ToList();
            }

            return new SequentialOrderingProblem(data) {Name = fileContent.Name};
        }

        /// <summary>
        /// Gets the file source (TSPLIB or SOPLIB).
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <returns></returns>
        private static FileSource GetFileSource(string sourcePath)
        {
            using (var sr = new StreamReader(sourcePath))
            {
                var character = (char) sr.Read();
                if (character == '0')
                {
                    return FileSource.SopLib;
                }

                return FileSource.TspLib;
            }
        }
    }
}