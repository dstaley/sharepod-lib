using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharePodLib;
using SharePodLib.Export;
using System.IO;

namespace SharePodLib.Forms
{
    internal partial class FileCopyPrompt : Form
    {
        IPodFileExporter.FilenameCollisionBehavior _behaviour = IPodFileExporter.FilenameCollisionBehavior.Ignore;

        public FileCopyPrompt(string filename)
        {
            InitializeComponent();
            FileNameLabel.Text = Path.GetFileName(filename);
            FolderName.Text = Path.GetDirectoryName(filename);
        }

        private void SkipFileButton_Click(object sender, EventArgs e)
        {
            _behaviour = IPodFileExporter.FilenameCollisionBehavior.Ignore;
            this.Close();
        }

        private void RenameFileButton_Click(object sender, EventArgs e)
        {
            _behaviour = IPodFileExporter.FilenameCollisionBehavior.Rename;
            this.Close();
        }

        public IPodFileExporter.FilenameCollisionBehavior SelectedBehaviour
        {
            get { return _behaviour; }
        }

        public bool ApplyToAllDuplicates
        {
            get { return ApplyToAllFilesCheckBox.Checked; }
        }
    }
}
