using XLocalizer.DB.Models;

namespace XLocalizer.DB
{
    /// <summary>
    /// Interface to create custom localization providers depending on any db type.
    /// </summary>
    public interface IDbResourceProvider
    {
        /// <summary>
        /// Get a localized string from DB
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue<TResource>(string key, out string value)
            where TResource : class, IXDbResource;

        /// <summary>
        /// Try set localized resource to db
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        bool TrySetValue<TResource>(TResource resource)
            where TResource : class, IXDbResource;
    }
}
