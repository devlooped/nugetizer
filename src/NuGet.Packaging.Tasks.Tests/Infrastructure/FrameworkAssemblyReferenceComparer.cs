using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public class FrameworkAssemblyReferenceComparer : IEqualityComparer<FrameworkAssemblyReference>
    {
        private static FrameworkAssemblyReferenceComparer _instance = new FrameworkAssemblyReferenceComparer(StringComparer.OrdinalIgnoreCase);

        private StringComparer _stringComparer;

        public FrameworkAssemblyReferenceComparer(StringComparer stringComparer)
        {
            if (stringComparer == null)
            {
                throw new ArgumentNullException("stringComparer");
            }

            _stringComparer = stringComparer;
        }

        public static FrameworkAssemblyReferenceComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(FrameworkAssemblyReference x, FrameworkAssemblyReference y)
        {
            if (x == null && x == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return _stringComparer.Equals(x.AssemblyName, y.AssemblyName)
                && _stringComparer.Equals(GetSupportedFrameworks(x), GetSupportedFrameworks(y));
        }

        public int GetHashCode(FrameworkAssemblyReference obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return _stringComparer.GetHashCode(obj.AssemblyName ?? "")
                + _stringComparer.GetHashCode(GetSupportedFrameworks(obj));
        }

        static string GetSupportedFrameworks(FrameworkAssemblyReference obj)
        {
            if (obj.SupportedFrameworks == null)
            {
                return "";
            }

            return string.Join(",", obj.SupportedFrameworks.Select(framework => framework.ToString()));
        }
    }
}