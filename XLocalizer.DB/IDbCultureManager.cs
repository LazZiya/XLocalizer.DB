using XLocalizer.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace XLocalizer.DB
{
    /// <summary>
    /// Manage localizaton cultures
    /// </summary>
    public interface IDbCultureManager
    {
        /// <summary>
        /// Add new culture
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        Task<bool> AddCultureAsync(XDbCulture culture);

        /// <summary>
        /// List cultures
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="searchExp"></param>
        /// <returns></returns>
        Task<(IEnumerable<XDbCulture> list, int total)> CulturesListAsync(int start, int size, List<Expression<Func<XDbCulture, bool>>> searchExp);

        /// <summary>
        /// Delete culture
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> DeleteCultureAsync(string name);

        /// <summary>
        /// Get a culture from db
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<XDbCulture> GetCultureAsync(string name);

        /// <summary>
        /// Update a resource
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> UpdateCultureAsync(XDbCulture entity);

        /// <summary>
        /// Set a culture as default
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> SetAsDefaultAsync(string name);
    }
}