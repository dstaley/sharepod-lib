/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib.Databinding;
using SharePodLib.Exceptions;
using System.ComponentModel;

namespace SharePodLib.Parsers.iTunesDB
{

    /// <summary>
    /// Field iTunes uses to sort the playlist.  This is a manual sort, the iPod doesnt do it for us.
    /// Currently SharePodLib doesnt do sorting.
    /// </summary>
    public enum PlaylistSortField
    {
        Unknown = 0,
        Manual = 1,
        Unknown1 = 2,
        Title = 3,
        Album = 4,
        Artist = 5,
        Bitrate = 6,
        Genre = 7,
        Kind = 8,
        DateModified = 9,
        TrackNumber = 10,
        Size = 11,
        Time = 12,
        Year = 13,
        SampleRate = 14,
        Comment = 15,
        DateAdded = 16,
        Equalizer = 17,
        Composer = 18,
        Unknown2 = 19,
        PlayCount = 20,
        LastPlayed = 21,
        DiscNumber = 22,
        Rating = 23,
        ReleaseDate = 24,
        BPM = 25,
        Grouping = 26,
        Category = 27,
        Description = 28,
        Show = 29,
        Season = 30,
        EpisodeNumber = 31
    }

    // Implements a MHYP entry in iTunesDB
    /// <summary>
    /// An iPod Playlist.  There are 4 diffent types of playlists currently. (Master playlist, Smart, Podcast, Standard).
    /// The Master Playlist holds all tracks on the iPod.  
	/// Smart playlists are rebuilt by iTunes based on rules (Artist=Nirvana etc).
    /// </summary>
    public class Playlist : BaseDatabaseElement
    {
        private byte _isMaster;
        private byte[] _unk1;
        private int _timeStamp;
        //private int _unk2;
        private ulong _id;
        private int _unk3;
        private Int16 _stringObjectCount;
        private Int16 _isPodcast;
        private int _sortField;
        List<BaseMHODElement> _dataObjects;
        protected List<PlaylistItem> _playlistItems;

        //Non database fields
        private bool _isSmartPlaylist;
        private DataBoundList<Track> _bindingTrackList;
        private string _lengthSummary;
        private string _sizeSummary;
        protected bool _isDirty;

        internal Playlist(IPod iPod) : this()
        {
            _iPod = iPod;
        }

        //internal Playlist(IPod iPod, bool isMaster)
        //    : this()
        //{
        //    _iPod = iPod;
        //    if (isMaster)
        //    {
        //        _isMaster = 1;
        //    }
        //}

        internal Playlist()
        {
            _headerSize = 108;
            _requiredHeaderSize = 48;
            _identifier = "mhyp".ToCharArray();
            _unk1 = new byte[3];
            _stringObjectCount = 1;
            _dataObjects = new List<BaseMHODElement>();
            _playlistItems = new List<PlaylistItem>();
            _unusedHeader = new byte[_headerSize - _requiredHeaderSize];
            _bindingTrackList = new DataBoundList<Track>();
            _bindingTrackList.AllowRemove = true;

        }

        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhyp");
                        
            _sectionSize = reader.ReadInt32();
            int dataObjectCount = reader.ReadInt32();
            int playlistItemsCount = reader.ReadInt32();
            _isMaster = reader.ReadByte();
            _unk1 = reader.ReadBytes(3);
            _timeStamp = reader.ReadInt32();
            //_unk2 = reader.ReadInt32();
            _id = reader.ReadUInt64();
            _unk3 = reader.ReadInt32();
            _stringObjectCount = reader.ReadInt16();
            _isPodcast = reader.ReadInt16();
            _sortField = reader.ReadInt32();

            this.ReadToHeaderEnd(reader);

            for (int i = 0; i < dataObjectCount; i++)
            {
                BaseMHODElement mhod = MHODFactory.ReadMHOD(iPod, reader);
                //MHODElement mhod = new MHODElement();
                //mhod.Read(_iPod, reader);
                if (mhod.Type == MHODElementType.SmartPlaylistRule ||
                    mhod.Type == MHODElementType.SmartPlaylistData)
                {
                    _isSmartPlaylist = true;
                }
                
                _dataObjects.Add(mhod);
                
            }

