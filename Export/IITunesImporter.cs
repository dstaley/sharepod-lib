using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Export
{
    /// <summary>
    /// Implement this interface to easily import tracks into iTunes.
    /// You could either use the Apple SDK, or iTunes LibraryEditor (which SharePod uses).
    /// http://www.1amstudios.com/products/iTunesLibraryEditor/
    /// 
    /// See the SharePodLib-Sample for reference implementations.
    /// </summary>
    public interface IITunesImporter
    {
        /// <summary>
        /// Returns true if the importer can import
        /// </summary>
        bool CanImport { get; }
        /// <summary>
        /// Playlist that subsequent tracks should be added to.  If playlistName is null, then subsequent
        /// tracks should not be added to any playlist.
        /// </summary>
        /// <param name="playlistName"></param>
        void SetActivePlaylist(string playlistName);
        /// <summary>
        /// Import the specified file into iTunes
        /// </summary>
        /// <param name="filePath">Path to the local copied file</param>
        /// <param name="iPodTrack">iPod track which was copied</param>
        void ImportFile(string filePath, Track iPodTrack);
        /// <summary>
        /// This is called once all tracks have been copied.  Clean up, save files etc here.
        /// </summary>
        void FinishImport();
    }
}
