namespace SharePodLib_Sample.Forms
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.PlaylistsListBox = new System.Windows.Forms.ListBox();
            this.TracksGridView = new System.Windows.Forms.DataGridView();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Artist = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Album = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Genre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.AddItemToolButton = new System.Windows.Forms.ToolStripButton();
            this.RemoveItemFromPlaylistToolButton = new System.Windows.Forms.ToolStripButton();
            this.RemoveItemToolButton = new System.Windows.Forms.ToolStripButton();
            this.AddPlaylistToolButton = new System.Windows.Forms.ToolStripButton();
            this.DeletePlaylistToolButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveChangesToolButton = new System.Windows.Forms.ToolStripButton();
            this.AboutToolButton = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.IPodTypeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SpaceStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ExportSelectedPlaylist = new System.Windows.Forms.Button();
            this.ExportSelectedTrack = new System.Windows.Forms.Button();
            this.ReloadIPodButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TracksGridView)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PlaylistsListBox
            // 
            this.PlaylistsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.PlaylistsListBox.FormattingEnabled = true;
            this.PlaylistsListBox.Location = new System.Drawing.Point(12, 61);
            this.PlaylistsListBox.Name = "PlaylistsListBox";
            this.PlaylistsListBox.Size = new System.Drawing.Size(158, 316);
            this.PlaylistsListBox.TabIndex = 0;
            // 
            // TracksGridView
            // 
            this.TracksGridView.AllowUserToAddRows = false;
            this.TracksGridView.AllowUserToDeleteRows = false;
            this.TracksGridView.AllowUserToResizeRows = false;
            this.TracksGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TracksGridView.BackgroundColor = System.Drawing.Color.White;
            this.TracksGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TracksGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TracksGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Title,
            this.Artist,
            this.Album,
            this.Genre});
            this.TracksGridView.Location = new System.Drawing.Point(176, 44);
            this.TracksGridView.MultiSelect = false;
            this.TracksGridView.Name = "TracksGridView";
            this.TracksGridView.ReadOnly = true;
            this.TracksGridView.RowHeadersVisible = false;
            this.TracksGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.TracksGridView.Size = new System.Drawing.Size(577, 333);
            this.TracksGridView.TabIndex = 1;
            // 
            // Title
            // 
            this.Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Title.DataPropertyName = "Title";
            this.Title.FillWeight = 200F;
            this.Title.HeaderText = "Title";
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            // 
            // Artist
            // 
            this.Artist.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Artist.DataPropertyName = "Artist";
            this.Artist.HeaderText = "Artist";
            this.Artist.Name = "Artist";
            this.Artist.ReadOnly = true;
            // 
            // Album
            // 
            this.Album.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Album.DataPropertyName = "Album";
            this.Album.HeaderText = "Album";
            this.Album.Name = "Album";
            this.Album.ReadOnly = true;
            // 
            // Genre
            // 
            this.Genre.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Genre.DataPropertyName = "Genre";
            this.Genre.HeaderText = "Genre";
            this.Genre.Name = "Genre";
            this.Genre.ReadOnly = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddItemToolButton,
            this.RemoveItemFromPlaylistToolButton,
            this.RemoveItemToolButton,
            this.AddPlaylistToolButton,
            this.DeletePlaylistToolButton,
            this.toolStripSeparator1,
            this.SaveChangesToolButton,
            this.AboutToolButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(2, 5, 1, 4);
            this.toolStrip1.Size = new System.Drawing.Size(765, 32);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // AddItemToolButton
            // 
            this.AddItemToolButton.Image = ((System.Drawing.Image)(resources.GetObject("AddItemToolButton.Image")));
            this.AddItemToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddItemToolButton.Name = "AddItemToolButton";
            this.AddItemToolButton.Size = new System.Drawing.Size(78, 20);
            this.AddItemToolButton.Text = "Add tracks";
            this.AddItemToolButton.Click += new System.EventHandler(this.AddItemToolButton_Click);
            // 
            // RemoveItemFromPlaylistToolButton
            // 
            this.RemoveItemFromPlaylistToolButton.Image = ((System.Drawing.Image)(resources.GetObject("RemoveItemFromPlaylistToolButton.Image")));
            this.RemoveItemFromPlaylistToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RemoveItemFromPlaylistToolButton.Name = "RemoveItemFromPlaylistToolButton";
            this.RemoveItemFromPlaylistToolButton.Size = new System.Drawing.Size(127, 20);
            this.RemoveItemFromPlaylistToolButton.Text = "Remove from playlist";
            this.RemoveItemFromPlaylistToolButton.Click += new System.EventHandler(this.RemoveItemFromPlaylistToolButton_Click);
            // 
            // RemoveItemToolButton
            // 
            this.RemoveItemToolButton.Image = ((System.Drawing.Image)(resources.GetObject("RemoveItemToolButton.Image")));
            this.RemoveItemToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RemoveItemToolButton.Name = "RemoveItemToolButton";
            this.RemoveItemToolButton.Size = new System.Drawing.Size(85, 20);
            this.RemoveItemToolButton.Text = "Delete track";
            this.RemoveItemToolButton.Click += new System.EventHandler(this.RemoveItemToolButton_Click);
            // 
            // AddPlaylistToolButton
            // 
            this.AddPlaylistToolButton.Image = ((System.Drawing.Image)(resources.GetObject("AddPlaylistToolButton.Image")));
            this.AddPlaylistToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddPlaylistToolButton.Name = "AddPlaylistToolButton";
            this.AddPlaylistToolButton.Size = new System.Drawing.Size(82, 20);
            this.AddPlaylistToolButton.Text = "Add playlist";
            this.AddPlaylistToolButton.Click += new System.EventHandler(this.AddPlaylistToolButton_Click);
            // 
            // DeletePlaylistToolButton
            // 
            this.DeletePlaylistToolButton.Image = ((System.Drawing.Image)(resources.GetObject("DeletePlaylistToolButton.Image")));
            this.DeletePlaylistToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DeletePlaylistToolButton.Name = "DeletePlaylistToolButton";
            this.DeletePlaylistToolButton.Size = new System.Drawing.Size(94, 20);
            this.DeletePlaylistToolButton.Text = "Delete playlist";
            this.DeletePlaylistToolButton.Click += new System.EventHandler(this.DeletePlaylistToolButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // SaveChangesToolButton
            // 
            this.SaveChangesToolButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveChangesToolButton.Image")));
            this.SaveChangesToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveChangesToolButton.Name = "SaveChangesToolButton";
            this.SaveChangesToolButton.Size = new System.Drawing.Size(94, 20);
            this.SaveChangesToolButton.Text = "Save changes";
            this.SaveChangesToolButton.Click += new System.EventHandler(this.SaveChangesToolButton_Click);
            // 
            // AboutToolButton
            // 
            this.AboutToolButton.Image = ((System.Drawing.Image)(resources.GetObject("AboutToolButton.Image")));
            this.AboutToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AboutToolButton.Name = "AboutToolButton";
            this.AboutToolButton.Size = new System.Drawing.Size(56, 20);
            this.AboutToolButton.Text = "About";
            this.AboutToolButton.Click += new System.EventHandler(this.AboutToolButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Media files|*.mp3;*.wav;*.m4v;*.m4a";
            this.openFileDialog1.Multiselect = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.IPodTypeStatusLabel,
            this.SpaceStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 412);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(765, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // IPodTypeStatusLabel
            // 
            this.IPodTypeStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 10, 2);
            this.IPodTypeStatusLabel.Name = "IPodTypeStatusLabel";
            this.IPodTypeStatusLabel.Size = new System.Drawing.Size(37, 17);
            this.IPodTypeStatusLabel.Text = "(type)";
            // 
            // SpaceStatusLabel
            // 
            this.SpaceStatusLabel.Name = "SpaceStatusLabel";
            this.SpaceStatusLabel.Size = new System.Drawing.Size(43, 17);
            this.SpaceStatusLabel.Text = "(space)";
            // 
            // ExportSelectedPlaylist
            // 
            this.ExportSelectedPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExportSelectedPlaylist.Location = new System.Drawing.Point(12, 382);
            this.ExportSelectedPlaylist.Name = "ExportSelectedPlaylist";
            this.ExportSelectedPlaylist.Size = new System.Drawing.Size(138, 23);
            this.ExportSelectedPlaylist.TabIndex = 4;
            this.ExportSelectedPlaylist.Text = "Export selected playlist";
            this.ExportSelectedPlaylist.UseVisualStyleBackColor = true;
            this.ExportSelectedPlaylist.Click += new System.EventHandler(this.ExportSelectedPlaylist_Click);
            // 
            // ExportSelectedTrack
            // 
            this.ExportSelectedTrack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExportSelectedTrack.Location = new System.Drawing.Point(176, 382);
            this.ExportSelectedTrack.Name = "ExportSelectedTrack";
            this.ExportSelectedTrack.Size = new System.Drawing.Size(132, 23);
            this.ExportSelectedTrack.TabIndex = 5;
            this.ExportSelectedTrack.Text = "Export selected tracks";
            this.ExportSelectedTrack.UseVisualStyleBackColor = true;
            this.ExportSelectedTrack.Click += new System.EventHandler(this.ExportSelectedTrack_Click);
            // 
            // ReloadIPodButton
            // 
            this.ReloadIPodButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ReloadIPodButton.Location = new System.Drawing.Point(669, 382);
            this.ReloadIPodButton.Name = "ReloadIPodButton";
            this.ReloadIPodButton.Size = new System.Drawing.Size(84, 23);
            this.ReloadIPodButton.TabIndex = 6;
            this.ReloadIPodButton.Text = "Reload iPod";
            this.ReloadIPodButton.UseVisualStyleBackColor = true;
            this.ReloadIPodButton.Click += new System.EventHandler(this.ReloadIPodButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Playlists:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 434);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ReloadIPodButton);
            this.Controls.Add(this.ExportSelectedTrack);
            this.Controls.Add(this.ExportSelectedPlaylist);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.TracksGridView);
            this.Controls.Add(this.PlaylistsListBox);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MainForm";
            this.Text = "SharePodLib Sample";
            ((System.ComponentModel.ISupportInitialize)(this.TracksGridView)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox PlaylistsListBox;
        private System.Windows.Forms.DataGridView TracksGridView;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton AddItemToolButton;
		private System.Windows.Forms.ToolStripButton RemoveItemToolButton;
		private System.Windows.Forms.ToolStripButton AddPlaylistToolButton;
		private System.Windows.Forms.ToolStripButton DeletePlaylistToolButton;
		private System.Windows.Forms.ToolStripButton SaveChangesToolButton;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel SpaceStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel IPodTypeStatusLabel;
		private System.Windows.Forms.ToolStripButton RemoveItemFromPlaylistToolButton;
		private System.Windows.Forms.Button ExportSelectedPlaylist;
		private System.Windows.Forms.Button ExportSelectedTrack;
        private System.Windows.Forms.ToolStripButton AboutToolButton;
        private System.Windows.Forms.Button ReloadIPodButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn Artist;
        private System.Windows.Forms.DataGridViewTextBoxColumn Album;
        private System.Windows.Forms.DataGridViewTextBoxColumn Genre;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
	}
}

