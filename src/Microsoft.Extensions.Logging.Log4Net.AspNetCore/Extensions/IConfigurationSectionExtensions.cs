namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions
{
	using System;
	using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;

	/// <summary>
	/// class containing extensions for IConfigurationSection interface.
	/// </summary>
	[Obsolete("To be removed on next releases")]
	internal static class IConfigurationSectionExtensions
    {
        /// <summary>
        /// Converts IConfigurationSection to dictionary.
        /// </summary>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns>The dictionary</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="configurationSection"/> is null.</exception>
        [Obsolete("To be removed on next releases")]
		public static IEnumerable<NodeInfo> ToNodesInfo(this IConfigurationSection configurationSection) =>
            configurationSection.Get<IEnumerable<NodeInfo>>();
    }
}