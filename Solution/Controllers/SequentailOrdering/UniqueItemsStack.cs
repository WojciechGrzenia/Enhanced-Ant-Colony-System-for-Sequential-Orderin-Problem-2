using System.Collections.Generic;
using System.Linq;

namespace Controllers.SequentailOrdering
{
    /// <summary>
    ///     Stack with unique items.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public class UniqueItemsStack<T> : Stack<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UniqueItemsStack{T}" /> class.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        public UniqueItemsStack(IEnumerable<T> enumerable)
            : base(enumerable.Distinct())
        {
        }

        /// <summary>
        ///     Pushes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public new void Push(T item)
        {
            if (!Contains(item))
            {
                base.Push(item);
            }
        }
    }
}