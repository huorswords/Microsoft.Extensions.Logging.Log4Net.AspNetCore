namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;

    /// <summary>
    /// class containing extensions for IConfigurationSection interface.
    /// </summary>
    internal static class IConfigurationSectionExtensions
    {
        /// <summary>
        /// Converts IConfigurationSection to dictionary.
        /// </summary>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns>The dictionary</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="configurationSection"/> is null.</exception>
        public static IEnumerable<NodeInfo> ConvertToNodesInfo(this IConfigurationSection configurationSection) =>
            configurationSection.Get<IEnumerable<NodeInfo>>();
    }
}