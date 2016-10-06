using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuGet.Packaging.VisualStudio.ExtenderProviders
{
	public class NoneItemExtenderProvider : EnvDTE.IExtenderProvider, EnvDTE80.IInternalExtenderProvider, IDisposable
	{
		internal const string ExtenderName = nameof(NoneItemExtender);
		static bool inCanExtend = false;

		static ISet<string> CategoryIds => new HashSet<string>(new[]
		{
			VSConstants.CATID.CSharpFileProperties_string,
			VSConstants.CATID.VBFileProperties_string,
		}, StringComparer.OrdinalIgnoreCase);

		static bool IsSupportedCATID(string ExtenderCATID) => CategoryIds.Contains(ExtenderCATID);

		readonly ObjectExtenders extenders;
		readonly int[] cookies;

		public NoneItemExtenderProvider(ObjectExtenders extenders)
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
			bool returnValue = ExtenderName.Equals(NoneItemExtenderProvider.ExtenderName)
				&& IsSupportedCATID(ExtenderCATID)
				&& extendeeCATIDProp != null
				&& IsSupportedCATID(extendeeCATIDProp.GetValue(ExtendeeObject).ToString());

			inCanExtend = false;

			return returnValue;
		}

		public object GetExtender(string ExtenderCATID,
								  string ExtenderName,
								  object ExtendeeObject,
								  EnvDTE.IExtenderSite ExtenderSite,
								  int Cookie)
		{
			NoneItemExtender extender = null;
			if (CanExtend(ExtenderCATID, ExtenderName, ExtendeeObject))
			{
				var browseObject = ExtendeeObject as IVsBrowseObject;
				if (browseObject != null)
				{
					IVsHierarchy hierarchyItem;
					uint itemId;
					browseObject.GetProjectItem(out hierarchyItem, out itemId);
					extender = new NoneItemExtender(hierarchyItem, itemId, ExtenderSite, Cookie);
				}
			}
			return extender;
		}

		public object GetExtenderNames(string ExtenderCATID, object ExtendeeObject)
		{
			return new string[] { ExtenderName };
		}
	}
}
