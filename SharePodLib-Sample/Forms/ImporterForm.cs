using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharePodLib;

namespace SharePodLib_Sample.Forms
{
    public partial class ImporterForm : Form
    {
        IPod _iPod;
        string[] _filenames;

        public ImporterForm(IPod ipod, string[] filenames)
        {
            InitializeComponent();

            _iPod = ipod;
            _filenames = filenames;            
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            IPodFileImporter importer = new IPodFileImporter(_iPod, new List<string>(_filenames), false);
            importer.ProgressErrorEvent += new IPodFileImporter.ProgressErrorEventHandler(Importer_ProgressErrorEvent);
            importer.ProgressEvent += new IPodFileImporter.ProgressEventHandler(Importer_ProgressEvent);
            importer.Completed += new IPodFileImporter.ProgressEventHandler(Importer_Completed);
            importer.PerformCopy();
        }

        void Importer_Completed(string message)
        {
            
            this.Invoke(new MethodInvoker(() => listBox1.Items.Add(message)));
            Application.DoEvents();
        }

        void Importer_ProgressEvent(string message)
        {
            this.Invoke(new MethodInvoker(() => listBox1.Items.Add("Progress: " + message)));
            Application.DoEvents();
        }

        void Importer_ProgressErrorEvent(string message, string errorMessage)
        {
            this.Invoke(new MethodInvoker(() => listBox1.Items.Add("Error: " + message)));
            Application.DoEvents();
        }
    }
}
