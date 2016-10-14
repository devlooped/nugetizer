using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	class InMemoryPropertyStorage : IPropertyStorage
	{
		Dictionary<string, object> values = new Dictionary<string, object>();

		public T GetPropertyValue<T>([CallerMemberName] string memberName = "") =>
			(T)values[memberName];

		public void SetPropertyValue<T>(T value, [CallerMemberName] string memberName = "") =>
			values[memberName] = value;
	}
}
