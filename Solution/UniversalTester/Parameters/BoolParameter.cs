using System;
using System.Collections.Generic;
using System.Linq;

namespace UniversalTester.Parameters
{
    /// <summary>
    ///     Represents tester bool parameter.
    /// </summary>
    [Serializable]
    public class BoolParameter : Parameter
    {
        /// <summary>
        ///     The random generator.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoolParameter" /> class.
        /// </summary>
        /// <param name="random">The random generator.</param>
        public BoolParameter(Random random)
        {
            _random = random;
            DependentParameters = new HashSet<Parameter>();
            ToActivateWhenFalse = new HashSet<Parameter>();
            IsActiveChanged += (sender, e) =>
            {
                ChangeParametersActiveness(DependentParameters, Value && IsActive);
                ChangeParametersActiveness(ToActivateWhenFalse, !Value && IsActive);
            };
        }

        /// <summary>
        ///     Gets or sets the dependent parameters.
        /// </summary>
        /// <value>
        ///     The dependent parameters.
        /// </value>
        public HashSet<Parameter> DependentParameters { get; set; }

        /// <summary>
        ///     Gets or sets to activate when false.
        /// </summary>
        /// <value>
        ///     To activate when false.
        /// </value>
        public HashSet<Parameter> ToActivateWhenFalse { get; set; }

        /// <summary>
        ///     Gets or sets a value.
        /// </summary>
        /// <value>
        ///     Current value.
        /// </value>
        public bool Value { get; set; }

        /// <summary>
        ///     Clones this instance. Doesn't clone the dependencies.
        /// </summary>
        /// <returns>Clone.</returns>
        public override Parameter Clone()
        {
            return new BoolParameter(_random)
            {
                IsActive = IsActive,
                Value = Value,
                Name = Name,
            };
        }

        /// <summary>
        ///     Randomizes the instance.
        /// </summary>
        /// <param name="temperature">The temperature.</param>
        /// <param name="maxTemperature">The maximum temperature.</param>
        /// <returns><c>true</c> if value changed.</returns>
        public override bool Randomize(double temperature, double maxTemperature)
        {
            double changeChance;
            if (temperature == maxTemperature)
            {
                changeChance = 0.5;
            }
            else
            {
                changeChance = temperature/maxTemperature;
                ;
            }

            var change = _random.NextDouble() < changeChance;
            if (change)
            {
                Value = !Value;
                ChangeParametersActiveness(DependentParameters, Value && IsActive);
                ChangeParametersActiveness(ToActivateWhenFalse, !Value && IsActive);
            }

            return change;
        }

        /// <summary>
        ///     Changes the parameters activeness.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="value">if set to <c>true</c> parameters are activated.</param>
        private static void ChangeParametersActiveness(IEnumerable<Parameter> parameters, bool value)
        {
            foreach (var dependentParameter in parameters)
            {
                dependentParameter.IsActive = value;
                if (dependentParameter.IsActive)
                {
                    dependentParameter.Randomize(1, 1);
                }
            }
        }
    }
}