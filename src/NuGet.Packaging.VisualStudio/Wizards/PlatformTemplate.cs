using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	internal class PlatformTemplate
	{
		static readonly Regex NameRegex = new Regex(@"\$PlatformTemplate\.(?<Suffix>[^\$]*)");
		static readonly Regex ValueRegex = new Regex(@"(?<TemplateId>[^\|]*)\|(?<DisplayName>.*)");

		public string Suffix { get; set; }
		public string TemplateId { get; set; }
		public string DisplayName { get; set; }

		public static bool TryParse(string name, string value, out PlatformTemplate parameter)
		{
			parameter = default(PlatformTemplate);

			if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
			{
				var nameMatch = NameRegex.Match(name);
				var valueMatch = ValueRegex.Match(value);

				if (nameMatch.Success && valueMatch.Success)
					parameter = new PlatformTemplate
					{
						Suffix = nameMatch.Groups["Suffix"].Value,
						TemplateId = valueMatch.Groups["TemplateId"].Value,
						DisplayName = valueMatch.Groups["DisplayName"].Value,
					};
			}

			return parameter != null;
		}
	}
}