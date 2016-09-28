using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;

namespace NuGet.Build.Packaging
{
	public class FrameworkAssemblyReferenceComparer : IEqualityComparer<FrameworkAssemblyReference>
    {
		public static FrameworkAssemblyReferenceComparer Default { get; private set; }

		static FrameworkAssemblyReferenceComparer()
		{
			Default = new FrameworkAssemblyReferenceComparer(StringComparer.OrdinalIgnoreCase);
		}

        StringComparer comparer;

        private FrameworkAssemblyReferenceComparer(StringComparer comparer)
        {
            this.comparer = comparer;
        }

        public bool Equals(FrameworkAssemblyReference x, FrameworkAssemblyReference y)
        {
            if (x == null && x == null)
                return true;

            if (x == null || y == null)
                return false;

            return comparer.Equals(x.AssemblyName, y.AssemblyName)
                && comparer.Equals(GetSupportedFrameworks(x), GetSupportedFrameworks(y));
        }

        public int GetHashCode(FrameworkAssemblyReference obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return comparer.GetHashCode(obj.AssemblyName ?? "")
                + comparer.GetHashCode(GetSupportedFrameworks(obj));
        }

        static string GetSupportedFrameworks(FrameworkAssemblyReference obj)
        {
            if (obj.SupportedFrameworks == null)
                return "";

            return string.Join(",", obj.SupportedFrameworks.Select(framework => framework.ToString()));
        }
    }
}