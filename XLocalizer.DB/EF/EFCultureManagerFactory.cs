using XLocalizer.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace XLocalizer.DB.EF
{
    /// <summary>
    /// Manage localizaton cultures
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class EFCultureManagerFactory<TContext> : IDbCultureManager 
        where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> _contextFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initialize a new instance of <see cref="EFCultureManagerFactory{TContext}"/>
        /// </summary>
        /// <param name="dbContextFactory"></param>
        /// <param name="logger"></param>
        public EFCultureManagerFactory(IDbContextFactory<TContext> dbContextFactory, ILogger<EFCultureManagerFactory<TContext>> logger)
        {
            _contextFactory = dbContextFactory ?? throw new NotImplementedException(nameof(dbContextFactory));

            _logger = logger;
        }

        /// <summary>
        /// Add new culture
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public async Task<bool> AddCultureAsync(XDbCulture culture)
        {
            bool suscess;

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();

                dbContext.Entry<XDbCulture>(culture).State = EntityState.Added;

                suscess = await dbContext.SaveChangesAsync() > 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                suscess = false;
            }

            return suscess;
        }

        /// <summary>
        /// Delete culture and relevant records!
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCultureAsync(string name)
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var entity = dbContext.Set<XDbCulture>().Find(name);

            if (entity != null)
                dbContext.Entry(entity).State = EntityState.Deleted;

            return await dbContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Get specified count of <see cref="XDbResource"/> as <see cref="XDbResourceListItem"/>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="searchExp"></param>
        /// <returns>object of two parts, first one is a list of items, second one is the total recods</returns>
        public async Task<(IEnumerable<XDbCulture> list, int total)> CulturesListAsync(int start, int size, List<Expression<Func<XDbCulture, bool>>> searchExp)
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var query = dbContext.Set<XDbCulture>()
                                .AsNoTracking()
                                .WhereList(searchExp);

            var total = await query.CountAsync();

            var list = await query.OrderBy(x => x.EnglishName)
                                  .Skip((start - 1) * size)
                                  .Take(size)
                                  .ToListAsync();

            return (list, total);
        }

        /// <summary>
        /// Get a culture
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<XDbCulture> GetCultureAsync(string name)
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.Set<XDbCulture>()
                                 .AsNoTracking()
                                 .SingleOrDefaultAsync(x => x.ID == name);
        }

        /// <summary>
        /// Update a resource
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateCultureAsync(XDbCulture entity)
        {
            using var dbContext = _contextFactory.CreateDbContext();
            // check if culture is being tracked
            var local = dbContext.Set<XDbCulture>().Local.SingleOrDefault(x => x.ID.Equals(entity.ID));

            // if entity is tracked detach it from context
            if (local != null)
                dbContext.Entry<XDbCulture>(local).State = EntityState.Detached;

            dbContext.Attach(entity).State = EntityState.Modified;

            return await dbContext.SaveChangesAsync() > 0;
        }
        
        /// <summary>
        /// Set a culture as default
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> SetAsDefaultAsync(string name)
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var newDefault = await dbContext.Set<XDbCulture>()
                                           .SingleOrDefaultAsync(x => x.ID == name);

            if (newDefault != null)
                newDefault.IsDefault = newDefault.IsActive = true;

            var oldDefault = await dbContext.Set<XDbCulture>()
                                           .Where(x => x.IsDefault == true)
                                           .ToListAsync();

            foreach (var c in oldDefault)
                c.IsDefault = false;

            return await dbContext.SaveChangesAsync() > 0;
        }
    }
}
