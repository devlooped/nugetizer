using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NuGetizer;

public class IncludesResolver
{
    static readonly Regex IncludeRegex = new Regex(@"<!--\s?include\s(.*?)\s?-->", RegexOptions.Compiled);

    public static string Process(string filePath)
    {
        var content = File.ReadAllText(filePath).Trim();
        // Allow self-excluding files for processing. Could be useful if the file itself  
        // documents the include/exclude mechanism, for example.
        if (content.StartsWith("<!-- exclude -->") || content.EndsWith("<!-- exclude -->"))
            return content;

        var replacements = new Dictionary<Regex, string>();

        foreach (Match match in IncludeRegex.Matches(content))
        {
            var includedPath = match.Groups[1].Value.Trim();
            string fragment = default;
            if (includedPath.Contains("#"))
            {
                fragment = "#" + includedPath.Split('#')[1];
                includedPath = includedPath.Split('#')[0];
            }

            var includedFullPath = Path.Combine(Path.GetDirectoryName(filePath), includedPath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (File.Exists(includedFullPath))
            {
                // Resolve nested includes
                var includedContent = Process(includedFullPath);
                if (fragment != null)
                {
                    var anchor = $"<!-- {fragment} -->";
                    var start = includedContent.IndexOf(anchor);
                    if (start == -1)
                        // Warn/error?
                        continue;

                    includedContent = includedContent.Substring(start);
                    var end = includedContent.IndexOf(anchor, anchor.Length);
                    if (end != -1)
                        includedContent = includedContent.Substring(0, end + anchor.Length);
                }

                // see if we already have a section we previously replaced
                var existingRegex = new Regex(@$"<!--\s?include {includedPath}{fragment}\s?-->[\s\S]*<!-- {includedPath}{fragment} -->");
                var replacement = $"<!-- include {includedPath}{fragment} -->{Environment.NewLine}{includedContent}{Environment.NewLine}<!-- {includedPath}{fragment} -->";
                if (existingRegex.IsMatch(content))
                    replacements[existingRegex] = replacement;
                else
                    replacements[new Regex(@$"<!--\s?include {includedPath}{fragment}\s?-->")] = replacement;
            }
        }

        if (replacements.Count > 0)
        {
            var updated = content;
            foreach (var replacement in replacements)
                updated = replacement.Key.Replace(updated, replacement.Value);

            return updated.Trim();
        }

        return content;
    }
}
