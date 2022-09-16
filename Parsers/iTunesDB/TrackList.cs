/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using SharePodLib.Databinding;
using SharePodLib.Exceptions;
using SharePodLib.Forms;

namespace SharePodLib.Parsers.iTunesDB
{
    // Implements a MHLT entry in iTunesDB
    /// <summary>
    /// List of iPod tracks.  This is where tracks are added/removed from the iPod.
    /// </summary>
    public class TrackList : BaseDatabaseElement
    {
        private int _trackCount;
        private DataBoundList<Track> _childSections;
        private bool _isDirty;

        internal TrackList()
        {
            _requiredHeaderSize = 12;
            _childSections = new DataBoundList<Track>();
        }

        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhlt");

            _trackCount = reader.ReadInt32();

            this.ReadToHeaderEnd(reader);

            for (int i = 0; i < _trackCount; i++)
            {
                Track track = new Track();
                track.Read(iPod, reader);
                _childSections.Add(track);
            }
        }
        
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(_identifier);
            writer.Write(_headerSize);
            writer.Write(_childSections.Count);
            writer.Write(_unusedHeader);

            for (int i = 0; i < _childSections.Count; i++)
            {
                _childSections[i].Write(writer);
            }
            _isDirty = false;
        }

        internal override int GetSectionSize()
        {
            int size = _headerSize;
            for (int i = 0; i < _childSections.Count; i++)
            {
                size += _childSections[i].GetSectionSize();
            }
            return size;
        }


        #endregion

        /// <summary>
        /// Returns track matching specified Id.  Returns null if no matching track found.
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public Track FindById(int trackId)
        {
            foreach (Track trk in _childSections)
            {
                if (trk.Id == trackId)
                    return trk;
            }
            return null;
        }

        public Track Find(Predicate<Track> match)
        {
            return _childSections.Find(match);
        }

        public List<Track> FindAll(Predicate<Track> match)
        {
            return _childSections.FindAll(match);
        }

        /// <summary>
        /// Returns track matching specified DBId.  Returns null if no matching track found.
        /// </summary>
        public Track FindByDBId(long dbId)
        {
            foreach (Track trk in _childSections)
            {
                if (trk.DBId == dbId)
                    return trk;
            }
            return null;
        }

        /// <summary>
        /// Databound list of tracks in this tracklist.  
        /// Tracks shouldnt be added/removed through this list (instead you should go through the normal
        /// iPod.Tracks.Add(newTrack), but its useful to bind to a DataGridView or other control which supports databinding
        /// </summary>
        public DataBoundList<Track> BindingList
        {
            get { return _childSections; }
        }

        /// <summary>
        /// Return the track at specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Track this[int index]
        {
            get { return _childSections[index]; }
        }

        /// <summary>
        /// Adds a new track to the iPod.  Copies the file onto the iPod drive.
        /// Throws exception if track couldn't be added, otherwise returns a full Track object.
        /// </summary>
        /// <param name="newItem"></param>
        /// <returns></returns>
        public Track Add(NewTrack newItem)
        {
            _iPod.AssertIsWritable();

            if (!newItem.IsVideo.HasValue)
            {
                throw new ArgumentException("NewTrack.IsVideo property must be set");
            }

            FileInfo fileInfo = new FileInfo(newItem.FilePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(newItem.FilePath + " couldn't be found");
            }

            Track existing = GetExistingTrack(newItem);
            if (existing != null)
            {
                throw new TrackAlreadyExistsException("A track with the same Title, Artist and Album already exists on your iPod", existing);
            }

            long freespace = _iPod.FileSystem.AvailableFreeSpace;
            freespace -= fileInfo.Length;
            if (freespace <= 0)
            {
                throw new OutOfDiskSpaceException("Your iPod does not have enough free space.");
            }

            Track track = new Track();
            track.Create(_iPod, newItem);

            //Try and actually copy the file onto the iPod drive
            string iPodFileName = Path.Combine(_iPod.DriveLetter, track.FilePath);

            //If the file is already in iPod's music folder, just move it to the new path, otherwise copy it
            if (newItem.FilePath.StartsWith(_iPod.FileSystem.IPodControlPath, StringComparison.InvariantCultureIgnoreCase))
            {
                File.Move(newItem.FilePath, iPodFileName);
            }
            else
            {
                _iPod.FileSystem.CopyFileToDevice(newItem.FilePath, iPodFileName);
            }

            if (!String.IsNullOrEmpty(newItem.ArtworkFile))
            {
                try
                {
                    track.SetArtwork(newItem.ArtworkFile);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogException(ex);
                }
            }

            _childSections.RaiseListChangedEvents = true;
            //track.Index = _childSections.Count;
            _childSections.Add(track);
            _childSections.RaiseListChangedEvents = false;

            //Add new track to Master iPod Playlist
            _iPod.Playlists.GetMasterPlaylist().AddTrack(track, -1, true);
            return track;
        }

        /// <summary>
        /// Deletes a track from the iPod.  The actual file is also deleted.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public bool Remove(Track track)
        {
            _iPod.AssertIsWritable();
            
            //Try and actually remove the file from the iPod drive
            string iPodFileName = Path.Combine(_iPod.DriveLetter, track.FilePath);
            if (_iPod.FileSystem.FileExists(iPodFileName))
            {
                _iPod.FileSystem.DeleteFile(iPodFileName);
            }

            _iPod.ArtworkDB.RemoveArtwork(track);

            _childSections.RaiseListChangedEvents = true;
            _childSections.AllowRemove = true;
            _childSections.Remove(track);
            _iPod.Playlists.RemoveTrackFromAllPlaylists(track);
            _childSections.AllowRemove = false;
            _childSections.RaiseListChangedEvents = false;
            _iPod.Session.DeletedTracks.Add(track);
            _isDirty = true;
            return true;
        }

        /// <summary>
        /// Returns true if this tracklist contains the specified track.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Track item)
        {
            return _childSections.Contains(item);
        }

        /// <summary>
        /// Returns index of specified track, if track doesnt exist, returns -1.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int GetTrackIndex(Track item)
        {
            for (int i = 0; i < _childSections.Count; i++)
            {
                if (_childSections[i] == item)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Number of tracks in this list.
        /// </summary>
        public int Count
        {
            get { return _childSections.Count; }
        }

        /// <summary>
        /// Returns an enumerator for each track in this list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Track> GetEnumerator()
        {
            foreach (Track track in _childSections)
            {
                yield return track;
            }
        }

        /// <summary>
        /// Returns a track with matching Title, artist, album, tracknumber.
        /// If no existing track is found, return null;
        /// </summary>
        /// <param name="newTrack"></param>
        /// <returns></returns>
        private Track GetExistingTrack(NewTrack newTrack)
        {
            foreach (Track existing in _childSections)
            {
                if (existing.Title == newTrack.Title &&
                    existing.Artist == newTrack.Artist &&
                    existing.Album == newTrack.Album &&
                    existing.TrackNumber == newTrack.TrackNumber)
                {
                    return existing;
                }
            }

            return null;
        }

        internal bool IsDirty
        {
            get { return _isDirty; }
        }
    }
}
