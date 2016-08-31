using Clide;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NuGet.Packaging.VisualStudio
{
	class MoveProjectItemsToProjectVisitor : ISolutionVisitor
	{
		readonly Stack<ISolutionExplorerNode> stack;
		readonly List<ISolutionExplorerNode> nodesToBeDeleted;
		readonly Func<IProjectItemNode, bool> filterItem = x => false; // null filter

		public MoveProjectItemsToProjectVisitor(IProjectNode targetProject, Func<IProjectItemNode, bool> filterItem = null)
		{
			stack = new Stack<ISolutionExplorerNode>();
			stack.Push(targetProject);

			if (filterItem != null)
				this.filterItem = filterItem;

			nodesToBeDeleted = new List<ISolutionExplorerNode>();
		}

		public bool VisitEnter(IProjectNode project) => true;

		public bool VisitEnter(IFolderNode folder)
		{
			if (filterItem(folder))
				return false;

			var parent = stack.Peek();

			var targetFolder = parent
				.Nodes
				.FirstOrDefault(x => string.Equals(x.Name, folder.Name, StringComparison.OrdinalIgnoreCase));

			if (targetFolder == null)
				targetFolder = parent.As<IProjectItemContainerNode>().CreateFolder(folder.Name);

			stack.Push(targetFolder);

			return true;
		}

		public bool VisitEnter(IItemNode item)
		{
			if (filterItem(item))
				return false;

			var parent = stack.Peek();

			var targetItem = parent
				.Nodes
				.OfType<IItemNode>()
				.FirstOrDefault(x => string.Equals(x.Name, item.Name, StringComparison.OrdinalIgnoreCase));

			// Delete the existing item in the target project
			if (targetItem != null)
				targetItem.Delete();

			// And add the new one
			parent.As<IProjectItemContainerNode>().AddItem(item.PhysicalPath);

			return false;
		}

		public bool VisitEnter(IReferencesNode references) => false;

		public bool VisitEnter(IReferenceNode reference) => false;

		public bool VisitEnter(IGenericNode node) => true;

		public bool VisitLeave(IFolderNode folder)
		{
			if (filterItem(folder))
				return false;

			stack.Pop();
			nodesToBeDeleted.Add(folder);

			return true;
		}

		public bool VisitLeave(IReferencesNode references) => true;

		public bool VisitLeave(IGenericNode node) => true;

		public bool VisitLeave(IReferenceNode reference) => false;

		public bool VisitLeave(IItemNode item)
		{
			if (filterItem(item))
				return false;

			nodesToBeDeleted.Add(item);
			return false;
		}

		public bool VisitLeave(IProjectNode project)
		{
			// Once the project has been processed, delete all nodes
			// in order to avoid CollectionModified exception
			foreach (var node in nodesToBeDeleted)
			{
				var deletableNode = node.As<IDeletableNode>();
				if (deletableNode != null)
				{
					try
					{
						deletableNode.Delete();
					}
					catch
					{
						// Ignore? 
					}
				}
			}

			return false;
		}

		public bool VisitEnter(ISolutionItemNode solutionItem)
		{
			throw new NotSupportedException();
		}

		public bool VisitEnter(ISolutionFolderNode solutionFolder)
		{
			throw new NotSupportedException();
		}

		public bool VisitEnter(ISolutionNode solution)
		{
			throw new NotSupportedException();
		}

		public bool VisitLeave(ISolutionFolderNode solutionFolder)
		{
			throw new NotSupportedException();
		}

		public bool VisitLeave(ISolutionItemNode solutionItem)
		{
			throw new NotSupportedException();
		}

		public bool VisitLeave(ISolutionNode solution)
		{
			throw new NotSupportedException();
		}
	}
}