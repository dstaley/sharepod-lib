using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SharePodLib_Sample.Forms
{
	public partial class NewPlaylistForm : Form
	{
		public NewPlaylistForm()
		{
			InitializeComponent();
		}

		public string PlaylistName
		{
			get { return PlaylistTextbox.Text; }
		}			
	}
}