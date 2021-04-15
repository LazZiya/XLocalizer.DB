using XLocalizer.Common;
using XLocalizer.DB.Models;
using XLocalizer.Translate;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;

namespace XLocalizer.DB
{
    /// <summary>
    /// DbStringLocalizer
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class DbStringLocalizer<TResource> : IStringLocalizer, IStringLocalizer<TResource>
        where TResource : class, IXDbResource, new()
    {
        private readonly IDbResourceProvider _provider;
        private readonly ITranslator _translator;
        private readonly ExpressMemoryCache _cache;
        private readonly XLocalizerOptions _options;
        private string _transCulture;
        private readonly ILogger _logger;

        /// <summary>
        /// Initialize a new instance of DbStringLocalizer
        /// </summary>
        /// <param name="options"></param>
        /// <param name="provider"></param>
        /// <param name="translatorFactory"></param>
        /// <param name="cache"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="localizationOptions"></param>
        public DbStringLocalizer(IDbResourceProvider provider,
                                 ITranslatorFactory translatorFactory,
                                 ExpressMemoryCache cache,
                                 IOptions<XLocalizerOptions> options,
                                 IOptions<RequestLocalizationOptions> localizationOptions,
                                 ILoggerFactory loggerFactory)
        {
            _options = options.Value;
            _provider = provider;
            _translator = translatorFactory.Create();
            _cache = cache;
            _logger = loggerFactory.CreateLogger<DbStringLocalizer<TResource>>();
            _transCulture = options.Value.TranslateFromCulture ?? localizationOptions.Value.DefaultRequestCulture.Culture.Name;
        }

        /// <summary>
        /// Get localized string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LocalizedString this[string name] => GetLocalizedString(name);

        /// <summary>
        /// Get localized string with arguments
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public LocalizedString this[string name, params object[] arguments] => GetLocalizedString(name, arguments);

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <param name="includeParentCultures"></param>
        /// <returns></returns>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT IMPLEMENTED! use <see cref="CultureSwitcher"/> instead.
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private LocalizedString GetLocalizedString(string name, params object[] arguments)
        {
            var _targetCulture = CultureInfo.CurrentCulture.Name;
            var _targetEqualSource = _transCulture.Equals(_targetCulture, StringComparison.OrdinalIgnoreCase);

            // Option 0: Skip localization if:
            // LocalizeDefaultCulture == false and currentCulture == _transCulture
            if (!_options.LocalizeDefaultCulture && _targetEqualSource)
            {
                return new LocalizedString(name, name, resourceNotFound: true, searchedLocation: string.Empty);
            }

            // Option 1: Look in the cache
            bool availableInCache = _cache.TryGetValue<TResource>(name, out string value);
            if (availableInCache)
            {
                return new LocalizedString(name, string.Format(value, arguments), false, string.Empty);
            }

            // Option 2: Look in db source
            bool availableInSource = _provider.TryGetValue<TResource>(name, out value);
            if (availableInSource)
            {
                // Add to cache
                _cache.Set<TResource>(name, value);

                return new LocalizedString(name, string.Format(value, arguments), false, string.Empty);
            }

            // Option 3: Try online translation service
            //           and don't do online translation if target == source culture,
            //           because online tranlsation services requires two different cultures.
            var availableInTranslate = false;
            if (_options.AutoTranslate && !_targetEqualSource)
            {
                availableInTranslate = _translator.TryTranslate(_transCulture, CultureInfo.CurrentCulture.Name, name, out value);
                if (availableInTranslate)
                {
                    // Add to cache
                    _cache.Set<TResource>(name, value);
                }
            }

            // Save to db when AutoAdd is anebled and:
            // A:translation success or, B: AutoTranslate is off or, C: Target and source cultures are same
            // option C: useful when we use code keys to localize defatul culture as well
            if (_options.AutoAddKeys && (availableInTranslate || !_options.AutoTranslate || _targetEqualSource))
            {
                var res = new TResource
                {
                    Key = name,
                    Value = value ?? name,
                    Comment = "Created by XLocalizer",
                    CultureID = CultureInfo.CurrentCulture.Name,
                    IsActive = false
                };

                bool savedToResource = _provider.TrySetValue<TResource>(res);
                _logger.LogInformation($"Save resource to db, status: '{savedToResource}', key: '{name}', value: '{value ?? name}'");
            }

            return new LocalizedString(name, string.Format(value ?? name, arguments), !availableInTranslate, typeof(TResource).FullName);
        }
    }
}