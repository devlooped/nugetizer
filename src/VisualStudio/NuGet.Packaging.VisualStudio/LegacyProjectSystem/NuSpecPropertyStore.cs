using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyPageBase;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuGet.Packaging.VisualStudio
{
	class NuSpecPropertyStore : PropertyPageBase.IPropertyStore
	{
		IVsBuildPropertyStorage buildStorage;

		public event StoreChangedDelegate StoreChanged;

		/// <summary>
		/// Use the data passed in to initialize ourselve
		/// </summary>
		/// <param name="dataObject">This is normally our configuration object or null when we should release it</param>
		public void Initialize(object[] dataObjects)
		{
			// If we are editing multiple configuration at once, we may get multiple objects
			foreach (object dataObject in dataObjects)
			{
				var browseObject = dataObject as IVsBrowseObject;

				if (browseObject != null)
				{
					IVsHierarchy hierarchy;
					uint item;

					browseObject.GetProjectItem(out hierarchy, out item);
					buildStorage = (IVsBuildPropertyStorage)hierarchy;
				}
			}
		}

		/// <summary>
		/// Set the value of the specified property in storage
		/// </summary>
		/// <param name="propertyName">Name of the property to set</param>
		/// <param name="propertyValue">Value to set the property to</param>
		public void Persist(string propertyName, string propertyValue)
		{
			if (buildStorage != null)
			{
				// We don't want to save null, so make it empty
				if (propertyValue == null)
					propertyValue = String.Empty;

				buildStorage.SetPropertyValue(propertyName, null, (uint)_PersistStorageType.PST_PROJECT_FILE, propertyValue);

				StoreChanged?.Invoke();
			}
		}

		/// <summary>
		/// Retreive the value of the specified property from storage
		/// </summary>
		/// <param name="propertyName">Name of the property to retrieve</param>
		/// <returns></returns>
		public string PropertyValue(string propertyName)
		{
			string value = null;

			if (buildStorage != null)
				buildStorage.GetPropertyValue(propertyName, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out value);

			return value;
		}

		public void Dispose()
		{
		}
	}
}
