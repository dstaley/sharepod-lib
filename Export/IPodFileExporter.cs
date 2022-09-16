/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 


using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib;
using SharePodLib.Parsers.iTunesDB;
using System.IO;
using SharePodLib.Parsers;
using System.Windows.Forms;
using System.Diagnostics;
using SharePodLib.Forms;

namespace SharePodLib.Export
{
    
	/// <summary>
	/// Class for copying tracks from the iPod to the PC
	/// </summary>
    public class IPodFileExporter
    {
        /// <summary>
        /// How to handle files that already exist. Either ignore (don't copy) or Rename (Copy of ..., Copy (2) of ...)
        /// </summary>
        public enum FilenameCollisionBehavior
        {
            Ignore,
            Rename,
            PromptUser
        }

        #region Events
        /// <summary>
        /// Delegate for song transferred event
        /// </summary>
        /// <param name="track"></param>
        public delegate void ProgressEventHandler(Track track);
        /// <summary>
        /// Delegate for song transfer error event
        /// </summary>
        /// <param name="track"></param>
        /// <param name="errorMessage"></param>
        public delegate void ProgressErrorEventHandler(Track track, string errorMessage);
        /// <summary>
        /// Delegate for all songs transferred complete event
        /// </summary>
        /// <param name="message"></param>
        public delegate void CompletedEventHandler(string message);

        /// <summary>
        /// Fires when a track has been transferred.
        /// </summary>
        public event ProgressEventHandler ProgressEvent;

        /// <summary>
        /// Fires when an error occured transferring a song 
        /// </summary>
        public event ProgressErrorEventHandler ProgressErrorEvent;

        /// <summary>
        /// Fires when all songs have been transfer
        /// </summary>
        public event CompletedEventHandler Completed;

        #endregion

        
        private List<Track> _copyTracks;
        private List<Playlist> _copyPlaylists;
        private string _copyFolder;
        private string _copyFormat;
        private FilenameCollisionBehavior _collisionBehaviour;
        private IPodFileExporterResult _result;
        private bool _abortFlag;
        private List<Track> _skippedTracks = new List<Track>();

        /// <summary>
        /// The result of PerformCopy.  Includes 
        /// </summary>
        public IPodFileExporterResult Result
        {
            get { return _result; }
        }

        public IPod IPod { get; private set; }
        
        /// <summary>
		/// IPodFileExporter constructor
        /// </summary>
        /// <param name="iPod"></param>
        public IPodFileExporter(IPod iPod)
        {
            IPod = iPod;
        }

        /// <summary>
        /// Set list of tracks to copy to PC using the config defined in constructor
        /// </summary>
        /// <param name="tracks"></param>
        public void SetTracksToCopy(List<Track> tracks)
        {
            _copyPlaylists = null;
            _copyTracks = tracks;
        }

        /// <summary>
        /// Set list of playlists to copy using the config defined in constructor
        /// </summary>
        /// <param name="playlists"></param>
        public void SetPlaylistsToCopy(List<Playlist> playlists)
        {
            _copyTracks = null;
            _copyPlaylists = playlists;
        }

