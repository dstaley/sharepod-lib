using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib;
using System.IO;
using SharePodLib.Parsers;
using System.Windows.Forms;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib_Sample
{
    public class IPodFileImporter
    {

        public delegate void ProgressEventHandler(string message);
        public delegate void ProgressErrorEventHandler(string message, string errorMessage);
        public event ProgressEventHandler ProgressEvent;
        public event ProgressErrorEventHandler ProgressErrorEvent;
        public event ProgressEventHandler Completed;

        public IPod IPod { get; private set; }
        private List<string> _copyFiles;
        private List<Track> _copiedTracks;

        private long _nbrBytesCopied;
        private DateTime _startTime;
        private bool _stopFlag;


        public IPodFileImporter(IPod iPod, List<string> copyFiles, bool askSubFolders)
        {
            IPod = iPod;
            _copyFiles = copyFiles;
            _copiedTracks = new List<Track>();

            //Handle the user importing whole folders.
            //Pre-process by replacing the folder entries with the contained files
            int filesCount = _copyFiles.Count;
            for (int i = filesCount - 1; i >= 0; i--)
            {
                if (Directory.Exists(_copyFiles[i]))
                {
                    ImportFolderFilesToCopyList(_copyFiles[i], askSubFolders);
                    _copyFiles.RemoveAt(i);
                }
            }
        }

        public int TrackCount
        {
            get { return _copyFiles.Count; }
        }

        public List<Track> CopiedTracks
        {
            get { return _copiedTracks; }
        }

        public void PerformCopy()
        {
            _startTime = DateTime.Now;
            _stopFlag = false;

            ProgressEvent("Waiting for iPod to begin sync...");

            IPod.FileSystem.StartSync();

            foreach (string filename in _copyFiles)
            {
                if (_stopFlag) break;

                try
                {
                    FileInfo file = new FileInfo(filename);

                    if (!IsFileSupported(file))
                    {
                        ProgressErrorEvent(filename, "Unsupported file type");
                        continue;
                    }

                    TagLib.File mediaFile = TagLib.File.Create(filename);

                    NewTrack newTrack = new NewTrack();
                    newTrack.FilePath = filename;
                    newTrack.Album = mediaFile.Tag.Album;
                    newTrack.Artist = mediaFile.Tag.FirstPerformer;

                    newTrack.Bitrate = (uint)mediaFile.Properties.AudioBitrate;
                    newTrack.Genre = mediaFile.Tag.FirstGenre;
                    newTrack.Length = (uint)mediaFile.Properties.Duration.TotalMilliseconds;
                    newTrack.TrackNumber = mediaFile.Tag.Track;

                    newTrack.AlbumArtist = mediaFile.Tag.FirstAlbumArtist;
                    newTrack.Composer = mediaFile.Tag.FirstComposer;
                    newTrack.Year = mediaFile.Tag.Year;
                    newTrack.Comments = mediaFile.Tag.Comment;
                    newTrack.DiscNumber = mediaFile.Tag.Disc;
                    newTrack.TotalDiscCount = mediaFile.Tag.DiscCount;
                    newTrack.AlbumTrackCount = mediaFile.Tag.TrackCount;
                    newTrack.Title = mediaFile.Tag.Title;
                    newTrack.IsVideo = ((mediaFile.Properties.MediaTypes & TagLib.MediaTypes.Video) == TagLib.MediaTypes.Video);

                    if (String.IsNullOrEmpty(newTrack.Title))
                        newTrack.Title = Path.GetFileNameWithoutExtension(file.Name);

                    bool tempArtworkFile = false;
                    string artworkFile = "";
                    if (mediaFile.Tag.Pictures.Length > 0)
                    {
                        artworkFile = Path.GetTempFileName();
                        if (mediaFile.Tag.Pictures[0].Data.Count > 100)
                        {
                            File.WriteAllBytes(artworkFile, mediaFile.Tag.Pictures[0].Data.Data);
                            newTrack.ArtworkFile = artworkFile;
                            tempArtworkFile = true;
                        }
                    }
                    if (!tempArtworkFile)
                    {
                        artworkFile = Path.Combine(file.Directory.FullName, "folder.jpg");
                        if (File.Exists(artworkFile))
                        {
                            newTrack.ArtworkFile = artworkFile;
                            try
                            {
                                List<TagLib.IPicture> pictures = new List<TagLib.IPicture>();
                                TagLib.Picture newPicture = new TagLib.Picture(artworkFile);
                                pictures.Add(newPicture);
                                mediaFile.Tag.Pictures = pictures.ToArray();
                                mediaFile.Save();
                            }
                            catch (Exception ex)
                            {
                                DebugLogger.LogException(ex);
                            }
                        }
                    }

                    Track addedTrack = IPod.Tracks.Add(newTrack);

                    if (tempArtworkFile)
                        File.Delete(artworkFile);

                    _copiedTracks.Add(addedTrack);
                    ProgressEvent(filename);
                    _nbrBytesCopied += file.Length;
                }
                catch (Exception ex)
                {
                    DebugLogger.LogException(ex);
                    ProgressErrorEvent("Failed to import " + filename, ex.Message);
                }
            }

            ProgressEvent("Synchronizing database");

            IPod.SaveChanges();

            string message = "";
            if (_copiedTracks.Count == 0)
            {
                message = "No files were copied";
            }
            else
            {
                int secondsTaken = (int)new TimeSpan(DateTime.Now.Ticks - _startTime.Ticks).TotalSeconds;
                long mbSec = 0;
                if (secondsTaken > 0)
                    mbSec = (_nbrBytesCopied / 1048576) / secondsTaken;
                message = string.Format("Copy finished. {0} in {1} files copied in {2}. ({3}Mb/sec)", Helpers.GetFileSizeString(_nbrBytesCopied, 1), _copiedTracks.Count, Helpers.GetTimeString(secondsTaken), mbSec);
            }

            if (Completed != null)
            {
                Completed(message);
            }
        }

        private void ImportFolderFilesToCopyList(string folder, bool askSubFolders)
        {
            DialogResult result;

            if (!askSubFolders)
            {
                result = DialogResult.Yes;
            }
            else
            {
                result = MessageBox.Show("Do you want to include sub-folders of " + folder + "?", "SharePodLib-Sample", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }
            if (result == DialogResult.Cancel)
            {
                return;
            }

            DirectoryInfo di = new DirectoryInfo(folder);
            FileInfo[] files;
            if (result == DialogResult.Yes)
            {
                files = di.GetFiles("*.*", SearchOption.AllDirectories);
            }
            else
            {
                files = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            }

            foreach (FileInfo file in files)
            {
                try
                {
                    _copyFiles.Add(file.FullName);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogException(ex);
                }
            }
            return;
        }

        public static bool IsFileSupported(FileInfo file)
        {
            string extension = file.Extension.ToLower();
            if (extension == ".mp3" || extension == ".m4a" || extension == ".m4v" ||
                extension == ".wav" || extension == ".mp4" || extension == ".aac" || extension == ".m4b" ||
                extension == ".aif" || extension == ".afc" || extension == ".m4r")
                return true;

            return false;
        }

        public static TagLib.File M4bResolver(TagLib.File.IFileAbstraction abstraction, string mimetype, TagLib.ReadStyle style)
        {
            if (abstraction.Name.EndsWith(".m4b"))
                return new TagLib.Mpeg4.File(abstraction, style);
            return null;
        }

        public static TagLib.File M4rResolver(TagLib.File.IFileAbstraction abstraction, string mimetype, TagLib.ReadStyle style)
        {
            if (abstraction.Name.EndsWith(".m4r"))
                return new TagLib.Mpeg4.File(abstraction, style);
            return null;
        }

        internal void StopCopying()
        {
            _stopFlag = true;
        }
    }
}
