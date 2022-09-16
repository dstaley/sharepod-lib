using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharePodLib;
using SharePodLib.Parsers.iTunesDB;
using SharePodLib_Sample.Forms;
using SharePodLib.Export;
using System.IO;
using SharePodLib.Exceptions;
using SharePodLib.Parsers;

namespace SharePodLib_Sample.Forms
{
	public partial class MainForm : Form
	{
		private IPod _iPod;

		public MainForm()
		{
			InitializeComponent();
			TracksGridView.AutoGenerateColumns = false;
			PlaylistsListBox.SelectedIndexChanged += new EventHandler(PlaylistsListBox_SelectedIndexChanged);
			this.FormClosed += new FormClosedEventHandler(MainForm_FormClosed);
            this.Shown += new EventHandler(MainForm_Shown);
		}


		void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_iPod != null)
			{
				_iPod.SaveChanges();
				_iPod.ReleaseLock();
			}
		}

        void MainForm_Shown(object sender, EventArgs e)
        {
            
            try
            {
                //Setting this to valid values will disable the loading screen.
                //Licence keys and source code can be purchased from http://www.getsharepod.com/fordevelopers/
                //Thanks for supporting SharePodLib!
                SharePodLib.SharePodLib.SetLicence("my name", "my licence key");

                //Start up logging 
                SharePodLib.DebugLogger.StartLogging("SharePodLibSample.log");

                //We should only call this once - it looks through each drive on the PC, so can be slow.
                _iPod = SharePodLib.IPod.GetConnectedIPod();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
                return;
            }

            try
            {
                _iPod.AssertIsWritable();
            }
            catch (UnsupportedITunesVersionException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            _iPod.AcquireLock();

            SetupUI();

        }

        private void SetupUI()
        {
            BindPlaylistsToListBox();
            IPodTypeStatusLabel.Text = "iPod type: " + _iPod.DeviceInfo.Family.ToString();
            SpaceStatusLabel.Text = String.Format("Space available: {0} / {1}", Helpers.GetFileSizeString(_iPod.FileSystem.AvailableFreeSpace, 0), Helpers.GetFileSizeString(_iPod.FileSystem.TotalSize, 0));
            PlaylistsListBox.SelectedIndex = 0; //select the first playlist automatically
        }


		private void BindPlaylistsToListBox()
		{
			//Could either use DataBinding :

			//PlaylistsListBox.DisplayMember = "Name";
			//PlaylistsListBox.DataSource = _iPod.Playlists.BindingList;

			//Or normal iteration
			
			PlaylistsListBox.Items.Clear();

			foreach (Playlist playlist in _iPod.Playlists)
			{
				PlaylistsListBox.Items.Add(String.Format("{0} - {1} items", playlist.Name, playlist.TrackCount));
			}
		}

		void PlaylistsListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (PlaylistsListBox.SelectedIndex == -1)
			{
				TracksGridView.DataSource = null;
				DeletePlaylistToolButton.Enabled = false;
				return;
			}

			Playlist selectedPlaylist = _iPod.Playlists[PlaylistsListBox.SelectedIndex];
			
			TracksGridView.DataSource = selectedPlaylist.BindingTrackList;
			
			DeletePlaylistToolButton.Enabled = !selectedPlaylist.IsMaster; //cant delete the Master playlist

		}

		#region Toolstrip buttons

		private void AddItemToolButton_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
                ImporterForm importForm = new ImporterForm(_iPod, openFileDialog1.FileNames);
                importForm.ShowDialog();
                
			}
		}

		private void RemoveItemToolButton_Click(object sender, EventArgs e)
		{
			if (TracksGridView.SelectedRows.Count == 0)
			{
				MessageBox.Show("You must select a track first");
			}
			//Because we're using DataBinding to the DataGridView, we can get the selected track like this:
			Track selectedTrack = (Track)TracksGridView.SelectedRows[0].DataBoundItem;

			try
			{
				//Remove the selected track from the iPod
				_iPod.Tracks.Remove(selectedTrack);
				//Note we don't have to refresh the TrackList control - using the BindingTrackList (line 94) does this for us.
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void RemoveItemFromPlaylistToolButton_Click(object sender, EventArgs e)
		{
			if (TracksGridView.SelectedRows.Count == 0)
			{
				MessageBox.Show("You must select a track first");
			}
			//Because we're using DataBinding to the DataGridView, we can get the selected track like this:
			Track selectedTrack = (Track)TracksGridView.SelectedRows[0].DataBoundItem;

			try
			{
				//Only remove the track from the current playlist, its still on the iPod
				Playlist selectedPlaylist = _iPod.Playlists[PlaylistsListBox.SelectedIndex];
				selectedPlaylist.RemoveTrack(selectedTrack);
				//Note we don't have to refresh the TrackList control - using the BindingTrackList (line 94) does this for us.
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}			
		}

		private void AddPlaylistToolButton_Click(object sender, EventArgs e)
		{
			NewPlaylistForm newPlaylistForm = new NewPlaylistForm();
			if (newPlaylistForm.ShowDialog() == DialogResult.OK)
			{
				try
				{
					//Add new playlist to iPod. Pass in the new name and get a playlist instance back.
					Playlist addedPlaylist = _iPod.Playlists.Add(newPlaylistForm.PlaylistName);
					//Add the first track on the iPod to this new playlist
					addedPlaylist.AddTrack(_iPod.Tracks[0]);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}

				BindPlaylistsToListBox();
			}
		}

		private void DeletePlaylistToolButton_Click(object sender, EventArgs e)
		{
			Playlist selectedPlaylist = _iPod.Playlists[PlaylistsListBox.SelectedIndex];
			
			try
			{
				_iPod.Playlists.Remove(selectedPlaylist, false);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			BindPlaylistsToListBox();
		}

		private void SaveChangesToolButton_Click(object sender, EventArgs e)
		{
			try
			{
				_iPod.SaveChanges();
				MessageBox.Show("Changes saved");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		#endregion		

		private void ExportSelectedPlaylist_Click(object sender, EventArgs e)
		{
			IPodFileExporter exporter = new IPodFileExporter(_iPod);

			List<Playlist> selectedPlaylists = new List<Playlist>();
			selectedPlaylists.Add(_iPod.Playlists[PlaylistsListBox.SelectedIndex]);

			exporter.SetPlaylistsToCopy(selectedPlaylists);

			ExporterForm form = new ExporterForm(exporter);
			form.ShowDialog();
		}

		private void ExportSelectedTrack_Click(object sender, EventArgs e)
		{
			
			IPodFileExporter exporter = new IPodFileExporter(_iPod);

			List<Track> selectedTracks = new List<Track>();
			Track selectedTrack = (Track)TracksGridView.SelectedRows[0].DataBoundItem;
			selectedTracks.Add(selectedTrack);

			exporter.SetTracksToCopy(selectedTracks);

			ExporterForm form = new ExporterForm(exporter);
			form.ShowDialog();
		}

		private void AboutToolButton_Click(object sender, EventArgs e)
		{
			AboutForm form = new AboutForm();
			form.ShowDialog();
		}

        private void ReloadIPodButton_Click(object sender, EventArgs e)
        {
            try
            {
                _iPod.ReleaseLock();
                _iPod.Refresh();
                _iPod.AcquireLock();
                SetupUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
	}
}