        /// <summary>
        /// How many tracks are in the copy list
        /// </summary>
        public int TrackCount
        {
            get
            {
                if (_copyTracks != null)
                    return _copyTracks.Count;
                else if (_copyPlaylists != null)
                {
                    int plTrackCount = 0;
                    foreach (Playlist p in _copyPlaylists)
                        plTrackCount += p.TrackCount;
                    return plTrackCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public uint BytesToCopy
        {
            get
            {
                uint bytes = 0;
                if (_copyTracks != null)
                    _copyTracks.ForEach(a => bytes += a.FileSize.ByteCount);
                else if (_copyPlaylists != null)
                {
                    foreach (Playlist p in _copyPlaylists)
                        foreach (Track t in p.Tracks)
                            bytes += t.FileSize.ByteCount;
                }
                return bytes;
            }
        }
                

        /// <summary>
        /// Copy the selected items to the PC.
		/// This should be called after a call to "SetTracksToCopy()" or SetPlaylistsToCopy().
        /// </summary>
        /// <param name="copyFolder">Output folder to copy files to</param>
        /// <param name="copyFormat">
        /// Defines the structure of the copied filename.
        /// Supported tags are returned from IPodFileExporter.ValidFileNamePatternTokens.
        /// For example: "[Artist] - [Album] - [Title]" or "[Artist]\[Album] - [Title]"
        /// </param>
        /// <param name="iTunesImporter">An IITunesImporter to use to import tracks into iTunes. Can be null</param>
        /// <param name="collisionBehaviour">How to handle existing files</param>
        /// <returns>List of copied filenames</returns>
        public IPodFileExporterResult PerformCopy(string copyFolder, string copyFormat, IITunesImporter iTunesImporter, FilenameCollisionBehavior collisionBehaviour)
        {
            _result = new IPodFileExporterResult();
            _skippedTracks = new List<Track>();
            _abortFlag = false;

            _collisionBehaviour = collisionBehaviour;

            DirectoryInfo di = new DirectoryInfo(copyFolder);
            if (!di.Exists)
            {
                throw new DirectoryNotFoundException(copyFolder + " is not a valid directory");
            }
            if (!copyFolder.EndsWith("\\"))
            {
                copyFolder += "\\";
            }

            _copyFolder = copyFolder;
            _copyFormat = copyFormat;

            _result.StartTracking();

            if (_copyTracks != null)
            {
                PerformTracksCopy(iTunesImporter);
            }
            else if (_copyPlaylists != null)
            {
                PerformPlaylistCopy(iTunesImporter);
            }

            _result.StopTracking();
            _result.WasStopped = _abortFlag;

            if (iTunesImporter != null)
            {
                try
                {
                    bool imported = iTunesImporter.CanImport;
                    iTunesImporter.FinishImport();
                    _result.FilesImportedToiTunes = imported;
                }
                catch (Exception ex)
                { DebugLogger.LogException(ex); }
            }
            
            RaiseCompletedEvent(_result.ToString());
            return _result;
        }

        /// <summary>
        /// Stops the current copy operation.
        /// The Completed event will still be raised (after the current file has finished copying) but this.Result.WasStopped will be true.
        /// </summary>
        public void StopCopying()
        {
            _abortFlag = true;
        }

        /// <summary>
        /// Actually do the copy.
        /// </summary>
        private void PerformTracksCopy(IITunesImporter iTunesImporter)
        {
            foreach (Track track in _copyTracks)
            {
                try
                {
                    bool shouldRaiseEvent=true;
                    string newFileName = CopyTrack(track, ref shouldRaiseEvent);

                    if (iTunesImporter != null)
                        iTunesImporter.ImportFile(newFileName, track);
                    
                    if (shouldRaiseEvent)
                        RaiseProgressEvent(track);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogException(ex);
                    RaiseProgressErrorEvent(track, ex.Message);
                    _result.OnTrackNotCopied();
                }
                if (_abortFlag)
                    break;
            }
        }

        /// <summary>
        /// Copy the list of playlists
        /// </summary>
        private void PerformPlaylistCopy(IITunesImporter iTunesImporter)
        {
            foreach (Playlist playlist in _copyPlaylists)
            {
                if (iTunesImporter != null) iTunesImporter.SetActivePlaylist(playlist.Name);

                foreach (Track track in playlist.Tracks)
                {
                    try
                    {
                        bool shouldRaiseEvent = true;
                        string newFileName = CopyTrack(track, ref shouldRaiseEvent);

                        if (iTunesImporter != null) iTunesImporter.ImportFile(newFileName, track);

                        if (shouldRaiseEvent) RaiseProgressEvent(track);
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.LogException(ex);
                        RaiseProgressErrorEvent(track, ex.Message);
                        _result.OnTrackNotCopied();
                    }
                    if (_abortFlag)
                        break;
                }
                if (_abortFlag)
                    break;
            }
        }

        /// <summary>
        /// Copy the list of tracks
        /// </summary>
        private string CopyTrack(Track track, ref bool raiseProgressEvent)
        {
            string newFileName = GetOutputFileName(track);

            //If we've already copied the track, dont copy again. 
            if (_result.TracksCopied.ContainsKey(track))
            {
                raiseProgressEvent = false;
                return _result.TracksCopied[track];
            }
            //If we've skipped the track, dont try and copy again.
            if (_skippedTracks.Contains(track))
            {
                raiseProgressEvent = false;
                return newFileName;
            }

            string path = newFileName.Substring(0, newFileName.LastIndexOf("\\"));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Trace.WriteLine(String.Format("Copying {0} to {1}", Path.Combine(IPod.DriveLetter, track.FilePath), newFileName));

            if (File.Exists(newFileName))
            {
                FilenameCollisionBehavior behaviour = _collisionBehaviour;

                if (_collisionBehaviour == FilenameCollisionBehavior.PromptUser)
                {
                    FileCopyPrompt promptForm = new FileCopyPrompt(newFileName);
                    promptForm.ShowDialog();
                    behaviour = promptForm.SelectedBehaviour;
                    if (promptForm.ApplyToAllDuplicates)
                        _collisionBehaviour = behaviour;
                }
                if (behaviour == FilenameCollisionBehavior.Rename)
                {
                    newFileName = GetNewFileNameForFile(newFileName);
                }
                else
                {
                    _skippedTracks.Add(track);
                    return newFileName;
                }
            }

            IPod.FileSystem.CopyFileFromDevice(Path.Combine(IPod.DriveLetter, track.FilePath), newFileName);
            _result.OnTrackCopied(track, newFileName, new FileInfo(newFileName).Length);

            return newFileName;
        }
               
        /// <summary>
        /// Builds up a filename from a Track and placeholder tokens.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        private string GetOutputFileName(Track track)
        {
            string extension = Path.GetExtension(track.FilePath);

            string newName = _copyFormat + extension;
            newName = newName.Replace("[Title]", GetSafeTag(track.Title));
            newName = newName.Replace("[Artist]", GetSafeTag(track.Artist));
            newName = newName.Replace("[Album]", GetSafeTag(track.Album));
            newName = newName.Replace("[TrackNumber]", track.TrackNumber.ToString("00"));
            newName = newName.Replace("[Genre]", GetSafeTag(track.Genre));
            newName = newName.Replace("[Composer]", GetSafeTag(track.Composer));
            newName = newName.Replace("[AlbumArtist]", GetSafeTag(track.AlbumArtist));
            newName = newName.Replace("[DiscNumber]", track.DiscNumber.ToString());
            newName = newName.Replace("[Year]", track.Year.ToString());
            
            while (newName.Contains(" \\"))
            {
                newName = newName.Replace(" \\", "\\");
            }
                        
            if (!_copyFolder.EndsWith("\\"))
                _copyFolder += "\\";
            return _copyFolder + newName;
        }

        /// <summary>
        /// Strips out bad characters to have in filenames
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private string GetSafeTag(string tag)
        {
            if (tag == null)
                tag = String.Empty;
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
                tag = tag.Replace(c, '_');

            tag = tag.Trim();
            if (String.IsNullOrEmpty(tag))
            {
                return "Unknown";
            }
            else
            {
                return tag;
            }
        }

        /// <summary>
        /// Generates Copy of xxx or Copy (x) of xxx filenames.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string GetNewFileNameForFile(string file)
        {
            string oldFileName = Path.GetFileName(file);
            string path = Path.GetDirectoryName(file);
            
            for (int i = 1; ; i++)
            {
                string newFileName = Path.Combine(path, String.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(oldFileName), i, Path.GetExtension(oldFileName)));
                if (!File.Exists(newFileName))
                    return newFileName;
            }
        }

        private void RaiseCompletedEvent(string message)
        {
            if (Completed != null)
                Completed(message);
        }

        private void RaiseProgressEvent(Track track)
        {
            if (ProgressEvent != null)
            {
                ProgressEvent(track);
            }
        }

        private void RaiseProgressErrorEvent(Track track, string message)
        {
            if (ProgressErrorEvent != null)
            {
                ProgressErrorEvent(track, message);
            }
        }

        /// <summary>
        /// Contains the tokens to be used for the copyFormat argument.  E.g [Title], [Artist] etc.
        /// </summary>
        public List<string> ValidFileNamePatternTokens
        {
            get
            {
                List<string> validTokens = new List<string>();
                validTokens.Add("[Title]");
                validTokens.Add("[Artist]");
                validTokens.Add("[Album]");
                validTokens.Add("[TrackNumber]");
                validTokens.Add("[Genre]");
                validTokens.Add("[Composer]");
                validTokens.Add("[AlbumArtist]");
                validTokens.Add("[DiscNumber]");
                validTokens.Add("[Year]");
                return validTokens;
            }
        }

    }
}
