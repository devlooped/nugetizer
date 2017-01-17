using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSLangProj;
using VSLangProj80;

namespace NuGet.Packaging.VisualStudio
{
	class ProjectEventsService : VSProjectEvents2
	{
		Lazy<VSProject> project;

		public ProjectEventsService(Lazy<VSProject> project)
		{
			this.project = project;
		}

		public ReferencesEvents ReferencesEvents => project.Value.Events.ReferencesEvents;

		public BuildManagerEvents BuildManagerEvents => project.Value.Events.BuildManagerEvents;

		public ImportsEvents ImportsEvents => project.Value.Events.ImportsEvents;

		public VSLangProjWebReferencesEvents VSLangProjWebReferencesEvents => null;
	}
}
