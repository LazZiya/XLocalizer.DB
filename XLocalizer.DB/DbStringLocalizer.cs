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
        private string _defaultCulture;
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
            _defaultCulture = localizationOptions.Value.DefaultRequestCulture.Culture.Name;
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
            var availableInTranslate = false;

            // Option 1: Look in the cache
            bool availableInCache = _cache.TryGetValue(name, out string value);

            if (!availableInCache)
            {
                var culture = CultureInfo.CurrentCulture.Name;

                // Option 2: Look in DB
                //bool availableInDb = TryGetValueFromDb(name, out value);
                bool availableInDb = _provider.TryGetValue<TResource>(name, out value);

                if (!availableInDb && _options.AutoTranslate)
                {

                    if (_defaultCulture != culture)
                    {
                        // Option 3: Online translate
                        availableInTranslate = _translator.TryTranslate(_defaultCulture, CultureInfo.CurrentCulture.Name, name, out value);
                    }
                }

                if (!availableInDb && _options.AutoAddKeys && _defaultCulture != culture)
                {
                    var res = new TResource
                    {
                        Key = name,
                        Value = value ?? name,
                        Comment = "Created by XLocalizer",
                        CultureID = CultureInfo.CurrentCulture.Name,
                        IsActive = false
                    };

                    // Save value to XML resource regardless the value has been translated or not
                    // If the value is not translated, the default "name" will be assigned to the "value"
                    // Anyhow, the saved values needs to be checked and confirmed one by one
                    bool savedToResource = _provider.TrySetValue<TResource>(res);
                    _logger.LogInformation($"Save resource to db, status: '{savedToResource}', key: '{name}', value: '{value ?? name}'");
                }

                if (availableInDb || availableInTranslate)
                {
                    // Save to cache
                    _cache.Set(name, value);

                    // Set availability to true
                    availableInCache = true;
                }
            }

            var val = string.Format(value ?? name, arguments);

            return new LocalizedString(name, val, resourceNotFound: !availableInCache, searchedLocation: typeof(TResource).FullName);
        }
    }
}