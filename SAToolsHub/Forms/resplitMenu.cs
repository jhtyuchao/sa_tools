﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml.Serialization;
using SAModel.SAEditorCommon.ModManagement;
using SAEditorCommon.ProjectManagement;
using SplitTools.SAArc;

namespace SAToolsHub
{
	public partial class resplitMenu : Form
	{
		SAModel.SAEditorCommon.UI.ProgressDialog splitProgress;
		bool overwrite = false;
		Templates.SplitTemplate template;
		List<Templates.SplitEntry> splitEntries = new List<Templates.SplitEntry>();
		List<Templates.SplitEntryMDL> splitMDLEntries = new List<Templates.SplitEntryMDL>();

		public resplitMenu()
		{
			InitializeComponent();
			backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
			backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted);
		}

		#region General Functions
		#endregion

		#region Form Functions
		private void resplitMenu_Shown(object sender, EventArgs e)
		{
			template = ProjectFunctions.openTemplateFile(SAToolsHub.GetTemplate());
			checkedListBox1.Items.Clear();

			foreach (Templates.SplitEntry splitEntry in template.SplitEntries)
			{
				checkedListBox1.Items.Add(splitEntry);
				if (splitEntry.CmnName != null)
					checkedListBox1.DisplayMember = "CmnName";
				else
					checkedListBox1.DisplayMember = "IniFile";
			}

			foreach (Templates.SplitEntryMDL mdlEntry in template.SplitMDLEntries)
			{
				checkedListBox1.Items.Add(mdlEntry);
				string mdlFile = Path.GetFileNameWithoutExtension(mdlEntry.ModelFile);
				checkedListBox1.DisplayMember = mdlFile;
			}
		}

		private void chkAll_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < checkedListBox1.Items.Count; i++)
			{
				checkedListBox1.SetItemChecked(i, true);
			}
		}

		private void unchkAll_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < checkedListBox1.Items.Count; i++)
			{
				checkedListBox1.SetItemChecked(i, false);
			}
		}

		private void btnSplit_Click(object sender, EventArgs e)
		{
			foreach (Object item in checkedListBox1.CheckedItems)
			{
				if (item.GetType() == typeof(Templates.SplitEntry))
					splitEntries.Add((Templates.SplitEntry)item);
				else
					splitMDLEntries.Add((Templates.SplitEntryMDL)item);
			}

#if !DEBUG
			backgroundWorker1.RunWorkerAsync();
#endif
#if DEBUG
			backgroundWorker1_DoWork(null, null);
			BackgroundWorker1_RunWorkerCompleted(null, null);
#endif
		}

		private void chkOverwrite_CheckedChanged(object sender, EventArgs e)
		{
			if (chkOverwrite.Checked)
				overwrite = true;
			else
				overwrite = false;
		}
		#endregion

		void splitGame(string game, SAModel.SAEditorCommon.UI.ProgressDialog progress)
		{
			string appPath = Path.GetDirectoryName(Application.ExecutablePath);
			string dataFolder = template.GameInfo.DataFolder;
			string gamePath = SAToolsHub.gameDirectory;
			string projFolder = SAToolsHub.projectDirectory;
			string iniFolder;

			progress.SetMaxSteps(splitEntries.Count + splitMDLEntries.Count + 1);

			if (Directory.Exists(Path.Combine(appPath, "GameConfig", dataFolder)))
				iniFolder = Path.Combine(appPath, "GameConfig", dataFolder);
			else
				iniFolder = Path.Combine(appPath, "..\\GameConfig", dataFolder);

			progress.SetTask("Splitting Game Content");
			foreach (Templates.SplitEntry splitEntry in splitEntries)
				ProjectFunctions.SplitTemplateEntry(splitEntry, progress, gamePath, iniFolder, projFolder, overwrite);
			// Split MDL files for SA2
			if (splitMDLEntries.Count > 0)
			{
				progress.SetTask("Splitting Character Models");
				foreach (Templates.SplitEntryMDL splitMDL in splitMDLEntries)
					ProjectFunctions.SplitTemplateMDLEntry(splitMDL, progress, gamePath, projFolder, overwrite);
			}
			// Project folders for buildable PC games
			progress.SetTask("Updating Project File");
			UpdateProjectFile(progress);
			progress.StepProgress();
		}

		void UpdateProjectFile(SAModel.SAEditorCommon.UI.ProgressDialog progress)
		{
			bool needsUpdate = false;

			if (splitEntries.Count > 0 || splitMDLEntries.Count > 0)
			{
				Templates.ProjectTemplate projFile = ProjectFunctions.openProjectFileString(Path.GetFullPath(SAToolsHub.projXML));
				Templates.ProjectInfo projInfo = projFile.GameInfo;
				List<Templates.SplitEntry> projEntries = projFile.SplitEntries;
				List<Templates.SplitEntryMDL> projMdlEntries = projFile.SplitMDLEntries;
				

				foreach (Templates.SplitEntry entry in splitEntries)
				{
					if (!projEntries.Exists(x => x.IniFile == entry.IniFile))
					{
						projEntries.Add(entry);
						needsUpdate = true;
					}
				}

				if (projMdlEntries.Count > 0)
				{
					foreach (Templates.SplitEntryMDL entry in splitMDLEntries)
					{
						if (!projMdlEntries.Exists(x => x.ModelFile == entry.ModelFile))
						{
							projMdlEntries.Add(entry);
							needsUpdate = true;
						}
					}
				}

				if (needsUpdate)
				{
					XmlSerializer serializer = new XmlSerializer(typeof(Templates.ProjectTemplate));
					TextWriter writer = new StreamWriter(SAToolsHub.projXML);
					Templates.ProjectTemplate updProjFile = new Templates.ProjectTemplate();


					updProjFile.GameInfo = projInfo;
					updProjFile.SplitEntries = projEntries;
					if (splitMDLEntries.Count > 0)
						updProjFile.SplitMDLEntries = projMdlEntries;

					serializer.Serialize(writer, updProjFile);
					writer.Close();
				}
			}
		}

#region Background Worker
		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			using (splitProgress = new SAModel.SAEditorCommon.UI.ProgressDialog("Creating project"))
			{
				Invoke((Action)splitProgress.Show);

				splitGame(SAToolsHub.setGame, splitProgress);

				Invoke((Action)splitProgress.Close);
			}
		}

		private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e != null && e.Error != null)
			{
				MessageBox.Show("Item failed to split: " + e.Error.Message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				DialogResult successDiag = MessageBox.Show("Selected items have been resplit successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.None);
				if (successDiag == DialogResult.OK)
				{
					SAToolsHub.resplit = true;
					this.Close();
				}
			}
		}
#endregion
	}
}
