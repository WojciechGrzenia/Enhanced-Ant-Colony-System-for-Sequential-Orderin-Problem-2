using System;

namespace UniversalTester.Parameters
{
    /// <summary>
    ///     Represents tester double parameter.
    /// </summary>
    [Serializable]
    public class DoubleParameter : Parameter
    {
        /// <summary>
        ///     The random generator.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoubleParameter" /> class.
        /// </summary>
        /// <param name="random">The random generator.</param>
        public DoubleParameter(Random random)
        {
            _random = random;
        }

        /// <summary>
        ///     Gets or sets the maximum value.
        /// </summary>
        /// <value>
        ///     The maximum value.
        /// </value>
        public double MaxValue { get; set; }

        /// <summary>
        ///     Gets or sets the minimum value.
        /// </summary>
        /// <value>
        ///     The minimum value.
        /// </value>
        public double MinValue { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public double Value { get; set; }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override Parameter Clone()
        {
            return new DoubleParameter(_random)
            {
                IsActive = IsActive,
                Name = Name,
                Value = Value,
                MinValue = MinValue,
                MaxValue = MaxValue
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
            var oldValue = Value;
            var boundOffset = (temperature/maxTemperature)*(MaxValue - MinValue);
            var lowerBound = Math.Max(Value - boundOffset, MinValue);
            var upperBound = Math.Min(Value + boundOffset, MaxValue);
            var boundsDifferential = upperBound - lowerBound;
            Value = lowerBound + (_random.NextDouble()*boundsDifferential);
            return oldValue != Value;
        }
    }
}