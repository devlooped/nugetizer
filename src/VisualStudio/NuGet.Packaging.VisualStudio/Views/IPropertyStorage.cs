using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace NuGet.Packaging.VisualStudio
{
	interface IPropertyStorage
	{
		T GetPropertyValue<T>([CallerMemberName] string memberName = "");
		void SetPropertyValue<T>(T value, [CallerMemberName] string memberName = "");
	}
}
