namespace Controllers.DataLayer.Helpers.DataFileClasses
{
    /// <summary>
    ///     Represents data file content.
    /// </summary>
    public abstract class FileContent
    {
        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; protected set; }

        /// <summary>
        ///     Gets or sets the number of dimensions.
        /// </summary>
        /// <value>
        ///     The number of dimensions.
        /// </value>
        public int NumberOfDimensions { get; protected set; }

        /// <summary>
        ///     Gets or sets the values of routes matrix.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        public int[,] Values { get; protected set; }
    }
}