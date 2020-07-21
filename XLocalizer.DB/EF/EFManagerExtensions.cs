using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace XLocalizer.DB.EF
{
    /// <summary>
    /// Extension methods for Express managers...
    /// </summary>
    internal static class EFManagerExtensions
    {
        /// <summary>
        /// Converts a list of search expressions to IQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predications"></param>
        /// <returns></returns>
        internal static IQueryable<T> WhereList<T>(this IQueryable<T> source, List<Expression<Func<T, bool>>> predications)
            where T : class
        {
            if (predications != null)
            {
                foreach (var p in predications)
                {
                    source = source.Where(p);
                }
            }

            return source;
        }
    }
}
