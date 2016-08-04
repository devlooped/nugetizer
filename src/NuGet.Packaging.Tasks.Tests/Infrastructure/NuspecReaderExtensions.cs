
using System.Linq;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public static class NuspecReaderExtensions
    {
        public static string GetMetadataValue(this NuspecReader reader, string name)
        {
            return reader.GetMetadata().First(item => item.Key == name).Value;
        }
    }
}

