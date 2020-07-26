using XLocalizer.DB.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;

namespace XLocalizer.DB
{
    /// <summary>
    /// Database html localizer factory
    /// </summary>
    public class DbHtmlLocalizerFactory<TResource> : IXHtmlLocalizerFactory
        where TResource : class, IXDbResource
    {
        private readonly IStringLocalizerFactory _factory;

        /// <summary>
        /// Initialize a new instance of DbHtmlLocalizerFactory
        /// </summary>
        /// <param name="factory"></param>
        public DbHtmlLocalizerFactory(IStringLocalizerFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Create a new instance of DbHtmlLocalizer
        /// </summary>
        /// <returns></returns>
        public IHtmlLocalizer Create()
        {
            return new DbHtmlLocalizer<TResource>(_factory);
        }

        /// <summary>
        /// Create a new instance of DbHtmlLocalizer
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <returns></returns>
        public IHtmlLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
                throw new NotImplementedException(nameof(resourceSource));

            var localizer = _factory.Create(resourceSource);
            return new DbHtmlLocalizer(localizer);
        }

        /// <summary>
        /// Create a new instance of DbHtmlLocalizer
        /// </summary>
        /// <param name="baseName"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public IHtmlLocalizer Create(string baseName, string location)
        {
            if (string.IsNullOrEmpty(baseName))
                throw new NotImplementedException(nameof(baseName));

            if (string.IsNullOrEmpty(location))
                throw new NotImplementedException(nameof(location));

            var localizer = _factory.Create(baseName, location);
            return new DbHtmlLocalizer(localizer);
        }
    }
}
