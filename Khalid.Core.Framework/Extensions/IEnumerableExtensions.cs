using System;
using System.Collections.Generic;
using System.Text;

namespace Khalid.Core.Framework
{
    public static class IEnumerableExtensions
    {
        // Possibly call this "Do"
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var e in source)
            {
                action(e);
                yield return e;
            }
        }
    }
}
