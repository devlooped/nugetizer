using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Build.Packaging
{
	public static class TargetFrameworks
	{
		public const string NET45 = ".NETFramework,Version=v4.5";
		public const string NET46 = ".NETFramework,Version=v4.6";
		public const string PCL78 = ".NETPortable,Version=v4.0,Profile=Profile78";
		public const string XI10 = "Xamarin.iOS,Version=v1.0";
		public const string XA50 = "MonoAndroid,Version=v5.0";
	}
}
