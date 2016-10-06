using System;

namespace UniversalTester.Parameters
{
    /// <summary>
    ///     Represents tester int parameter.
    /// </summary>
    [Serializable]
    public class IntParameter : Parameter
    {
        /// <summary>
        ///     The random generator.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntParameter" /> class.
        /// </summary>
        /// <param name="random">The random.</param>
        public IntParameter(Random random)
        {
            _random = random;
        }

        /// <summary>
        ///     Gets or sets the maximum value.
        /// </summary>
        /// <value>
        ///     The maximum value.
        /// </value>
        public int MaxValue { get; set; }

        /// <summary>
        ///     Gets or sets the minimum value.
        /// </summary>
        /// <value>
        ///     The minimum value.
        /// </value>
        public int MinValue { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public int Value { get; set; }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override Parameter Clone()
        {
            return new IntParameter(_random)
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
            Value = (int) Math.Round(lowerBound + (_random.NextDouble()*boundsDifferential));
            return oldValue == Value;
        }
    }
}