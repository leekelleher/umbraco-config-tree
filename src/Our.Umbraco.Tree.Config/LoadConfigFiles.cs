using System;
using System.Collections.Generic;
using System.Text;
using umbraco;
using umbraco.businesslogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;
using umbraco.IO;

namespace Our.Umbraco.Tree.Config
{
	/// <summary>
	/// Loads the config files into the tree.
	/// </summary>
	[Tree("developer", "configFiles", "Config Files")]
	public class LoadConfigFiles : FileSystemTree
	{
		public LoadConfigFiles(string application)
			: base(application)
		{
		}

		/// <summary>
		/// Gets the file path.
		/// </summary>
		/// <value>The file path.</value>
		protected override string FilePath
		{
			get
			{
				return SystemDirectories.Config;
			}
		}

		/// <summary>
		/// Gets the file search pattern.
		/// </summary>
		/// <value>The file search pattern.</value>
		protected override string FileSearchPattern
		{
			get
			{
				return "*.config";
			}
		}

		/// <summary>
		/// Renders the specified tree.
		/// </summary>
		/// <param name="tree">The application tree.</param>
		public override void Render(ref XmlTree tree)
		{
			base.Render(ref tree);

			// the NodeKey is empty for the root, but contains the folder name for sub-folders.
			if (string.IsNullOrEmpty(this.NodeKey))
			{
				// Add the Web.Config node
				var xNode = XmlTreeNode.Create(this);
				xNode.NodeID = "WebConfig";
				xNode.Action = "javascript:openConfigEditor('web.config');";
				xNode.Text = "Web.config";
				xNode.Icon = "../../developer/Config/config.gif";
				xNode.OpenIcon = xNode.Icon;
				tree.Add(xNode);
			}
		}

		/// <summary>
		/// Renders the JS.
		/// </summary>
		/// <param name="Javascript">The javascript.</param>
		public override void RenderJS(ref StringBuilder Javascript)
		{
			Javascript.Append(@"
				function openConfigEditor(id) { parent.right.document.location.href = 'developer/Config/editConfigFile.aspx?file=' + id; }
			");
		}

		/// <summary>
		/// Creates the root node.
		/// </summary>
		/// <param name="rootNode">The root node.</param>
		protected override void CreateRootNode(ref XmlTreeNode rootNode)
		{
			rootNode.Text = "Config Files";
			rootNode.Icon = FolderIcon;
			rootNode.OpenIcon = FolderIconOpen;
			rootNode.NodeID = "initConfigFiles";
			rootNode.NodeType = string.Concat(rootNode.NodeID, "_", this.TreeAlias);
			rootNode.Menu = new List<IAction>(new IAction[] { ActionRefresh.Instance });
		}

		/// <summary>
		/// Called when rendering folder node.
		/// </summary>
		/// <param name="xNode">The tree node.</param>
		protected override void OnRenderFolderNode(ref XmlTreeNode xNode)
		{
			xNode.Menu = new List<IAction>(new IAction[] { ActionRefresh.Instance });
			xNode.NodeType = "configFolder";
		}

		/// <summary>
		/// Called when rendering file node.
		/// </summary>
		/// <param name="xNode">The tree node.</param>
		protected override void OnRenderFileNode(ref XmlTreeNode xNode)
		{
			xNode.Action = xNode.Action.Replace("openFile", "openConfigEditor");
			xNode.Menu = new List<IAction>();
			xNode.Icon = "../../developer/Config/config.gif";
			xNode.OpenIcon = xNode.Icon;
		}
	}
}