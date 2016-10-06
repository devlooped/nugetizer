using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace NuGet.Packaging.VisualStudio.ExtenderProviders
{
	public class LibraryProjectExtenderProvider : EnvDTE.IExtenderProvider, EnvDTE80.IInternalExtenderProvider, IDisposable
	{
		internal const string ExtenderName = nameof(LibraryProjectExtender);
		static bool inCanExtend = false;

		static ISet<string> CategoryIds => new HashSet<string>(new[]
		{
			PrjBrowseObjectCATID.prjCATIDCSharpProjectBrowseObject,
			PrjBrowseObjectCATID.prjCATIDVBProjectBrowseObject,
		}, StringComparer.OrdinalIgnoreCase);

		static bool IsSupportedCATID(string ExtenderCATID) => CategoryIds.Contains(ExtenderCATID);

		readonly ObjectExtenders extenders;
		readonly int[] cookies;

		public LibraryProjectExtenderProvider(ObjectExtenders extenders)
		{
			this.extenders = extenders;
			cookies = CategoryIds
				.Select(catId => this.extenders.RegisterExtenderProvider(catId, ExtenderName, this))
				.ToArray();
		}

		public void Dispose()
		{
			foreach (var cookie in cookies)
			{
				extenders.UnregisterExtenderProvider(cookie);
			}	
		}

		public bool CanExtend(string ExtenderCATID, string ExtenderName, object ExtendeeObject)
		{
			// check if provider can create extender for the given
			// ExtenderCATID, ExtenderName, and Extendee instance
			var extendeeCATIDProp = TypeDescriptor.GetProperties(ExtendeeObject)["ExtenderCATID"];

			//Recursion Guard: We need to check the "Build Action" property in HasContentBuildAction.
			//However, accessing this property will cause the Extender list to be built again, meaning 
			//that VS will call into our CanExtend method again. In this case, just return false.
			if (inCanExtend) return false;

			inCanExtend = true;
			var returnValue = ExtenderName.Equals(LibraryProjectExtenderProvider.ExtenderName)
				&& IsSupportedCATID(ExtenderCATID)
				&& extendeeCATIDProp != null
				&& IsSupportedCATID(extendeeCATIDProp.GetValue(ExtendeeObject).ToString());

			inCanExtend = false;

			return returnValue;
		}

		public object GetExtender(string ExtenderCATID,
								  string ExtenderName,
								  object ExtendeeObject,
								  IExtenderSite ExtenderSite,
								  int Cookie)
		{
			if (CanExtend(ExtenderCATID, ExtenderName, ExtendeeObject))
			{
				var browseObject = ExtendeeObject as IVsBrowseObject;
				if (browseObject != null)
				{
					IVsHierarchy hierarchy;
					uint itemId;
					int hr = browseObject.GetProjectItem(out hierarchy, out itemId);
					if (ErrorHandler.Succeeded(hr) && hierarchy != null)
					{
						return new LibraryProjectExtender(hierarchy, ExtenderSite, Cookie);
					}
				}
			}

			return null;
		}

		public object GetExtenderNames(string ExtenderCATID, object ExtendeeObject)
		{
			return new string[] { ExtenderName };
		}
	}
}
