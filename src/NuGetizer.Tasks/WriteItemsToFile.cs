using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NuGetizer.Tasks
{
    /// <summary>
    /// Writes items to an MSBuild document.
    /// </summary>
    public class WriteItemsToFile : Task
    {
        [Required]
        public ITaskItem[] Items { get; set; } = Array.Empty<ITaskItem>();

        public string ItemName { get; set; }

        [Required]
        public ITaskItem File { get; set; }

        public string IncludeMetadata { get; set; } = "true";

        public string UseFullPath { get; set; } = "false";

        public string Overwrite { get; set; } = "true";

        public override bool Execute()
        {
            var itemName = ItemName ?? "None";
            var includeMetadata = bool.Parse(IncludeMetadata);
            var useFullPath = bool.Parse(UseFullPath);
            var overwrite = bool.Parse(Overwrite);

            Func<ITaskItem, IEnumerable<XElement>> metadataFromItem;
            if (includeMetadata)
                metadataFromItem = item => item.CloneCustomMetadata()
                    .OfType<KeyValuePair<string, string>>()
                    .Select(entry => new XElement(entry.Key, entry.Value));
            else
                metadataFromItem = item => Enumerable.Empty<XElement>();

            XElement itemFromElement(ITaskItem item) => new(itemName,
                new XAttribute("Include", useFullPath ? item.GetMetadata("FullPath") : item.ItemSpec), metadataFromItem(item));

            var filePath = File.GetMetadata("FullPath");

            XDocument document;
            if (!overwrite && System.IO.File.Exists(filePath))
                document = XDocument.Load(filePath);
            else
                document = new XDocument(new XElement("Project"));

            document.Root.Add(
                new XElement("ItemGroup",
                    Items.Select(item => itemFromElement(item))));

            if (overwrite && System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            document.Save(filePath);

            return true;
        }
    }
}
