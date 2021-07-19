using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Minimatch;

namespace NuGetizer.Tasks
{
    /// <summary>
    /// Evaluates one or more minimatch expressions against a set of 
    /// items and returns two lists: those that matched and those that 
    /// didn't.
    /// </summary>
    public class EvaluateWildcards : Task
    {
        [Required]
        public ITaskItem[] Items { get; set; }

        [Required]
        public string Wildcards { get; set; }

        [Output]
        public ITaskItem[] MatchingItems { get; set; }

        [Output]
        public ITaskItem[] NonMatchingItems { get; set; }

        public override bool Execute()
        {
            var matching = new List<ITaskItem>();
            var nonMatching = new List<ITaskItem>();

            var options = new Options
            {
                AllowWindowsPaths = true,
                Dot = true,
                IgnoreCase = true
            };

            var matchers = Wildcards
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(wildcard => new Minimatcher(wildcard.Trim(), options))
                .ToList();

            foreach (var item in Items)
            {
                if (matchers.Any(matcher => matcher.IsMatch(item.ItemSpec)) ||
                    matchers.Any(matcher => matcher.IsMatch(item.GetMetadata("Fullpath"))))
                    matching.Add(item);
                else
                    nonMatching.Add(item);
            }

            MatchingItems = matching.ToArray();
            NonMatchingItems = nonMatching.ToArray();

            return true;
        }
    }
}