            for (int i = 0; i < playlistItemsCount; i++)
            {
                PlaylistItem mhip = new PlaylistItem();
                mhip.Read(iPod, reader);
                _playlistItems.Add(mhip);
            }
        }

        internal override void Write(BinaryWriter writer)
        {
            _sectionSize = GetSectionSize();

            writer.Write(_identifier);
            writer.Write(_headerSize);
            writer.Write(_sectionSize);
            writer.Write(_dataObjects.Count);
            writer.Write(_playlistItems.Count);
            writer.Write(_isMaster);
            writer.Write(_unk1);
            writer.Write(_timeStamp);
            //writer.Write(_unk2);
            writer.Write(_id);
            writer.Write(_unk3);
            writer.Write(_stringObjectCount);
            writer.Write(_isPodcast);
            writer.Write(_sortField);
            writer.Write(_unusedHeader);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
                _dataObjects[i].Write(writer);
            }
            for (int i = 0; i < _playlistItems.Count; i++)
            {
                _playlistItems[i].Write(writer);
            }
            _isDirty = false;
        }

        internal override int GetSectionSize()
        {
            int size = _headerSize;
            for (int i = 0; i < _dataObjects.Count; i++)
            {
                size += _dataObjects[i].GetSectionSize();
            }
            for (int i = 0; i < _playlistItems.Count; i++)
            {
                size += _playlistItems[i].GetSectionSize();
            }

            return size;
        }

        #endregion
                
        /// <summary>
        /// Playlist name as it appears on the iPod.
        /// </summary>
        public string Name
        {
            get
            {
                StringMHOD title = GetDataElement(MHODElementType.Title);
                if (title != null) return title.Data;

                if (IsMaster) return "iPod";
                return "Unnamed";
            }
            set
            {
                if (IsPodcastPlaylist)
                    throw new OperationNotAllowedException("Podcast playlists cannot be modified");

                if (string.IsNullOrEmpty(value))
                    throw new OperationNotAllowedException("The playlist name cannot be empty");

                UnicodeMHOD titleElement = (UnicodeMHOD)GetDataElement(MHODElementType.Title);
                if (titleElement != null)
                {
                    titleElement.Data = value;
                }
                else
                {
                    titleElement = new UnicodeMHOD(MHODElementType.Title);
                    titleElement.Data = value;
                    titleElement.Position = 1;
                    _dataObjects.Add(titleElement);
                }
                _isDirty = true;
            }
        }

        /// <summary>
        /// True if this playlist is the Master playlist.
        /// </summary>
        public bool IsMaster
        {
            get { return _isMaster == 1; }
        }

        /// <summary>
        /// True if this playlist is a Smart playlist.
        /// </summary>
        public bool IsSmartPlaylist
        {
            get { return _isSmartPlaylist; }
        }

        /// <summary>
        /// True if this playlist is a Podcast playlist.
        /// </summary>
        public bool IsPodcastPlaylist
        {
            get { return _isPodcast == 1; }
            set
            {
                if (value)
                    _isPodcast = 1;
                else
                    _isPodcast = 0;
            }
        }


        public Track this[int index]
        {
            get { return _playlistItems[index].Track; }
        }

        internal PlaylistItem GetPlaylistItem(int index)
        {
            return _playlistItems[index];
        }

        internal ulong Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Field iTunes uses to sort the playlist.  This is a manual sort, the iPod doesnt do it for us.
        /// </summary>
        public PlaylistSortField SortField
        {
            get
            {
                try
                {
                    PlaylistSortField sortField = (PlaylistSortField)_sortField;
                    return sortField;
                }
                catch (Exception ex)
                {
                    throw new UnknownSortOrderException(_sortField + " is not a supported Sort Order type.");
                }
            }
            set
            {
                _sortField = (int)value;
                _isDirty = true;
            }
        }
                

        internal void ResolveTracks(IPod iPod)
        {
            _iPod = iPod;
            
            for (int i = 0; i < _playlistItems.Count; i++)
            {
                _playlistItems[i].ResolveTrack(iPod);
                if (_playlistItems[i].Track != null)
                {
                    if (!_bindingTrackList.Contains(_playlistItems[i].Track))
                    {
                        _bindingTrackList.Add(_playlistItems[i].Track);
                    }
                }
            }
        }

        internal void UpdateSummaryData()
        {
            long _totalSize = 0;
            long _totalLength = 0;
            foreach (Track track in Tracks)
            {
                _totalSize += track.FileSize.ByteCount;
                _totalLength += track.Length.Seconds;
            }

            _lengthSummary = Helpers.GetTimeString(_totalLength);
            _sizeSummary = Helpers.GetFileSizeString(_totalSize, 0);
        }

        private StringMHOD GetDataElement(int type)
        {
            for (int i = 0; i < _dataObjects.Count; i++)
            {
                if (_dataObjects[i] is StringMHOD)
                {
                    if (_dataObjects[i].Type == type)
                    {
                        return (StringMHOD)_dataObjects[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Databound list of tracks in this playlist.  Tracks shouldnt be added/removed through this list (instead you should go through the normal
        /// iPod.Playlists[1].Tracks.Add(track), but its useful to bind to a DataGridView or other control which supports databinding
        /// </summary>
        public DataBoundList<Track> BindingTrackList
        {
            get { return _bindingTrackList; }
        }

        /// <summary>
        /// How many tracks are in this playlist.
        /// </summary>
        public int TrackCount
        {
            get { return _bindingTrackList.Count; }
        }

        /// <summary>
        /// How many playlist items are in this playlist
        /// </summary>
        internal int ItemCount
        {
            get { return _playlistItems.Count; }
        }

        /// <summary>
        /// Enumerates each track in this playlist.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Track> Tracks
        {
            get
            {
                foreach (PlaylistItem item in _playlistItems)
                {
                    if (item.Track != null)
                        yield return item.Track;
                }
            }
        }


        internal IEnumerable<PlaylistItem> Items()
        {
            foreach (PlaylistItem item in _playlistItems)
            {
                yield return item;
            }
        }
        /// <summary>
        /// Returns true if this playlist contains the specified track.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public bool ContainsTrack(Track track)
        {
            foreach (Track t in Tracks)
            {
                if (t == track)
                    return true;
            }
            return false;
        }

        internal bool ContainsTrack(long trackDBId)
        {
            foreach (Track t in Tracks)
            {
                if (t.DBId == trackDBId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the total length of tracks in this playlist as a hh:mm:ss string.
        /// </summary>
        public string LengthSummary
        {
            get { return _lengthSummary; }
        }

        /// <summary>
        /// Returns the total size of tracks in this playlist as Mb or Gb (whichever is more appropriate).
        /// </summary>
        public string SizeSummary
        {
            get { return _sizeSummary; }
        }


        internal void AddItem(PlaylistItem item, int position)
        {
            if (position < 0)
            {
                item.PlaylistPosition = _playlistItems.Count + 1;
                _playlistItems.Add(item);
            }
            else
            {
                item.PlaylistPosition = position;
                _playlistItems.Insert(position, item);
            }
        }

        /// <summary>
        /// Add the specifed track to the end of this playlist.  If the track already exists it won't be added twice.
        /// </summary>
        /// <param name="track"></param>
        public void AddTrack(Track track)
        {
            _iPod.AssertIsWritable();
            AddTrack(track, -1, false);
        }

        /// <summary>
        /// Add the specifed track to this playlist at the specified position.  If the track already exists it won't be added twice.
        /// </summary>
        public void AddTrack(Track track, int position)
        {
            _iPod.AssertIsWritable();
            AddTrack(track, position, false);
        }

        internal virtual void AddTrack(Track track, int position, bool skipChecks)
        {
            if (!skipChecks)
                AssertModificationRights();


            if (ContainsTrack(track))
                return;

            PlaylistItem item = new PlaylistItem();
            item.Track = track;
            
            if (IsPodcastPlaylist)
            {
                track.RememberPlaybackPosition = true;
                track.PodcastFlag = true;
            }

            _bindingTrackList.RaiseListChangedEvents = true;
            if (position < 0)
            {
                item.PlaylistPosition = _playlistItems.Count + 1;
                _playlistItems.Add(item);
                _bindingTrackList.Add(track);
            }
            else
            {
                item.PlaylistPosition = position;
                _playlistItems.Insert(position, item);
                _bindingTrackList.Insert(position, track);
            }
            
            _bindingTrackList.RaiseListChangedEvents = false;

            UpdateSummaryData();
            _isDirty = true;
        }

        /// <summary>
        /// Remove the specified track from this playlist.  This doesn't remove the track from the iPod (see TrackList.Remove())
        /// </summary>
        /// <param name="track"></param>
        public void RemoveTrack(Track track)
        {
            _iPod.AssertIsWritable();
            RemoveTrack(track, false);
        }

        //Use with caution (only internally)
        //Used when deleting a song (must be removed from the Master playlist)
        internal void RemoveTrack(Track track, bool skipChecks)
        {
            if (!skipChecks)
                AssertModificationRights();

            foreach (PlaylistItem item in _playlistItems)
            {
                if (item.Track == track)
                {
                    _playlistItems.Remove(item);
                    break;
                }
            }

            _bindingTrackList.RaiseListChangedEvents = true;
            _bindingTrackList.Remove(track);
            _bindingTrackList.RaiseListChangedEvents = false;

            UpdateSummaryData();
            _isDirty = true;
        }

        internal void RemoveAllItems()
        {
            _playlistItems.Clear();
            _bindingTrackList.Clear();
        }

        internal void RemoveItem(PlaylistItem item)
        {
            _playlistItems.Remove(item);
        }

        /// <summary>
        /// Move the specified track to the specified position in the playlist.
        /// </summary>
        public void MoveTrackToPosition(Track track, int newPosition)
        {
            this.RemoveTrack(track);
            this.AddTrack(track, newPosition);
        }

        internal void AssertModificationRights()
        {
            if (_isSmartPlaylist)
                throw new OperationNotAllowedException("You cannot change tracks in Smart Playlists");
            if (_isMaster == 1)
                throw new OperationNotAllowedException("You cannot change tracks in the Master playlist");
         }

        /// <summary>
        /// Create the index (fast lookup table) the iPod uses for Artist, Album, Genre menus.
        /// </summary>
        internal void ReIndex()
        {
            if (IsMaster)
            {
                //MenuIndexMHOD orig;

                for (int i = _dataObjects.Count - 1; i >= 0; i--)
                {
                    if (_dataObjects[i].Type == MHODElementType.MenuIndexTable ||
                        _dataObjects[i].Type == MHODElementType.LetterJumpTable)
                    {
                        //if (_dataObjects[i] is MenuIndexMHOD && ((MenuIndexMHOD)_dataObjects[i]).IndexType == 3)
                        //    orig = (MenuIndexMHOD)_dataObjects[i];

                        _dataObjects.RemoveAt(i);
                    }
                }

                List<Track> tracks = new List<Track>(_iPod.Tracks.BindingList);
                foreach (Track track in tracks)
                    track.ReIndex();

                // In some strange cases, the iPod will use the following indexes but also suplement
                // it with its own sorting - causing the onscreen menus to be a mixture of sorted/non-sorted tracks.
                // Commented out till figure it out.  Performance impact is minimal.

                //MenuIndexMHOD index = new MenuIndexMHOD(MenuIndexType.Title);
                //index.ReIndex(tracks);
                //_dataObjects.Add(index);
                //MenuIndexMHOD index = new MenuIndexMHOD(MenuIndexType.Album_Track_Title);
                //index.ReIndex(tracks);
                //_dataObjects.Add(index);
                //index = new MenuIndexMHOD(MenuIndexType.Artist_Album_Track_Title);
                //index.ReIndex(tracks);
                //_dataObjects.Add(index);
                //index = new MenuIndexMHOD(MenuIndexType.Genre_Artist_Album_Track_Title);
                //index.ReIndex(tracks);
                //_dataObjects.Add(index);
            }
        }
                

        internal bool IsDirty
        {
            get { return _isDirty; }
        }
    }
}
