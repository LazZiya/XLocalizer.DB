using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using XLocalizer.DB.Models;

namespace XLocalizer.DB.EF
{
    /// <summary>
    /// EF Localization provider
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class EFDbResourceProvider<TContext> : IDbResourceProvider
        where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly ILogger _logger;

        /// <summary>
        /// Initialize a new instance of <see cref="EFDbResourceProvider{TContext}"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public EFDbResourceProvider(TContext context, ILogger<EFDbResourceProvider<TContext>> logger)
        {
            _context = context ?? throw new NotImplementedException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// Try get a localized value from db
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
        /// Try save a localized value to db
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        public bool TrySetValue<TResource>(TResource resource)
            where TResource : class, IXDbResource
        {
            var existed = _context.Set<TResource>().AsNoTracking().SingleOrDefault(x => x.Key == resource.Key && x.CultureID == resource.CultureID);

            bool success = false;

            if (existed != null)
                _logger.LogInformation($"Resource already exists! Culture: '{resource.CultureID}' - Key: '{resource.Key}'");

            if (existed == null)
            {
                // var entity = DynamicObjectCreator.DbResource<TResource>(key, value);

                _context.Set<TResource>().Add(resource);

                try
                {
                    success = _context.SaveChanges() > 0;

                    if (success)
                        _logger.LogInformation($"New resource added. Culture: '{resource.CultureID}', Key: '{resource.Key}'");
                }
                catch (DbUpdateException e)
                {
                    _logger.LogError(e.Message);
                    success = false;
                }
            }

            return success;
        }
    }
}
