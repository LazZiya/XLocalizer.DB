using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XLocalizer.DB.Models;

namespace XLocalizer.DB
{
    /// <summary>
    /// Interface to implement resource exporters from db to .resx file
    /// </summary>
    public interface IDbResourceExporter
    {
        /// <summary>
        /// Export resources of specified type and culture to resx
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="culture"></param>
        /// <param name="approvedOnly"></param>
        /// <param name="overwriteExistingResources"></param>
        /// <returns></returns>
        Task<int> ExportToResx<TResource>(string culture, bool approvedOnly, bool overwriteExistingResources)
            where TResource : class, IXDbResource;

        /// <summary>
        /// Export resources of specified type and culture to resx
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="culture"></param>
        /// <param name="overwrite">Set to true to overwrite existing resources</param>
        /// <param name="p">start fro page number</param>
        /// <param name="s">page size, count of data to be read by one shot</param>
        /// <returns></returns>
        Task<int> ExportToResx<TResource>(Expression<Func<TResource, bool>> expression, string culture, bool overwrite, int p = 1, int s = 100)
            where TResource : class, IXDbResource;
    }
}
