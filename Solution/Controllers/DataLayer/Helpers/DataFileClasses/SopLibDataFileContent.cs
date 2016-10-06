using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Controllers.DataLayer.Helpers.DataFileClasses
{
    /// <summary>
    ///     Represents the content of SOPLIB file.
    /// </summary>
    public class SopLibDataFileContent : FileContent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SopLibDataFileContent" /> class.
        /// </summary>
        /// <param name="path">Path of the input file.</param>
        public SopLibDataFileContent(string path)
        {
            var content = File.ReadAllLines(path);
            Name = Path.GetFileName(path);
            NumberOfDimensions = content.Length;
            Values = new int[NumberOfDimensions, NumberOfDimensions];
            for (var i = 0; i < NumberOfDimensions; i++)
            {
                var lineValues = Regex.Matches(content[i], @"[^\s]+");
                for (var j = 0; j < NumberOfDimensions; j++)
                {
                    Values[i, j] = Convert.ToInt32(lineValues[j].ToString());
                }
            }
        }
    }
}