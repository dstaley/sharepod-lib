using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using SharePodLib.Export;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLibSample.ITunesImporters
{
    /// <summary>
    /// Sample iTunes SDK Importer.  
    /// To use, add a reference to the iTunes SDK dll and uncomment below.
    /// </summary>
    class ITunesSDKImporter : IITunesImporter
    {
        
        //iTunesLib.iTunesApp _iTunesApp;
        //iTunesLib.IITUserPlaylist _activePlaylist;

        public ITunesSDKImporter()
        {
            //try
            //{
            //    _iTunesApp = new iTunesLib.iTunesApp();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //    _iTunesApp = null;
            //}
        }


        #region IITunesImporter Members


        public void SetActivePlaylist(string playlistName)
        {
            //if (_iTunesApp == null) return;

            //try
            //{
            //    if (String.IsNullOrEmpty(playlistName))
            //        _activePlaylist = null;
            //    else
            //        _activePlaylist = (iTunesLib.IITUserPlaylist)_iTunesApp.CreatePlaylist(playlistName);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }

        public void ImportFile(string filePath, Track iPodTrack)
        {
            //if (_iTunesApp == null) return;

            //iTunesLib.IITOperationStatus status = null;

            //try
            //{
            //    if (_activePlaylist != null)
            //        status = _activePlaylist.AddFile(filePath);
            //    else
            //        status = _iTunesApp.LibraryPlaylist.AddFile(filePath);

            //    while (status.InProgress)
            //        Thread.Sleep(20);

            //    iTunesLib.IITTrack iTunesTrack = status.Tracks[1]; //collection is 1-based, just to make it more fun
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }

        public bool CanImport
        {
            get { return true; }
            //get { return _iTunesApp != null; }
        }

        public void FinishImport()
        {
            //if (_iTunesApp == null) return;

            //Marshal.ReleaseComObject(_iTunesApp);
            //_iTunesApp = null;
            //GC.Collect();
        }

        #endregion
    }
}
