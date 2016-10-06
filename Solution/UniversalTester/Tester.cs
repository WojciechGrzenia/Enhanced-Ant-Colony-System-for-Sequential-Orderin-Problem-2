using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniversalTester.Parameters;

namespace UniversalTester
{
    /// <summary>
    ///     A class that allows to search minimum value for specified problem by searching for the best set of parameters.
    /// </summary>
    [Serializable]
    public class Tester
    {
        private readonly Random _random;

        /// <summary>
        ///     The action to perform.
        /// </summary>
        [NonSerialized] private Action _action;

        /// <summary>
        ///     The assessment of action.
        /// </summary>
        [NonSerialized] private Func<double> _assessment;

        /// <summary>
        ///     Indicates if tester in working.
        /// </summary>
        private bool _isInProgress;

        /// <summary>
        ///     The number of iterations without improvement.
        /// </summary>
        private int _numberOfIterationsWithoutImprovement;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Tester" /> class.
        /// </summary>
        /// <param name="random">The random generator.</param>
        public Tester(Random random)
        {
            _random = random;
            BestSolution = new Dictionary<string, Parameter>();
            MaxTemperature = 10;
            Parameters = new Dictionary<String, Parameter>();
            CoolingFactor = 0.99999;
            AcceptableNumberOfIterationWithNoImprovement = 10;
        }

        /// <summary>
        ///     Gets or sets the acceptable number of iteration with no improvement.
        /// </summary>
        /// <value>
        ///     The acceptable number of iteration with no improvement.
        /// </value>
        public double AcceptableNumberOfIterationWithNoImprovement { get; set; }

        /// <summary>
        ///     Gets or sets the action.
        /// </summary>
        /// <value>
        ///     The action.
        /// </value>
        public Action Action
        {
            get { return _action; }
            set { _action = value; }
        }

        /// <summary>
        ///     Gets or sets the assessment.
        /// </summary>
        /// <value>
        ///     The assessment.
        /// </value>
        public Func<double> Assessment
        {
            get { return _assessment; }
            set { _assessment = value; }
        }

        /// <summary>
        ///     Gets or sets the best solution.
        /// </summary>
        /// <value>
        ///     The best solution.
        /// </value>
        public Dictionary<String, Parameter> BestSolution { get; set; }

        /// <summary>
        ///     Gets or sets the best solution assessment.
        /// </summary>
        /// <value>
        ///     The best solution assessment.
        /// </value>
        public double BestSolutionAssessment { get; set; }

        /// <summary>
        ///     Gets or sets the cooling factor.
        /// </summary>
        /// <value>
        ///     The cooling factor.
        /// </value>
        public double CoolingFactor { get; set; }

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        /// <value>
        ///     The data.
        /// </value>
        public object Data { get; set; }

