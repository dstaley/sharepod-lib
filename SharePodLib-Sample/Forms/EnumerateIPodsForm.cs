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
    public partial class EnumerateIPodsForm : Form
    {
        public EnumerateIPodsForm()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            List<IPod> iPods = SharePodLib.IPod.GetAllConnectedIPods(SharePodLib.IPodLoadAction.NoSync);
            IPodListBox.DataSource = null;
            IPodListBox.DataSource = iPods;

            this.Cursor = Cursors.Default;
        }
    }
}
