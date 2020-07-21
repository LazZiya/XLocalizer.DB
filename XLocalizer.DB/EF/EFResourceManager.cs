using XLocalizer.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace XLocalizer.DB.EF
{
    /// <summary>
    /// Manage CRUD operations for XLocalizer database resources
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class EFResourceManager<TContext> : IDbResourceManager
        where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly ILogger _logger;

        /// <summary>
        /// Create a new instance of <see cref="EFResourceManager{TContext}"/> based on the generic type
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public EFResourceManager(TContext context, ILogger<EFResourceManager<TContext>> logger)
        {
            _context = context ?? throw new NotImplementedException(nameof(context));

            _logger = logger;
        }

        /// <summary>
        /// Try get a localized value from database resource
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue<TResource>(string key, out string value)
            where TResource : class, IXDbResource
        {
            var culture = CultureInfo.CurrentCulture.Name;

            var v = _context.Set<TResource>()
                                  .AsNoTracking()
                                  .Where(x => x.Key == key && x.CultureID == culture)
                                  .Select(x => x.Value)
                                  .FirstOrDefault();
            if (v == null)
                _logger.LogWarning($"Resource not exist! Culture: '{culture}', Key: '{key}'");

            value = v ?? key;

            return v != null;
        }

        /// <summary>
        /// Add a new resource to the database
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> AddResourceAsync<TResource>(TResource entity)
            where TResource : class, IXDbResource
        {
            var existed = await _context.Set<TResource>().AsNoTracking().SingleOrDefaultAsync(x => x.Key == entity.Key && x.CultureID == entity.CultureID);

            bool success = false;

            if (existed != null)
                _logger.LogInformation($"Resource already exists! Culture: '{entity.CultureID}' - Key: '{entity.Key}'");

            if (existed == null)
            {
                // var entity = DynamicObjectCreator.DbResource<TResource>(key, value);

                _context.Set<TResource>().Add(entity);

                try
                {
                    success = await _context.SaveChangesAsync() > 0;

                    if (success)
                        _logger.LogInformation($"New resource added. Culture: '{entity.CultureID}', Key: '{entity.Key}'");
                }
                catch (DbUpdateException e)
                {
                    _logger.LogError(e.Message);
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Delete a resource entity from database
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<bool> DeleteResourceAsync<TResource>(Expression<Func<TResource, bool>> expression)
            where TResource : class, IXDbResource
        {
            var entity = await _context.Set<TResource>()
                                 .SingleOrDefaultAsync(expression);

            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Deleted;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Update a resource
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateResourceAsync<TResource>(TResource entity)
            where TResource : class, IXDbResource
        {
            // check if entity is being tracked
            var local = _context.Set<TResource>().Local.SingleOrDefault(x => x.ID.Equals(entity.ID));

            // if entity is tracked detach it from context
            if (local != null)
                _context.Entry<TResource>(local).State = EntityState.Detached;

            _context.Attach(entity).State = EntityState.Modified;

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Get specified count of resources
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="searchExp"></param>
        /// <returns>object of two parts, first one is a list of items, second one is the total recods</returns>
        public async Task<(IEnumerable<TResource> list, int total)> ResourcesSetListAsync<TResource>(int start, int size, List<Expression<Func<TResource, bool>>> searchExp)
            where TResource : class, IXDbResource
        {
            var query = _context.Set<TResource>()
                                .AsNoTracking()
                                .WhereList(searchExp);

            var total = await query.CountAsync();

            var list = await query.OrderBy(x => x.Key)
                                  .Skip((start - 1) * size)
                                  .Take(size)
                                  .ToListAsync();

            return (list, total);
        }

        /// <summary>
        /// Get a resource for editing
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<TResource> GetResourceAsync<TResource>(Expression<Func<TResource, bool>> expression)
            where TResource : class, IXDbResource
        {
            return await _context.Set<TResource>()
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(expression);
        }

        /// <summary>
        /// Updates all resources with the same oldKey to the newKey for all localized cultures.
        /// If the new key is existed before operation will be canceled
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public async Task<int> BulkUpdateAsync<TResource>(string oldKey, string newKey)
            where TResource : class, IXDbResource
        {
            var oldResources = _context.Set<TResource>()
                                             .Where(x => x.Key == oldKey)
                                             .AsEnumerable();

            foreach (var r in oldResources)
            {
                r.Key = newKey;
            }

            return await _context.SaveChangesAsync();
        }
    }
}