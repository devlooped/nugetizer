using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuGet.Packaging.VisualStudio
{
	class BuildPropertyStorage : IPropertyStorage
	{
		readonly IVsBuildPropertyStorage vsBuildPropertyStorage;
		readonly Dictionary<string, object> pendingValuesToBePersisted = new Dictionary<string, object>();
		readonly bool commitChangesImmediately;

		public BuildPropertyStorage(IVsBuildPropertyStorage vsBuildPropertyStorage, bool commitChangesImmediately = false)
		{
			this.vsBuildPropertyStorage = vsBuildPropertyStorage;
			this.commitChangesImmediately = commitChangesImmediately;
		}

		public T GetPropertyValue<T>([CallerMemberName] string memberName = "")
		{
			object value = null;
			if (pendingValuesToBePersisted.TryGetValue(memberName, out value))
				return (T)value;

			string valueAsString;
			if (ErrorHandler.Succeeded(
				vsBuildPropertyStorage.GetPropertyValue(
					memberName,
					null,
					(uint)_PersistStorageType.PST_PROJECT_FILE,
					out valueAsString)))
			{
				if (typeof(T) != typeof(string))
					value = TypeDescriptor
						.GetConverter(typeof(T))
						.ConvertFromString(valueAsString);
				else
					value = valueAsString;
			}

			return value != null ? (T)value : default(T);
		}

		public void SetPropertyValue<T>(T value, [CallerMemberName] string memberName = "")
		{
			pendingValuesToBePersisted[memberName] = value;

			if (commitChangesImmediately)
				CommitChanges();
		}

		public void CommitChanges()
		{
			foreach (var kvp in pendingValuesToBePersisted)
				vsBuildPropertyStorage.SetPropertyValue(
					kvp.Key,
					null,
					(uint)_PersistStorageType.PST_PROJECT_FILE,
					kvp.Value.ToString());

			pendingValuesToBePersisted.Clear();
		}
	}
}