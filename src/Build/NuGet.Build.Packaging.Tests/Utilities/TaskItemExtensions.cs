using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace NuGet.Build.Packaging
{
	public static class TaskItemExtensions
	{
		/// <summary>
		/// Checks if the given item has metadata key/values matching the 
		/// anonymous object property/values.
		/// </summary>
		public static bool Matches(this ITaskItem item, object metadata)
		{
			foreach (var prop in metadata.GetType().GetProperties())
			{
				var actual = item.GetMetadata(prop.Name);
				var expected = prop.GetValue(metadata).ToString();

				if (actual != expected)
					return false;
			}

			return true;
		}
	}
}