        /// <summary>
        ///     Gets or sets the maximum temperature.
        /// </summary>
        /// <value>
        ///     The maximum temperature.
        /// </value>
        public double MaxTemperature { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public Dictionary<String, Parameter> Parameters { get; set; }

        /// <summary>
        ///     Gets or sets the parameters assessment.
        /// </summary>
        /// <value>
        ///     The parameters assessment.
        /// </value>
        private double ParametersAssessment { get; set; }

        /// <summary>
        ///     Gets or sets the temperature.
        /// </summary>
        /// <value>
        ///     The temperature.
        /// </value>
        private double Temperature { get; set; }

        /// <summary>
        ///     Does the one step.
        /// </summary>
        public void DoOneStep()
        {
            if (!_isInProgress)
            {
                Initialize();
                _isInProgress = true;
                Action.Invoke();
                ParametersAssessment = BestSolutionAssessment = Assessment.Invoke();
                BestSolution =
                    Parameters.Select(parameter => parameter.Value.Clone()).ToDictionary(parameter => parameter.Name);
            }
            else
            {
                var parameters =
                    Parameters.Select(parameter => parameter.Value.Clone()).ToDictionary(parameter => parameter.Name);
                var parametersAssessment = ParametersAssessment;

                RandomizeParameter();

                Action.Invoke();
                ParametersAssessment = Assessment.Invoke();
                if (BestSolutionAssessment > ParametersAssessment)
                {
                    BestSolutionAssessment = ParametersAssessment;
                    BestSolution =
                        Parameters.Select(parameter => parameter.Value.Clone())
                            .ToDictionary(parameter => parameter.Name);
                    _numberOfIterationsWithoutImprovement = 0;
                }
                else
                {
                    _numberOfIterationsWithoutImprovement++;
                }

                if (!ShouldMoveToNewSolution(parametersAssessment))
                {
                    CopyValuesToParameters(parameters);
                    ParametersAssessment = parametersAssessment;
                }

                if (AcceptableNumberOfIterationWithNoImprovement > _numberOfIterationsWithoutImprovement)
                {
                    DecreaseTemperature();
                }
                else
                {
                    IncreaseTemperature(true);
                }
            }
        }

        /// <summary>
        /// Copies the values to parameters.
        /// </summary>
        /// <param name="sourceParameters">The source parameters.</param>
        private void CopyValuesToParameters(IReadOnlyDictionary<string, Parameter> sourceParameters)
        {
            foreach (var parameter in Parameters.Select(parameter => parameter.Value))
            {
                var sourceParameter = sourceParameters[parameter.Name];
                var valueProperty =
                    parameter.GetType().GetProperties().SingleOrDefault(property => property.Name.Equals("Value"));

                if (valueProperty != null)
                {
                    valueProperty.SetValue(parameter, valueProperty.GetValue(sourceParameter));
                }
            }
        }

        /// <summary>
        ///     Gets all active parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static List<Parameter> GetAllActiveParameters(IEnumerable<Parameter> parameters)
        {
            return parameters.Where(parameter => parameter.IsActive).ToList();
        }

        /// <summary>
        ///     Decreases the temperature.
        /// </summary>
        private void DecreaseTemperature()
        {
            Temperature *= CoolingFactor;
            CoolingFactor *= 1.000000001;
            if (CoolingFactor >= 1)
            {
                CoolingFactor = 1 - 0.0000000000001;
            }
        }

        /// <summary>
        ///     Gets the parameter to randomize.
        /// </summary>
        /// <returns></returns>
        private Parameter GetParameterToRandomize()
        {
            var parameters = GetAllActiveParameters(Parameters.Values.ToList());
            var randomNumber = _random.NextDouble();
            var randomIndex = Convert.ToInt32(Math.Round((parameters.Count - 1)*randomNumber));
            return parameters[randomIndex];
        }

        /// <summary>
        ///     Increases the temperature.
        /// </summary>
        private void IncreaseTemperature(bool decreaseCoolingFactor)
        {
            Temperature += 1.1*(Temperature/
                                Math.Pow(CoolingFactor, AcceptableNumberOfIterationWithNoImprovement + 1) - Temperature);
            _numberOfIterationsWithoutImprovement = 0;
            if (MaxTemperature < Temperature)
            {
                Temperature = MaxTemperature;
                if (decreaseCoolingFactor)
                {
                    AcceptableNumberOfIterationWithNoImprovement *= 1.1;
                    CoolingFactor *= 0.99;
                }
                else
                {
                    AcceptableNumberOfIterationWithNoImprovement *= 0.85;
                    CoolingFactor *= 1.015;
                }
            }
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            _numberOfIterationsWithoutImprovement = 0;
            Temperature = MaxTemperature;
        }

        /// <summary>
        ///     Randomizes the parameter.
        /// </summary>
        private void RandomizeParameter()
        {
            var success = false;
            for (var i = 0; i < 10; i++)
            {
                var parameter = GetParameterToRandomize();
                success = parameter.Randomize(Temperature, MaxTemperature);
                if (success)
                    break;
            }

            if(!success)
            {
                this.IncreaseTemperature(false);
                RandomizeParameter();
            }
        }

        /// <summary>
        ///     Should move to the new solution.
        /// </summary>
        /// <param name="oldParametersAssessment">The old parameters assessment.</param>
        /// <returns></returns>
        private bool ShouldMoveToNewSolution(double oldParametersAssessment)
        {
            var probability = Math.Min(1, Math.Exp(-(ParametersAssessment - oldParametersAssessment)/Temperature));

            return probability >= _random.NextDouble();
        }
    }
}