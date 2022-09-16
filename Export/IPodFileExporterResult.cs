using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Export
{
    public class IPodFileExporterResult
    {
        private DateTime _startTime;
        private int _nbrFilesCopied, _nbrFilesFailed, _secondsTaken;
        private bool _wasStopped;

        private long _nbrBytesCopied;
        private Dictionary<Track, string> _tracksCopied = new Dictionary<Track, string>();
        private bool _filesImportedToiTunes;

        /// <summary>
        /// Returns true if any files were imported into iTunes
        /// </summary>
        public bool FilesImportedToiTunes
        {
            get { return _filesImportedToiTunes; }
            internal set { _filesImportedToiTunes = value; }
        }

        /// <summary>
        /// The output filenames of copied tracks.
        /// </summary>
        public Dictionary<Track, string> TracksCopied
        {
            get { return _tracksCopied; }
        }

        /// <summary>
        /// How many tracks were actually copied
        /// </summary>
        public int CopiedTrackCount
        {
            get { return _nbrFilesCopied; }
        }

        /// <summary>
        /// How many tracks failed to copy
        /// </summary>
        public int FailedCopyTrackCount
        {
            get { return _nbrFilesFailed; }
        }

        /// <summary>
        /// How many seconds the copy took
        /// </summary>
        public int SecondsTaken
        {
            get { return _secondsTaken; }
        }

        /// <summary>
        /// How many bytes were copied.
        /// </summary>
        public long NbrBytesCopied
        {
            get { return _nbrBytesCopied; }
        }

        internal void StartTracking()
        {
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// True if the copy was stopped before completion
        /// </summary>
        public bool WasStopped
        {
            get { return _wasStopped; }
            set { _wasStopped = value; }
        }

        internal void StopTracking()
        {
            _secondsTaken = Math.Max(1, (int)new TimeSpan(DateTime.Now.Ticks - _startTime.Ticks).TotalSeconds);
        }

        internal void OnTrackCopied(Track track, string destination, long fileSize)
        {
            _tracksCopied.Add(track, destination);
            _nbrFilesCopied++;
            _nbrBytesCopied += fileSize;
        }

        internal void OnTrackNotCopied()
        {
            _nbrFilesFailed++;
        }


        public override string ToString()
        {
            if (_nbrFilesCopied == 0)
                return "No files were copied";

            long mbSec = (_nbrBytesCopied / 1048576) / _secondsTaken;
            string message = String.Format("Copy finished. {0} in {1} files copied in {2}. ({3}Mb/sec).", Helpers.GetFileSizeString(_nbrBytesCopied, 1), _nbrFilesCopied, Helpers.GetTimeString(_secondsTaken), mbSec);
            if (_nbrFilesFailed > 0)
                message += String.Format("\r\n{0} files couldn't be copied.", _nbrFilesFailed);

            return message;
        }
    }
}
