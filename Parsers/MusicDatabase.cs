/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib.Exceptions;
using SharePodLib.Forms;
using System.Diagnostics;
using System.Threading;
using SharePodLib.DatabaseHash;
using System.Windows.Forms;
using SharePodLib.Parsers.iTunesDB;
using SharePodLib.IPodDevice.FileSystems;

namespace SharePodLib.Parsers
{

    internal class MusicDatabase : BaseDatabase
    {
        internal iTunesDBRoot DatabaseRoot;
        internal TrackList TracksList;
        internal PlaylistList PlaylistsList;

        public MusicDatabase(IPod iPod)
        {
            _iPod = iPod;
            DeviceFileSystem fs = _iPod.FileSystem;
            if (fs.FileExists(fs.CombinePath(fs.ITunesFolderPath, "iTunesCDB")))
                _databaseFilePath = fs.CombinePath(fs.ITunesFolderPath, "iTunesCDB");
            else
                _databaseFilePath = fs.CombinePath(fs.ITunesFolderPath, "iTunesDB");
        }

        public override int Version
        {
            get { return DatabaseRoot.VersionNumber; }
        }

        internal int HashingScheme
        {
            get { return DatabaseRoot.HashingScheme; }
        }

        public override void Parse()
        {
            if (!_iPod.FileSystem.FileExists(_databaseFilePath))
            {
                throw new InvalidIPodDriveException("iPod database not found in " + _databaseFilePath);
            }

            if (_iPod.FileSystem.GetFileLength(_databaseFilePath) == 0)
            {
                throw new InvalidIPodDriveException(_databaseFilePath + " file is empty. \r\nPlease run iTunes with your iPod connected, then re-open SharePod.");
            }

            if (_iPod.FileSystem.FileExists(_iPod.FileSystem.ITunesLockPath))
            {
                try
                {
                    _iPod.FileSystem.DeleteFile(_iPod.FileSystem.ITunesLockPath);
                }
                catch (Exception ex)
                {
                    throw new ITunesLockException(_iPod.FileSystem.ITunesLockPath);
                }
            }

            LoadingForm loadForm = null;

            if (!SharePodLib.IsLicenced())
            {
                loadForm = new LoadingForm();
                loadForm.Show();
                System.Windows.Forms.Application.DoEvents();
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (_databaseFilePath.EndsWith("iTunesCDB"))
                DatabaseRoot = new iTunesCDB.ITunesCDBRoot();
            else
                DatabaseRoot = new iTunesDBRoot();
            
            ReadDatabase(DatabaseRoot);

            stopwatch.Stop();
            Trace.WriteLine("MusicDatabase: " + _compatibility + ", version " + DatabaseRoot.VersionNumber);

            Debug.WriteLine("MusicDatabase: Parsed in " + stopwatch.ElapsedMilliseconds + " msec");
                                    
            TrackListContainer tracksContainer = (TrackListContainer)DatabaseRoot.GetChildSection(MHSDSectionType.Tracks).GetListContainer();
            TracksList = tracksContainer.GetTrackList();

            PlaylistsList = DatabaseRoot.GetPlaylistList();
            PlaylistsList.ResolveTracks();
                        
            if (loadForm != null)
            {
                loadForm.OnLoadComplete();
            }
        }
                
        public override void Save()
        {
            AssertIsWritable();
            Debug.WriteLine("Saving MusicDatabase " + DateTime.Now);
            IPodBackup.BackupDatabase(_iPod);

            PlaylistListV2Container playlistV2Container = null;
            if (DatabaseRoot.GetChildSection(MHSDSectionType.PlaylistsV2) != null)
            {
                playlistV2Container = (PlaylistListV2Container)DatabaseRoot.GetChildSection(MHSDSectionType.PlaylistsV2).GetListContainer();
                PlaylistList playlistV2List = playlistV2Container.GetPlaylistsList();
                if (this.PlaylistsList != playlistV2List)
                {
                    //if we aren't already using the V2 playlist, sync it up here
                    playlistV2List.FollowChanges(this.PlaylistsList);
                }
            }

            PlaylistsList[0].ReIndex();

            WriteDatabase(DatabaseRoot);
        }

        public override void DoActionOnWriteDatabase(FileStream stream)
        {
            if (DatabaseRoot.VersionNumber >= 25)
            {
                DatabaseHasher.Hash(stream, _iPod);
            }
        }
        

        #region Properties
        
        public override bool IsDirty
        {
            get
            {
                if (TracksList.IsDirty || PlaylistsList.IsDirty)
                    return true;

                foreach (Track t in TracksList)
                {
                    if (t.IsDirty)
                        return true;
                }
                foreach (Playlist p in PlaylistsList)
                {
                    if (p.IsDirty)
                        return true;
                }
                return false;
            }
        }

        #endregion

    }
}
