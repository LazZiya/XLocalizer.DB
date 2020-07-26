using XLocalizer.Common;
using XLocalizer.DB.Models;
using XLocalizer.Translate;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;

namespace XLocalizer.DB
{
    /// <summary>
    /// DbStringLocalizer factory
    /// </summary>
    public class DbStringLocalizerFactory<TResource> : IXStringLocalizerFactory
        where TResource : class, IXDbResource, new()
    {
        private readonly IDbResourceProvider _provider;
        private readonly ITranslatorFactory _translatorFactory;
        private readonly ExpressMemoryCache _cache;
        private readonly IOptions<XLocalizerOptions> _options;
        private readonly IOptions<RequestLocalizationOptions> _localizationOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConcurrentDictionary<string, IStringLocalizer> _localizerCache = new ConcurrentDictionary<string, IStringLocalizer>();

        /// <summary>
        /// Initialize a new instance of DbStringLocalizerFactory
        /// </summary>
        public DbStringLocalizerFactory(IDbResourceProvider provider,
                                        ITranslatorFactory translatorFactory,
                                        ExpressMemoryCache cache,
                                        IOptions<XLocalizerOptions> options,
                                        IOptions<RequestLocalizationOptions> localizationOptions,
                                        ILoggerFactory loggerFactory)
        {
            _provider = provider;
            _translatorFactory = translatorFactory;
            _cache = cache;
            _options = options;
            _loggerFactory = loggerFactory;
            _localizationOptions = localizationOptions;
        }

        /// <summary>
        /// Create a new IStringLocalizer using the default settings
        /// </summary>
        /// <returns></returns>
        public IStringLocalizer Create()
        {
            return _localizerCache.GetOrAdd(typeof(TResource).FullName, _ =>
            {
                return new DbStringLocalizer<TResource>(_provider, _translatorFactory, _cache, _options, _localizationOptions, _loggerFactory);
            });
        }

        /// <summary>
        /// Create a new IStringLocalizer for the given type
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <returns></returns>
        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
                throw new NotImplementedException(nameof(resourceSource));

            return _localizerCache.GetOrAdd(resourceSource.FullName, _ =>
            {
                Type dbLocalizerType = typeof(DbStringLocalizer<>);
                Type dbLocalizer = dbLocalizerType.MakeGenericType(new[] { resourceSource });
                object dbLocalizerInstance = Activator.CreateInstance(dbLocalizer, _provider, _translatorFactory, _cache, _options, _localizationOptions, _loggerFactory);

                return (IStringLocalizer)dbLocalizerInstance;
            });
        }

        /// <summary>
        /// Create a new IStringLocalizer using the given type name
        /// </summary>
        /// <param name="baseName"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            var type = ResourceTypeHelper.GetResourceType(baseName, location);
            return Create(type);
        }
    }
}
