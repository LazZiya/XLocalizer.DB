using System.Collections.Generic;

namespace XLocalizer.DB.Models
{
    /// <summary>
    /// XLocalizer default entity model
    /// </summary>
    public class XDbResource : IXDbResource
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The resource key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Comment if any...
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Localized Value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Culture Id, two letter ISO name
        /// </summary>
        public string CultureID { get; set; }

        /// <summary>
        /// Cultre
        /// </summary>
        public XDbCulture Culture { get; set; }

        /// <summary>
        /// Enable, disable
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Simplified version of the resource entity to be used inside list
    /// </summary>
    public class XDbResourceListItem
    {
        /// <summary>
        /// Resource ID, could be any resource id with the same Key and different culture
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Resource key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// List of available localized values
        /// </summary>
        public IEnumerable<string> Cultures { get; set; }
    }
}
