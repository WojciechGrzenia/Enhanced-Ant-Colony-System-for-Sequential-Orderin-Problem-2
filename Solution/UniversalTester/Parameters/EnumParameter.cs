using System;

namespace UniversalTester.Parameters
{
    /// <summary>
    ///     Represents tester enum parameter.
    /// </summary>
    [Serializable]
    public class EnumParameter : Parameter
    {
        /// <summary>
        ///     The random generator.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnumParameter" /> class.
        /// </summary>
        /// <param name="random">The random.</param>
        public EnumParameter(Random random)
        {
            _random = random;
        }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public Enum Value { get; set; }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override Parameter Clone()
        {
            return new EnumParameter(_random)
            {
                IsActive = IsActive,
                Name = Name,
                Value = Value
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
            var oldValueHash = Value.GetHashCode();
            var names = Enum.GetNames(Value.GetType());
            var step = 1.0/names.Length;
            var randomValue = _random.NextDouble();
            double sum = 0;
            var index = 0;
            while (sum < randomValue)
            {
                sum += step;
                index++;
            }

            var pickedName = names[index - 1];
            Value = (Enum) Enum.Parse(Value.GetType(), pickedName);
            return oldValueHash != Value.GetHashCode();
        }
    }
}