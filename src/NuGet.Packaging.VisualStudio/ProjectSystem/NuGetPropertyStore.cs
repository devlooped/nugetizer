using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyPageBase;

namespace NuGet.Packaging.VisualStudio
{
	class NuGetPropertyStore : PropertyPageBase.IPropertyStore
	{
		public event StoreChangedDelegate StoreChanged;

		public void Dispose()
		{
		}

		public void Initialize(object[] dataObject)
		{
		}

		public void Persist(string propertyName, string propertyValue)
		{
		}

		public string PropertyValue(string propertyName)
		{
			return string.Empty;
		}
	}
}
