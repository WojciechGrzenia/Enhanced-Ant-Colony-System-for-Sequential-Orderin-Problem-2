using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Controllers.DataLayer.Helpers.DataFileClasses
{
    /// <summary>
    ///     Represents the content of TSPLIB file.
    /// </summary>
    public class TspLibDataFileContent : FileContent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TspLibDataFileContent" /> class.
        /// </summary>
        /// <param name="path">Path of the input file.</param>
        public TspLibDataFileContent(string path)
        {
            var content = File.ReadAllLines(path);
            Name = content[0].Replace("NAME: ", string.Empty).Trim();
            NumberOfDimensions = Convert.ToInt32(content[7].Trim());
            Values = new int[NumberOfDimensions, NumberOfDimensions];
            for (var i = 0; i < NumberOfDimensions; i++)
            {
                var lineValues = Regex.Matches(content[i + 8], @"[^\s]+");
                for (var j = 0; j < NumberOfDimensions; j++)
                {
                    Values[i, j] = Convert.ToInt32(lineValues[j].ToString());
                }
            }
        }
    }
}