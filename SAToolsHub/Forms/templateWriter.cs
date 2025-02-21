﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using SAModel.SAEditorCommon.ProjectManagement;

namespace SAToolsHub
{
	public partial class templateWriter : Form
	{
		// TODO: templateWriter - Rewrite to be actually useable
		public templateWriter()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Stream projFileStream;
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "Template File (*.xml)|*.xml";
			saveFileDialog1.RestoreDirectory = true;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				if ((projFileStream = saveFileDialog1.OpenFile()) != null)
				{
					XmlSerializer serializer = new XmlSerializer(typeof(Templates.SplitTemplate));
					XmlWriter writer = XmlWriter.Create(projFileStream, new XmlWriterSettings() { Indent = true });

					Templates.SplitTemplate splitTemplateFile = new Templates.SplitTemplate();

					serializer.Serialize(writer, splitTemplateFile);
					projFileStream.Close();
				}
			}
		}
	}
}
