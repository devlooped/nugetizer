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
	public class ProjectReferenceExtenderProvider : EnvDTE.IExtenderProvider, EnvDTE80.IInternalExtenderProvider, IDisposable
	{
		internal const string ExtenderName = nameof(ProjectReferenceExtender);
		static bool inCanExtend = false;

		public static ISet<string> CategoryIds => new HashSet<string>(new[]
		{
			PrjBrowseObjectCATID.prjCATIDCSharpReferenceBrowseObject,
			PrjBrowseObjectCATID.prjCATIDVBReferenceBrowseObject,
		}, StringComparer.OrdinalIgnoreCase);

		static bool IsSupportedCATID(string ExtenderCATID) => CategoryIds.Contains(ExtenderCATID);

		readonly ObjectExtenders extenders;
		readonly int[] cookies;

		public ProjectReferenceExtenderProvider(ObjectExtenders extenders)
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
			PropertyDescriptor extendeeCATIDProp = TypeDescriptor.GetProperties(ExtendeeObject)["ExtenderCATID"];
			PropertyDescriptor extendeeSourceProjectProp = TypeDescriptor.GetProperties(ExtendeeObject)["SourceProject"];

			//Recursion Guard: We need to check the "Build Action" property in HasContentBuildAction.
			//However, accessing this property will cause the Extender list to be built again, meaning 
			//that VS will call into our CanExtend method again. In this case, just return false.
			if (inCanExtend) return false;

			inCanExtend = true;
			bool returnValue = ExtenderName.Equals(ProjectReferenceExtenderProvider.ExtenderName)
				&& IsSupportedCATID(ExtenderCATID)
				&& extendeeCATIDProp != null
				&& extendeeSourceProjectProp != null
				&& IsSupportedCATID(extendeeCATIDProp.GetValue(ExtendeeObject).ToString())
				&& extendeeSourceProjectProp.GetValue(ExtendeeObject) != null;

			inCanExtend = false;

			return returnValue;
		}

		public object GetExtender(string ExtenderCATID,
								  string ExtenderName,
								  object ExtendeeObject,
								  IExtenderSite ExtenderSite,
								  int Cookie)
		{
			ProjectReferenceExtender extender = null;
			if (CanExtend(ExtenderCATID, ExtenderName, ExtendeeObject))
			{
				IVsBrowseObject browseObject = ExtendeeObject as IVsBrowseObject;
				if (browseObject != null)
				{
					IVsHierarchy hierarchyItem = null;
					uint itemId = VSConstants.VSITEMID_NIL;

					//get the hierarchy item (the item id will be wrong if the reference is not shown)
					browseObject.GetProjectItem(out hierarchyItem, out itemId);

					extender = new ProjectReferenceExtender(hierarchyItem, itemId, ExtenderSite, Cookie);
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
