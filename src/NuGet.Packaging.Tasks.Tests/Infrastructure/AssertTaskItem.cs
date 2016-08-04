using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public class AssertTaskItem : IEnumerable<ITaskItem>
    {
        private static readonly Action<IEnumerable<ITaskItem>> NoAssert = x => { };

        private IEnumerable<ITaskItem> taskItems;
        private StringComparer stringComparer;
        private Action<IEnumerable<ITaskItem>> assertInvariant;

        public AssertTaskItem(
            IEnumerable<ITaskItem> taskItems,
            string itemSpec,
            Action<IEnumerable<ITaskItem>> assertInvariant = null,
            StringComparer stringComparer = null)
        {
            this.assertInvariant = assertInvariant ?? NoAssert;
            this.stringComparer = stringComparer ?? StringComparer.OrdinalIgnoreCase;
            this.taskItems = taskItems.Where(x => this.stringComparer.Equals(x.ItemSpec, itemSpec));
        }

        public void Add(string metatadaName, string metadataValue)
        {
            this.Add(metatadaName, metadataValue, NoAssert);
        }

        public void Add(string metatadaName, string metadataValue, Action<IEnumerable<ITaskItem>> assert)
        {
            try
            {
                this.taskItems = this.taskItems.Where(x => this.stringComparer.Equals(x.GetMetadata(metatadaName), metadataValue));
                this.assertInvariant(this.taskItems);
                assert(this.taskItems);
            }
            catch (Exception ex)
            {
                throw new MetadataException(
                    string.Format("Invalid metadata state '{0}' '{1}'", metatadaName, metadataValue),
                    ex);
            }
        }

        public IEnumerator<ITaskItem> GetEnumerator()
        {
            return this.taskItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public class MetadataException : Xunit.Sdk.XunitException
        {
            public MetadataException(string message, Exception innerException) :
                base(message, innerException)
            {
            }
        }
    }
}