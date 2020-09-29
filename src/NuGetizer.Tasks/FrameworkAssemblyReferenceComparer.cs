using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;

namespace NuGetizer.Tasks
{
    public class FrameworkAssemblyReferenceComparer : IEqualityComparer<FrameworkAssemblyReference>
    {
        public static FrameworkAssemblyReferenceComparer Default { get; } = new FrameworkAssemblyReferenceComparer();

        public bool Equals(FrameworkAssemblyReference x, FrameworkAssemblyReference y)
        {
            if (x == null && x == null)
                return true;
            if (x == null || y == null)
                return false;

            return StringComparer.OrdinalIgnoreCase.Equals(x.AssemblyName, y.AssemblyName)
                && StringComparer.OrdinalIgnoreCase.Equals(GetSupportedFrameworks(x), GetSupportedFrameworks(y));
        }

        public int GetHashCode(FrameworkAssemblyReference obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.AssemblyName ?? "")
                + StringComparer.OrdinalIgnoreCase.GetHashCode(GetSupportedFrameworks(obj));
        }

        static string GetSupportedFrameworks(FrameworkAssemblyReference obj)
        {
            if (obj.SupportedFrameworks == null)
                return "";

            return string.Join(",", obj.SupportedFrameworks.Select(framework => framework.ToString()));
        }
    }
}