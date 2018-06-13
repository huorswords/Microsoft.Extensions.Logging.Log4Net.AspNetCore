using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore
{

	/// <summary>
	/// class containing extensions for IConfigurationSection interface.
	/// </summary>
	public static class IConfigurationSectionExtensions
	{

		private const string HierarchySeparatorChar = ":";

		/// <summary>
		/// Converts IConfigurationSection to dictionary.
		/// </summary>
		/// <param name="configurationSection">The configuration section.</param>
		/// <returns>The dictionary</returns>
		/// <exception cref="ArgumentNullException">configurationSection</exception>
		public static IDictionary<string, string> ConvertToDictionary(this IConfigurationSection configurationSection)
		{
			if (configurationSection == null)
			{
				throw new ArgumentNullException(nameof(configurationSection));
			}

			var configs = configurationSection.AsEnumerable().Skip(1);
			return configs.ToDictionary((k) => k.Key.Substring(k.Key.LastIndexOf(HierarchySeparatorChar) + 1), (v) => v.Value);
		}



	}
}
