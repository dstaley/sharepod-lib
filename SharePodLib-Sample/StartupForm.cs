using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharePodLib_Sample.Forms;

namespace SharePodLib_Sample
{
    public partial class StartupForm : Form
    {
        public StartupForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EnumerateIPodsForm form = new EnumerateIPodsForm();
            form.ShowDialog();
        }

        private void ConnectEventsButton_Click(object sender, EventArgs e)
        {
            EventsForm form = new EventsForm();
            form.ShowDialog();
        }

        private void SharePodCloneButton_Click(object sender, EventArgs e)
        {
            MainForm form = new MainForm();
            form.ShowDialog();
        }        
    }
}
