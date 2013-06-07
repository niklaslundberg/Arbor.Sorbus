using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Arbor.Sorbus.Core
{
    public static class ReadOnlyCollectionExtension
    {
        public static IReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable as IList<T>;

            if (list != null)
            {
                return new ReadOnlyCollection<T>(list);
            }

            return new ReadOnlyCollection<T>(enumerable.ToList());
        }
    }
}