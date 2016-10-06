using System;
using System.Collections.Generic;
using System.Linq;
using Controllers.DataLayer;
using Microsoft.Office.Interop.Excel;

namespace Controllers
{
    /// <summary>
    ///     Provides methods for creating report.
    /// </summary>
    public static class ReportController
    {
        private const string AvgCostColumnName = "Avg cost";
        private const string AvgIterationsColumnName = "Avg iterations";
        private const string BestCostColumnName = "Best cost";
        private const string CostImprovementColumnName = "C";
        private const string FilesColumn = "A";
        private const string FilesColumnName = "file";
        private const string IterationImprovementColumnName = "I";
        private const string OriginalAvgCostColumn = "B";
        private const string OriginalAvgIterationsColumn = "C";
        private const string OriginalBestCostColumn = "D";
        private static readonly Dictionary<string, int> RowsOfFileNames = new Dictionary<string, int>();

        /// <summary>
        ///     Creates the report from results files.
        /// </summary>
        /// <param name="resultsPath">The results path.</param>
        /// <param name="originalConfigurationName">Name of the original configuration.</param>
        /// <exception cref="System.ArgumentException">There are no results for given configuration name.</exception>
        public static void CreateReportFromResultsFiles(string resultsPath, string originalConfigurationName)
        {
            var results = FileHelper.GetAllResults(resultsPath);
            var app = new Application();
            var workbook = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet sheet = workbook.Sheets.Add();
            sheet.Name = "Report";
            var originalResults =
                results.Where(result => result.ConfigurationName.Equals(originalConfigurationName)).ToList();
            if (!originalResults.Any())
            {
                workbook.Close(false);
                app.Quit();
                throw new ArgumentException("There are no results for given configuration name.");
            }

            AddOriginalConfiguration(originalResults, sheet);

            var configurations =
                results.Select(result => result.ConfigurationName).Distinct().ToList();

            configurations.Remove(originalConfigurationName);

            foreach (var configuration in configurations)
            {
                var firstColumnToUse = sheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell, Type.Missing).Column + 1;
                FillConfigurationName(firstColumnToUse, 5, sheet, configuration);
                FillConfigurationColumnsNames(firstColumnToUse, sheet);
                ApplyPercentageFormatting(firstColumnToUse, sheet);
                var configurationItems =
                    results.Where(result => result.ConfigurationName.Equals(configuration)).ToList();
                foreach (var result in configurationItems)
                {
                    AddResultData(result, firstColumnToUse, sheet);
                }
            }

            sheet.Columns.AutoFit();
            app.Visible = true;
            RowsOfFileNames.Clear();
        }

        /// <summary>
        ///     Adds the original configuration.
        /// </summary>
        /// <param name="originalResults">The original results.</param>
        /// <param name="sheet">The sheet.</param>
        private static void AddOriginalConfiguration(IReadOnlyList<ResultFileInfo> originalResults, Worksheet sheet)
        {
            sheet.Cells[2, FilesColumn].Value2 = FilesColumnName;
            FillConfigurationName(2, 3, sheet, originalResults[0].ConfigurationName);
            sheet.Cells[2, OriginalAvgCostColumn].Value2 = AvgCostColumnName;
            sheet.Cells[2, OriginalAvgIterationsColumn].Value2 = AvgIterationsColumnName;
            sheet.Cells[2, OriginalBestCostColumn].Value2 = BestCostColumnName;

            var rowIndex = 3;
            foreach (var originalResult in originalResults)
            {
                sheet.Cells[rowIndex, FilesColumn].Value2 = originalResult.FileName;
                RowsOfFileNames.Add(originalResult.FileName, rowIndex);
                sheet.Cells[rowIndex, OriginalAvgCostColumn].Value2 = originalResult.AverageSolutionCost;
                sheet.Cells[rowIndex, OriginalAvgIterationsColumn].Value2 = originalResult.AverageNumberOfIterations;
                sheet.Cells[rowIndex, OriginalBestCostColumn].Value2 = originalResult.BestSolutionCost;
                rowIndex++;
            }
        }

