using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Controllers;
using DTO;

namespace Ants
{
    /// <summary>
    ///     Main form class.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        ///     The main controller.
        /// </summary>
        private readonly MainController _mainController = new MainController();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainForm" /> class.
        /// </summary>
        public MainForm()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
            InitializeComponent();
            var lockObject = new object();
            _mainController.OnRemainingTimeChangeEvent += (sender, args) =>
            {
                lock (lockObject)
                {
                    var remainingTime = args.RemainingTime;
                    this.BeginInvoke((Action)(() =>
                    {
                        labelProcessing.Visible = remainingTime.Ticks != 0;
                        labelTimeLeft.Visible = remainingTime.Ticks != 0;
                        labelTimeLeft.Text = String.Format("Time left: {0}", remainingTime);
                    }));
                }
            };

            _mainController.OnExceptionEvent += (sender, args) =>
            {
                lock (lockObject)
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        MessageBox.Show(args.Exception.ToString());
                    }));
                }
            };

            numericUpDownMaxNumberOfConcurrentJobs.Value = _mainController.NumberOfConcurrentTasks;
            comboBoxEatPheromoneDuringLocalSearch.SelectedIndex = 0;
            _mainController.Initialize();
        }

        /// <summary>
        ///     Applies the lower and upper bounds to the numerical text value of the control.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        private static void ApplyLowerUpperBounds(Control textBox, EventArgs e, double min, double max)
        {
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                double value = Convert.ToDouble(textBox.Text);
                if (value < min)
                {
                    textBox.Text = min.ToString(CultureInfo.CurrentCulture);
                }
                else if (value > max)
                {
                    textBox.Text = max.ToString(CultureInfo.CurrentCulture);
                }
            }
        }

        /// <summary>
        ///     Rejects the non int characters.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private static void RejectNonIntCharacters(TextBox textBox, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Handles the Click event of the buttonAddToQueue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void buttonAddToQueue_Click(object sender, EventArgs e)
        {
            var parameters = GetParametersFromForm();
            var files = GetFilesFromDataGridView();
            _mainController.NumberOfConcurrentTasks = Convert.ToInt32(numericUpDownMaxNumberOfConcurrentJobs.Value);
            _mainController.AddJobs(parameters, files);
        }

        /// <summary>
        ///     Handles the Click event of the buttonApplyNumberOfJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void buttonApplyNumberOfJobs_Click(object sender, EventArgs e)
        {
            _mainController.NumberOfConcurrentTasks = Convert.ToInt32(numericUpDownMaxNumberOfConcurrentJobs.Value);
        }

        /// <summary>
        ///     Handles the Click event of the buttonBrowse control. Pops up dialog allowing to pick input data files.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "SOP problem files (*.sop)|*.sop|All files (*.*)|*.*";
                dialog.Multiselect = true;
                dialog.ShowDialog();
                foreach (string path in dialog.FileNames)
                {
                    dataGridViewFiles.Rows.Add(Path.GetFileName(path), path);
                }
            }
        }

        /// <summary>
        ///     Gets the files from data grid view.
        /// </summary>
        /// <returns>List of paths from data grid view.</returns>
        private List<string> GetFilesFromDataGridView()
        {
            var result = new List<string>();
            foreach (DataGridViewRow row in dataGridViewFiles.Rows)
            {
                result.Add(row.Cells[1].Value.ToString());
            }

            return result;
        }

        /// <summary>
        ///     Gets the parameters from form.
        /// </summary>
        /// <returns>Sequential ordering job parameters.</returns>
        private SequentialOrderingJobParameters GetParametersFromForm()
        {
            var parameters = new SequentialOrderingJobParameters
            {
                EvaporatePheromones = checkBoxEvaporatePheromones.Checked,
                TestEvaporatePheromones = checkBoxTestEvaporatePheromones.Checked,
                CoefficientOfEvaporation = Convert.ToDouble(textBoxCoefficientOfEvaporation.Text),
                TestCoefficientOfEvaporation = checkBoxTestCoefficientOfEvaporation.Checked,
                InitialPheromoneAmount = Convert.ToDouble(textBoxInitialPheromoneAmount.Text),
                TestInitialPheromoneAmount = checkBoxTestInitialPheromoneAmount.Checked,
                NumberOfAnts = Convert.ToInt32(textBoxNumberOfAnts.Text),
                TestNumberOfAnts = checkBoxTestNumberOfAnts.Checked,
                CalculationTime = dateTimePickerCalculationTime.Value.TimeOfDay,
                NumberOfTrials = Convert.ToInt32(textBoxNumberOfTrials.Text),
                ExpectedNumberOfProbabilisticRuleUses =
                    Convert.ToDouble(textBoxExpectedNumberOfProbabilisticRuleUses.Text),
                TestExpectedNumberOfProbabilisticRuleUses =
                    checkBoxTestExpectedNumberOfProbabilisticRuleUses.Checked,
                CandidateListParameters = new CandidateListParameters
                {
                    UseCandidateList = checkBoxUseCandidateList.Checked,
                    UseFixedSize = checkBoxUseFixedSize.Checked,
                    FractionOfMaxSize = Convert.ToDouble(textBoxFractionOfMaxSize.Text),
                    IfNoneIsFeasibleTakeNextBest = checkBoxIfNoneIsFeasibleTakeNextBest.Checked,
                    Size = Convert.ToInt32(textBoxFixedSize.Text)
                },
                TestUseCandidateList = checkBoxTestUseCandidateList.Checked,
                TestUseFixedSize = checkBoxTestUseFixedSize.Checked,
                TestFractionOfMaxSize = checkBoxTestFractionOfMaxSize.Checked,
                TestIfNoneIsFeasibleTakeNextBest = checkBoxTestIfNoneFeasibleTakeNextBest.Checked,
                TestFixedSize = checkBoxTestFixedSize.Checked,
                PheromoneEatParameter = Convert.ToDouble(textBoxPheromoneEatParameter.Text),
                TestPheromoneEatParameter = checkBoxTestPheromoneEatParameter.Checked,
                UseEarlySolutionForInitialPheromoneAmount =
                    checkBoxUseEarlySolutionForInitialPheromoneAmount.Checked,
                TestUseEarlySolutionForInitialPheromoneAmount =
                    checkBoxTestUseEarlySolutionForInitialPheromoneAmount.Checked,
                BreakExpensiveSolutions = checkBoxBreakExpensiveSolutions.Checked,
                TestBreakExpensiveSolution = checkBoxTestBreakExpensiveSolutions.Checked,
                UseMetRestrictionsCounter = checkBoxUseMetRestrictionsCounter.Checked,
                TestUseMetRestrictionsCounter = checkBoxTestUseMetRestrictionsCounter.Checked,
                UseRouteCostInAddingPheromone = checkBoxUseRouteCostInAddingPheromone.Checked,
                TestUseRouteCostInAddingPheromone = checkBoxTestUseRouteCostInAddingPheromone.Checked,
                UseRouteCostInEvaporation = checkBoxUseRouteCostInEvaporation.Checked,
                TestUseRouteCostInEvaporation = checkBoxTestUseRouteCostInEvaporation.Checked,
                AllowToPickBestRouteOnlyByDeterministicRule =
                    checkBoxAllowToPickBestRouteOnlyByDeterministicRule.Checked,
                TestAllowToPickBestRouteOnlyByDeterministicRule =
                    checkBoxTestAllowToPickBestRouteOnlyByDeterministicRule.Checked,
                DecreaseHeuristicMeaningInTime = checkBoxDecreaseHeuristicMeaningInTime.Checked,
                TestDecreaseHeuristicMeaningInTime = checkBoxTestDecreaseHeuristicMeaningInTime.Checked,
                InitialHeuristicMeaning = Convert.ToDouble(textBoxInitialHeuristicMeaning.Text),
                TestInitialHeuristicMeaning = checkBoxTestInitialHeuristicMeaning.Checked,
                DontUseHeuristic = checkBoxDontUseHeuristic.Checked,
                TestDontUseHeuristic = checkBoxTestDontUseHeuristic.Checked,
                UseRouteCostInSettingInitialPheromoneAmount =
                    checkBoxUseRouteCostInSettingInitialPheromoneAmount.Checked,
                TestUseRouteCostInSettingInitialPheromoneAmount =
                    checkBoxTestUseRouteCostInSettingInitialPheromoneAmount.Checked,
                UseDynamicNumberOfProbabilisticRuleUses = checkBoxUseDynamicNumberOfProbabilisticRuleUses.Checked,
                TestUseDynamicNumberOfProbabilisticRuleUses =
                    checkBoxTestUseDynamicNumberOfProbabilisticRuleUses.Checked,
                ChangeNumberOfProbabilisticRuleUsesInTime =
                    checkBoxChangeNumberOfProbabilisticRuleUsesInTime.Checked,
                TestChangeNumberOfProbabilisticRuleUsesInTime =
                    checkBoxTestChangeNumberOfProbabilisticRuleUsesInTime.Checked,
                TimeProbabilisticChangeFactor = Convert.ToDouble(textBoxTimeProbabilisticChangeFactor.Text),
                TestTimeProbabilisticChangeFactor = checkBoxTestTimeProbabilisticChangeFactor.Checked,
                ProbabilisticChangeBorder = Convert.ToDouble(textBoxProbabilisticChangeBorder.Text),
                TestProbabilisticChangeBorder = checkBoxTestProbabilisticChangeBorder.Checked,
                IncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement =
                    checkBoxIncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement.Checked,
                TestIncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement =
                    checkBoxTestIncreaseNumberOfProbabilisticRuleUsesWhenNoImprovement.Checked,
                NoImprovementProbabilisticIncreaseFactor =
                    Convert.ToDouble(textBoxNoImprovementProbabilisticIncreaseFactor.Text),
                TestNoImprovementProbabilisticIncreaseFactor =
                    checkBoxTestNoImprovementProbabilisticIncreaseFactor.Checked,
                ResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound =
                    checkBoxResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound.Checked,
                TestResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound =
                    checkBoxTestResetExpectedNumberOfProbabilisticRuleOnBestSolutionFound.Checked,
                ForceAtLeastOneRouteChange = checkBoxForceAtLeastOneRouteChange.Checked,
                TestForceAtLeastOneRouteChange = checkBoxTestForceAtLeastOneRouteChange.Checked,
                TestEatPheromoneDuringLocalSearch = checkBoxTestEatPheromoneDuringLocalSearch.Checked,
                AcceptSolutionWithTheSameCost = checkBoxAcceptSolutionWithTheSameCost.Checked,
                TestAcceptSolutionWithTheSameCost = checkBoxTestAcceptSolutionWithTheSameCost.Checked,
                ConfigurationName = textBoxConfigurationName.Text,
                HeuristicMeaningDecreaseFactor = Convert.ToDouble(textBoxHeuristicMeaningDecreaseFactor.Text),
                TestHeuristicMeaningDecreaseFactor = checkBoxTestHeuristicMeaningDecreaseFactor.Checked,
                CalculationTimeOfTester = dateTimePickerMaxTestComputationTime.Value.TimeOfDay,
                UseChangedIndexesInStackInitialization = checkBoxUseChangedIndexesInStackInitialization.Checked,
                TestUseChangedIndexesInStackInitialization = checkBoxTestUseChangedIndexesInStackInitialization.Checked
            };

            switch (comboBoxEatPheromoneDuringLocalSearch.SelectedItem.ToString())
            {
                case "Don't eat pheromone":
                    parameters.EatPheromoneDuringLocalSearch = EatPheromoneDuringLocalSearch.DontEatPheromone;
                    break;

                case "On checked routes":
                    parameters.EatPheromoneDuringLocalSearch = EatPheromoneDuringLocalSearch.EatOnlyOnChangedRoutes;
                    break;

                case "On whole path":
                    parameters.EatPheromoneDuringLocalSearch = EatPheromoneDuringLocalSearch.EatOnWholePath;
                    break;
            }

            return parameters;
        }

        /// <summary>
        ///     Handles the ValueChanged event of the numericUpDownMaxNumberOfConcurrentJobs control.
        ///     It doesn't allow to select value lower than 1.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void numericUpDownMaxNumberOfConcurrentJobs_ValueChanged(object sender, EventArgs e)
        {
            numericUpDownMaxNumberOfConcurrentJobs.Value =
                Math.Round(numericUpDownMaxNumberOfConcurrentJobs.Value);
            if (numericUpDownMaxNumberOfConcurrentJobs.Value < 1)
            {
                numericUpDownMaxNumberOfConcurrentJobs.Value = 1;
            }
        }

        /// <summary>
        ///     Rejects the non double characters.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private static void RejectNonDoubleCharacters(Control textBox, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            // Allow only one decimal point.
            if (e.KeyChar == '.'
                && textBox.Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxCoefficientOfEvaporation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxCoefficientOfEvaporation_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxCoefficientOfEvaporation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxCoefficientOfEvaporation_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 0, 1);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxExpectedNumberOfProbabilisticRuleUses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxExpectedNumberOfProbabilisticRuleUses_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox)sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxExpectedNumberOfProbabilisticRuleUses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxExpectedNumberOfProbabilisticRuleUses_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 0, double.MaxValue);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxFixedSize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxFixedSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonIntCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxFixedSize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxFixedSize_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 1, int.MaxValue);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxFractionOfMaxSize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxFractionOfMaxSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxFractionOfMaxSize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxFractionOfMaxSize_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 0, 1);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxHeuristicMeaningDecreaseFactor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxHeuristicMeaningDecreaseFactor_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxHeuristicMeaningDecreaseFactor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxHeuristicMeaningDecreaseFactor_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 0, 1);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxInitialHeuristicMeaning control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxInitialHeuristicMeaning_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxInitialPheromoneAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxInitialPheromoneAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxNoImprovementProbabilisticIncreaseFactor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxNoImprovementProbabilisticIncreaseFactor_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxNoImprovementProbabilisticIncreaseFactor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxNoImprovementProbabilisticIncreaseFactor_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 0, 1);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxNumberOfAnts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxNumberOfAnts_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonIntCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxNumberOfAnts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxNumberOfAnts_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 1, int.MaxValue);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxNumberOfTrials control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxNumberOfTrials_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonIntCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxNumberOfTrials control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxNumberOfTrials_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 1, int.MaxValue);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxPheromoneEatParameter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxPheromoneEatParameter_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxPheromoneEatParameter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxPheromoneEatParameter_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 0, 1);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxProbabilisticChangeBorder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxProbabilisticChangeBorder_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        ///     Handles the TextChanged event of the textBoxProbabilisticChangeBorder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void textBoxProbabilisticChangeBorder_TextChanged(object sender, EventArgs e)
        {
            ApplyLowerUpperBounds((TextBox) sender, e, 0, 1);
        }

        /// <summary>
        ///     Handles the KeyPress event of the textBoxTimeProbabilisticChangeFactor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs" /> instance containing the event data.</param>
        private void textBoxTimeProbabilisticChangeFactor_KeyPress(object sender, KeyPressEventArgs e)
        {
            RejectNonDoubleCharacters((TextBox) sender, e);
        }

        /// <summary>
        /// Handles the Click event of the buttonCreateReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonCreateReport_Click(object sender, EventArgs e)
        {
            try
            {
                ReportController.CreateReportFromResultsFiles(MainController.ResultsPath,
                this.textBoxReportConfigurationName.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error occurred: {0}", ex.Message));
            }
        }
    }
}