using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharePodLib.Export;
using System.IO;
using SharePodLib.Parsers.iTunesDB;
using SharePodLib_Sample.ITunesImporters;

namespace SharePodLib_Sample.Forms
{
	public partial class ExporterForm : Form
	{
		IPodFileExporter _exporter;

		public ExporterForm(IPodFileExporter exporter)
		{
			InitializeComponent();

			_exporter = exporter;
			_exporter.Completed += new IPodFileExporter.CompletedEventHandler(Exporter_Completed);
			_exporter.ProgressErrorEvent += new IPodFileExporter.ProgressErrorEventHandler(Exporter_ProgressErrorEvent);
			_exporter.ProgressEvent += new IPodFileExporter.ProgressEventHandler(Exporter_ProgressEvent);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
            Application.DoEvents();

			string exportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SharePodLibSample";
			Directory.CreateDirectory(exportDirectory);

            // To automatically add tracks into iTunes in the below method, pass in an IITunesImporter implementation
            // instead of null.
            // There are 2 examples in the ITunesImporters folder.
            // The first is based on the standard Apple iTunes SDK. (ITunesSDKImporter.cs)
            // The second is based on my iTunes LibraryEditor dll.  (ITunesLibraryEditorImporter.cs)

			_exporter.PerformCopy(
				exportDirectory,
				"[Artist]\\[Album] - [Title]",
				null,
				IPodFileExporter.FilenameCollisionBehavior.PromptUser);

		}

		void Exporter_ProgressEvent(Track track)
		{
			EventsListBox.Items.Add(String.Format("{0} - {1}", track.Artist, track.Title));
            Application.DoEvents();
		}

		void Exporter_ProgressErrorEvent(Track track, string errorMessage)
		{
			EventsListBox.Items.Add(String.Format("{0} - {1}", track.Artist, track.Title));
			EventsListBox.Items.Add("\t" + errorMessage);
            Application.DoEvents();
		}

		void Exporter_Completed(string message)
		{
			StopButton.Enabled = false;
			CloseButton.Enabled = true;
			MessageBox.Show(_exporter.Result.ToString());
		}

		private void StopButton_Click(object sender, EventArgs e)
		{
			_exporter.StopCopying();
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}