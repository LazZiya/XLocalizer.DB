using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XLocalizer.DB.Models;
using XLocalizer.Resx;

namespace XLocalizer.DB.EF
{
    /// <summary>
    /// EF Resource exporter
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class EFDbResourceExporter<TContext> : IDbResourceExporter
        where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly XLocalizerOptions _options;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initialize a new instance of <see cref="EFDbResourceExporter{TContext}"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="loggerFactory"></param>
        public EFDbResourceExporter(TContext context, IOptions<XLocalizerOptions> options, ILoggerFactory loggerFactory)
        {
            _context = context ?? throw new NotImplementedException(nameof(context));

            _options = options.Value;

            _loggerFactory = loggerFactory;

            _logger = loggerFactory.CreateLogger<EFDbResourceExporter<TContext>>();
        }

        /// <summary>
        /// Export resources of specified type and culture to resx
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="culture"></param>
        /// <param name="approvedOnly"></param>
        /// <param name="overwriteExistingResources"></param>
        /// <returns></returns>
        public async Task<int> ExportToResx<TResource>(string culture, bool approvedOnly, bool overwriteExistingResources)
            where TResource : class, IXDbResource
        {
            Expression<Func<TResource, bool>> exp;

            if (approvedOnly)
                exp = x => x.CultureID == culture && x.IsActive == true;
            else
                exp = x => x.CultureID == culture;

            return await ExportToResx<TResource>(exp, culture, overwriteExistingResources, 1, 100);
        }

        /// <summary>
        /// Export resources of specified type and culture to resx
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="culture"></param>
        /// <param name="overwrite"></param>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public async Task<int> ExportToResx<TResource>(Expression<Func<TResource, bool>> expression, string culture, bool overwrite, int p, int s)
            where TResource : class, IXDbResource
        {
            ResxWriter manager = new ResxWriter(typeof(TResource), _options.ResourcesPath, culture, _loggerFactory);

            int totalRawData = await GetRawDataCountAsync<TResource>(expression);

            int totalExported = 0;

            for (int i = 0; i < totalRawData; i += s)
            {
                var dataLot = await GetRawDataListAsync<TResource>(p, s, expression);

                totalExported += await manager.AddRangeAsync(dataLot, overwrite);

                if (totalExported > 0)
                {
                    var saved = await manager.SaveAsync();
                    if (saved)
                        _logger.LogInformation($"Total '{totalExported}' resources exported to '{manager.ResourceFilePath}'");
                    else
                        _logger.LogError($"Exported not successful to '{manager.ResourceFilePath}'");
                }
            }

            return totalExported;
        }

        private async Task<IEnumerable<ResxElement>> GetRawDataListAsync<TResource>(int p, int s, Expression<Func<TResource, bool>> expression)
            where TResource : class, IXDbResource
        {
            return await _context.Set<TResource>()
                                 .AsNoTracking()
                                 .Where(expression)
                                 .Skip((p - 1) * s)
                                 .Take(s)
                                 .Select(x => new ResxElement
                                 {
                                     Key = x.Key,
                                     Value = x.Value,
                                     Comment = x.Comment,
                                 })
                                 .ToListAsync();
        }

        private async Task<int> GetRawDataCountAsync<TResource>(Expression<Func<TResource, bool>> expression)
            where TResource : class, IXDbResource
        {
            return await _context.Set<TResource>()
                                 .AsNoTracking()
                                 .Where(expression)
                                 .CountAsync();
        }


    }
}
