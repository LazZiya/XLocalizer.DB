namespace XLocalizer.DB.Models
{
    /// <summary>
    /// Interface to implement XLocalizer resources
    /// </summary>
    public interface IXDbResource
    {
        /// <summary>
        /// ID
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// The resource key
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Value...
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Culture two letter name
        /// </summary>
        string CultureID { get; set; }

        /// <summary>
        /// Culture
        /// </summary>
        XDbCulture Culture { get; set; }

        /// <summary>
        /// Comment if any...
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Get or set a value to indicat if this resource item is active or not.
        /// Auto generated items are set to False by default.
        /// </summary>
        bool IsActive { get; set; }
    }
}
