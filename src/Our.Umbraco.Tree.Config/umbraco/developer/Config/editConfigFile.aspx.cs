using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.uicontrols;

namespace Our.Umbraco.Tree.Config
{
	/// <summary>
	/// The Config Editor page class.
	/// </summary>
	public partial class EditConfigFile : UmbracoEnsuredPage
	{
		/// <summary>
		/// A string containing the path to the config folder.
		/// </summary>
		private const string CONFIGPATH = "/config/";

		/// <summary>
		/// A string containing the filename of the web.config file.
		/// </summary>
		private const string WEB_CONFIG = "web.config";

		/// <summary>
		/// Saves the contents of the config file to disk.
		/// </summary>
		/// <param name="filename">The filename of the config file.</param>
		/// <param name="oldName">The old filename of the config file.</param>
		/// <param name="contents">The contents of the config file.</param>
		/// <returns>Whether the save was successful.</returns>
		public bool SaveConfigFile(string filename, string oldFilename, string contents)
		{
			try
			{
				var oldFilePath = Server.MapPath(string.Concat(CONFIGPATH, oldFilename));
				string filePath;

				if (filename.Equals(WEB_CONFIG))
				{
					filePath = Server.MapPath(string.Concat("~/", filename));

					// make a back-up of the existing web.config - in case of emergency
					var unixTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString("F0");
					var backupFile = filePath.Replace(".config", string.Concat(".backup.", unixTime, ".config"));
					File.Copy(filePath, backupFile);
				}
				else
				{
					filePath = Server.MapPath(string.Concat(CONFIGPATH, filename));
				}

				using (var sw = File.CreateText(filePath))
				{
					sw.Write(contents);
				}

				// if the filename does not match the old filename - it definitely must NOT delete the web.config!
				if (filename != oldFilename && oldFilename != WEB_CONFIG)
				{
					// then delete the old file.
					if (File.Exists(oldFilePath))
						File.Delete(oldFilePath);
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Init"></see> event to initialize the page.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (this.UmbracoPanel1.hasMenu)
			{
				


                if (!CompatibilityHelper.IsVersion7OrNewer)
                {
                    // add the save button
                    var menuSave = this.UmbracoPanel1.Menu.NewImageButton();
                    menuSave.AlternateText = "Save File";
                    menuSave.ImageUrl = string.Concat(GlobalSettings.Path, "/images/editor/save.gif");
                    menuSave.Click += new ImageClickEventHandler(MenuSave_Click);
                    menuSave.CssClass = "btn btn-primary";
                    if (Request.QueryString["file"] == WEB_CONFIG)
                        menuSave.OnClientClick = "javascript:return confirm('You have modified the Web.config, are you sure that you still want to save?');";
                }
                else
                {
                    Button b = new Button();
                    b.CssClass = "btn btn-primary";
                    b.Click += b_Click;
                    b.Text = "Save";
                    b.ToolTip = "Save";
                    this.UmbracoPanel1.Menu.Controls[0].Controls.Add(b);

                    if (Request.QueryString["file"] == WEB_CONFIG)
                        b.OnClientClick = "javascript:return confirm('You have modified the Web.config, are you sure that you still want to save?');";

                }
            }
		}

        void b_Click(object sender, EventArgs e)
        {
            Save();
        }

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			var file = Request.QueryString["file"];
			string configFile;

			// special case for web.config
			if (file.Equals(WEB_CONFIG))
			{
				configFile = string.Concat("~/", file);

				// disable the filename text-box, so it can not be renamed
				this.txtName.Enabled = false;

				// show a warning message
				this.Feedback1.Text = "Warning: You are currently editing the Web.config file. Any modifications may potentially break your website. When you save a back-up copy will be made, in case of an emergency.";
				this.Feedback1.type = Feedback.feedbacktype.notice;
				this.Feedback1.Visible = true;
				this.Feedback1.Style.Add("height", "37px"); // HACK: The AutoResize for the CodeArea JavaScript goes whacky if the Feedback height isn't set! [LK]
			}
			else
			{
				configFile = string.Concat(CONFIGPATH, file);
			}

			this.txtName.Text = file;

			var appPath = Request.ApplicationPath;
			if (appPath == "/")
				appPath = string.Empty;

			this.ltrlPath.Text = string.Concat(appPath, configFile);

			if (!IsPostBack)
			{
				var openPath = Server.MapPath(configFile);

				if (File.Exists(openPath))
				{
					var fileContents = string.Empty;

					using (var sr = File.OpenText(openPath))
					{
						fileContents = sr.ReadToEnd();
					}

					if (!string.IsNullOrEmpty(fileContents))
						this.editorSource.Text = fileContents;
				}
				else
				{
					// display the error
					this.Feedback1.Text = string.Format("The file '{0}' does not exist.", file);
					this.Feedback1.type = Feedback.feedbacktype.error;
					this.Feedback1.Visible = true;

					// hide the editor menu and property panels
					this.UmbracoPanel1.hasMenu = false;
					this.PropertyPanel1.Visible = false;
					this.PropertyPanel2.Visible = false;
					this.PropertyPanel3.Visible = false;
				}
			}
		}

		/// <summary>
		/// Handles the Click event of the MenuSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
		private void MenuSave_Click(object sender, ImageClickEventArgs e)
		{
		    Save();
		}

	    private void Save()
	    {
            // validation?

            // save the file if there are no errors
            if (this.SaveConfigFile(this.txtName.Text, Request.QueryString["file"], this.editorSource.Text))
            {
                ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "fileSavedHeader"), ui.Text("speechBubbles", "fileSavedText"));
            }
            else
            {
                ClientTools.ShowSpeechBubble(speechBubbleIcon.error, ui.Text("speechBubbles", "fileErrorHeader"), ui.Text("speechBubbles", "fileErrorText"));
            }
	    }
	}
}
