using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Utils
{
    public static class CollectionUtil
    {
        /// <summary>
        /// Wraps the provided enumerable into a ReadOnlyCollection{T}
        /// 
        /// Copies all of the data into a new array, so the data can't be
        /// changed after creation. The exception is if the enumerable is
        /// already a ReadOnlyCollection{T}, in which case we just return it.
        /// </summary>
        internal static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return EmptyReadOnlyCollection<T>.Instance;
            }

            if (enumerable is ReadOnlyCollection<T> roCollection)
            {
                return roCollection;
            }

            if (enumerable is ICollection<T> collection)
            {
                int count = collection.Count;
                if (count == 0)
                {
                    return EmptyReadOnlyCollection<T>.Instance;
                }

                T[] array = new T[count];
                collection.CopyTo(array, 0);
                return new ReadOnlyCollection<T>(array);
            }

            // ToArray trims the excess space and speeds up access
            return new ReadOnlyCollection<T>(new List<T>(enumerable).ToArray());
        }

        internal static class EmptyReadOnlyCollection<T>
        {
            internal static ReadOnlyCollection<T> Instance = new ReadOnlyCollection<T>(new T[0]);
        }
    }
}
