using XLocalizer.Common;
using XLocalizer.DB.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace XLocalizer.DB
{
    /// <summary>
    /// Generic DBHtmlLocalizer
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class DbHtmlLocalizer<TResource> : DbHtmlLocalizer, IHtmlLocalizer<TResource>
        where TResource : class, IXDbResource
    {
        /// <summary>
        /// Initialze a new <see cref="DbHtmlLocalizer{TResource}"/> based on speicifed resource type
        /// </summary>
        /// <param name="factory"></param>
        public DbHtmlLocalizer(IStringLocalizerFactory factory)
            : base(factory.Create(typeof(TResource)))
        {

        }
    }

    /// <summary>
    /// DBHtmlLocalizer based on default type defined in startup
    /// </summary>
    public class DbHtmlLocalizer : IHtmlLocalizer
    {
        private readonly IStringLocalizer _localizer;

        /// <summary>
        /// Initialize a new instance of DbHtmlLocalizer
        /// </summary>
        /// <param name="localizer"></param>
        public DbHtmlLocalizer(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        /// <summary>
        /// Get localized html string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LocalizedHtmlString this[string name] => GetLocalizedHtmlString(name);

        /// <summary>
        /// Get localized html string with arguments
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public LocalizedHtmlString this[string name, params object[] arguments] => GetLocalizedHtmlString(name, arguments);

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
        /// Get localized string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LocalizedString GetString(string name)
        {
            return _localizer[name];
        }

        /// <summary>
        /// Get localized string with arguments
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public LocalizedString GetString(string name, params object[] arguments)
        {
            return _localizer[name, arguments];
        }

        /// <summary>
        /// NOT IMPLEMENTED! use <see cref="CultureSwitcher"/> instead.
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private LocalizedHtmlString GetLocalizedHtmlString(string name, params object[] arguments)
        {
            var val = _localizer[name, arguments];

            return new LocalizedHtmlString(name, val, val.ResourceNotFound);
        }
    }
}
