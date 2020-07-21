using XLocalizer.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq;

namespace XLocalizer.DB
{
    /// <summary>
    /// Interface to implement CRUD data operations for database resources
    /// </summary>
    public interface IDbResourceManager
    {
        /// <summary>
        /// Add a new resource to the database
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> AddResourceAsync<TResource>(TResource entity) 
            where TResource : class, IXDbResource;

        /// <summary>
        /// Try get a localized value from database resource
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue<TResource>(string key, out string value) 
            where TResource : class, IXDbResource;

        /// <summary>
        /// Delete a resource entity from database
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<bool> DeleteResourceAsync<TResource>(Expression<Func<TResource, bool>> expression)
            where TResource : class, IXDbResource;

        /// <summary>
        /// Update a resource
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> UpdateResourceAsync<TResource>(TResource entity)
            where TResource : class, IXDbResource;

        /// <summary>
        /// Get specified count of resources
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="searchExp"></param>
        /// <returns>object of two parts, first one is a list of items, second one is the total recods</returns>
        Task<(IEnumerable<TResource> list, int total)> ResourcesSetListAsync<TResource>(int start, int size, List<Expression<Func<TResource, bool>>> searchExp)
            where TResource : class, IXDbResource;

        /// <summary>
        /// Get a resource for editing
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<TResource> GetResourceAsync<TResource>(Expression<Func<TResource, bool>> expression)
            where TResource : class, IXDbResource;

        /// <summary>
        /// Updates all resources with the same oldKey to the newKey for all localized cultures.
        /// If the new key is existed before operation will be canceled
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        Task<int> BulkUpdateAsync<TResource>(string oldKey, string newKey)
            where TResource : class, IXDbResource;
    }
}