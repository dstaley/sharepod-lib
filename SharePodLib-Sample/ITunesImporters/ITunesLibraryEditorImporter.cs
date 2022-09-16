using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.iTunesDB;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using SharePodLib.Export;

namespace SharePodLib_Sample.ITunesImporters
{
    /// <summary>
    /// Sample iTunes LibraryEditor importer.
    /// For details about why you should be using this in your application, 
    /// see http://www.1amstudios.com/products/iTunesLibraryEditor/
    /// </summary>
    class ITunesLibraryEditorImporter : IITunesImporter
    {
        iTunesLibraryEditor.iTunesLibrary _library;
        iTunesLibraryEditor.Playlist _activePlaylist;

        public ITunesLibraryEditorImporter()
        {
            try
            {
                _library = new iTunesLibraryEditor.iTunesLibrary();
                _library.AcquireLock();
            }
            catch (Exception ex)
            {
                SharePodLib.DebugLogger.LogException(ex);
            }
        }

        public bool CanImport
        {
            get { return _library != null; }
        }


        public void SetActivePlaylist(string playlistName)
        {
            if (_library == null) return;

            try
            {
                if (String.IsNullOrEmpty(playlistName))
                    _activePlaylist = null;
                else
                {
                    _activePlaylist = _library.Playlists.GetPlaylistByName(playlistName);
                    if (_activePlaylist == null)
                        _activePlaylist = _library.Playlists.Add(playlistName);
                }
            }
            catch (Exception ex)
            {
                SharePodLib.DebugLogger.LogException(ex);
            }
        }

        public void ImportFile(string filePath, Track iPodTrack)
        {
            if (_library == null) return;

            try
            {
                iTunesLibraryEditor.Track iTunesTrack = _library.Tracks.Find(a => a.FilePath == filePath);
                if (iTunesTrack == null)
                    iTunesTrack = _library.Tracks.Add(filePath);

                if (_activePlaylist != null)
                    _activePlaylist.AddTrack(iTunesTrack);
            }
            catch (Exception ex)
            {
                SharePodLib.DebugLogger.LogException(ex);
            }
        }

        public void FinishImport()
        {
            if (_library == null) return;

            _library.Backup();
            _library.ReleaseLock();
            _library.SaveChanges();
        }
    }
}
