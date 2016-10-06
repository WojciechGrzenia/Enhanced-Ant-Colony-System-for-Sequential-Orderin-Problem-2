using System;

namespace UniversalTester.Parameters
{
    /// <summary>
    ///     Represents tester parameter.
    /// </summary>
    [Serializable]
    public abstract class Parameter
    {
        /// <summary>
        ///     Indicates whether this instance is used in the action performed by tester.
        /// </summary>
        private bool _isActive;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    var action = IsActiveChanged;
                    if (action != null)
                    {
                        action(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Occurs when activeness changed.
        /// </summary>
        protected event EventHandler<EventArgs> IsActiveChanged;

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns>Clone.</returns>
        public abstract Parameter Clone();

        /// <summary>
        ///     Randomizes the instance.
        /// </summary>
        /// <param name="temperature">The temperature.</param>
        /// <param name="maxTemperature">The maximum temperature.</param>
        /// <returns><c>true</c> if value changed.</returns>
        public abstract bool Randomize(double temperature, double maxTemperature);
    }
}