        /// <summary>
        ///     Adds the result data.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="firstColumnToUse">The first column to use.</param>
        /// <param name="sheet">The sheet.</param>
        private static void AddResultData(ResultFileInfo result, int firstColumnToUse, Worksheet sheet)
        {
            int rowNumber;
            if (!RowsOfFileNames.TryGetValue(result.FileName, out rowNumber))
            {
                rowNumber = RowsOfFileNames.Count + 3;
                RowsOfFileNames.Add(result.FileName, rowNumber);
                sheet.Cells[rowNumber, FilesColumn].Value2 = result.FileName;
            }
            var avgCostColumn = GetExcelColumnName(firstColumnToUse);
            var avgIterationsColumn = GetExcelColumnName(firstColumnToUse + 1);
            var bestCostColumn = GetExcelColumnName(firstColumnToUse + 2);
            var costInprovementColumn = GetExcelColumnName(firstColumnToUse + 3);
            var iterationsImprovementColumn = GetExcelColumnName(firstColumnToUse + 4);
            sheet.Cells[rowNumber, avgCostColumn].Value2 = result.AverageSolutionCost;
            sheet.Cells[rowNumber, avgIterationsColumn].Value2 = result.AverageNumberOfIterations;
            sheet.Cells[rowNumber, bestCostColumn].Value2 = result.BestSolutionCost;
            sheet.Cells[rowNumber, costInprovementColumn].Formula = string.Format("=({0}{1} - {2}{1})/{0}{1}",
                OriginalAvgCostColumn,
                rowNumber, avgCostColumn);
            sheet.Cells[rowNumber, iterationsImprovementColumn].Formula = string.Format("=-({0}{1} - {2}{1})/{0}{1}",
                OriginalAvgIterationsColumn,
                rowNumber, avgIterationsColumn);
        }

        /// <summary>
        ///     Applies the percentage formatting.
        /// </summary>
        /// <param name="firstColumnToUse">The first column to use.</param>
        /// <param name="sheet">The sheet.</param>
        private static void ApplyPercentageFormatting(int firstColumnToUse, Worksheet sheet)
        {
            sheet.Columns[firstColumnToUse + 3].NumberFormat = "0,00%";
            sheet.Columns[firstColumnToUse + 4].NumberFormat = "0,00%";
        }

        /// <summary>
        ///     Fills the configuration columns names.
        /// </summary>
        /// <param name="firstColumnToUse">The first column to use.</param>
        /// <param name="sheet">The sheet.</param>
        private static void FillConfigurationColumnsNames(int firstColumnToUse, Worksheet sheet)
        {
            sheet.Cells[2, firstColumnToUse].Value2 = AvgCostColumnName;
            sheet.Cells[2, firstColumnToUse + 1].Value2 = AvgIterationsColumnName;
            sheet.Cells[2, firstColumnToUse + 2].Value2 = BestCostColumnName;
            sheet.Cells[2, firstColumnToUse + 3].Value2 = CostImprovementColumnName;
            sheet.Cells[2, firstColumnToUse + 4].Value2 = IterationImprovementColumnName;
        }

        /// <summary>
        ///     Fills the name of the configuration.
        /// </summary>
        /// <param name="firstColumnToUse">The first column to use.</param>
        /// <param name="numberOfColumnsToMerge">The number of columns to merge.</param>
        /// <param name="sheet">The sheet.</param>
        /// <param name="configurationName">Name of the configuration.</param>
        private static void FillConfigurationName(int firstColumnToUse, int numberOfColumnsToMerge, Worksheet sheet,
            string configurationName)
        {
            sheet.Cells.Range[
                sheet.Cells[1, firstColumnToUse], sheet.Cells[1, firstColumnToUse + numberOfColumnsToMerge - 1]].Merge();
            sheet.Cells[1, firstColumnToUse].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            sheet.Cells[1, firstColumnToUse].Value2 = configurationName;
        }

        /// <summary>
        ///     Gets the name of the excel column.
        /// </summary>
        /// <param name="columnNumber">The column number.</param>
        /// <returns></returns>
        private static string GetExcelColumnName(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = String.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1)%26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo)/26;
            }

            return columnName;
        }
    }
}