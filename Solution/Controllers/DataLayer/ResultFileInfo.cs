using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Controllers.DataLayer
{
    public class ResultFileInfo
    {
        public string ConfigurationName { get; set; }
        public string FileName { get; set; }
        public double AverageSolutionCost { get; set; }
        public double AverageNumberOfIterations { get; set; }
        public double BestSolutionCost { get; set; }
    }
}
