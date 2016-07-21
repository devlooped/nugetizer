
using System.Collections.Generic;
using System.Text;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    class MSBuildArgumentBuilder
    {
        StringBuilder builder = new StringBuilder();

        static readonly Dictionary<string, string> defaultProperties;

        static MSBuildArgumentBuilder()
        {
            defaultProperties = new Dictionary<string, string>();
            defaultProperties.Add("NuGetTargetsPath", Assets.NuGetPackagingTargetsPath);
            defaultProperties.Add("NuGetTasksPath", Assets.NuGetPackagingTasksPath);
            defaultProperties.Add("NuGetToolPath", Assets.NuGetToolPath);
            defaultProperties.Add("BuildingInsideVisualStudio", "true");
        }

        public MSBuildArgumentBuilder(string fileName)
        {
            builder.AppendFormat("\"{0}\" ", fileName);
        }

        public void AppendProperty(string name, string value)
        {
            builder.AppendFormat("/p:{0}={1} ", name, value);
        }

        public void Append(string item)
        {
            builder.Append(item);
            builder.Append(' ');
        }

        public void Build()
        {
            foreach (var property in defaultProperties)
            {
                AppendProperty(property.Key, property.Value);
            }
        }

        public override string ToString ()
        {
            return builder.ToString().TrimEnd();
        }
    }
}

