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
    // Implements a MHLP entry in iTunesDB
    /// <summary>
    /// Class holding all playlists on the iPod
    /// </summary>
    public class PlaylistList : BaseDatabaseElement
    {
        
        private int _dataObjectCount;
        //private List<Playlist> _childSections;
        private DataBoundList<Playlist> _childSections;
        private bool _isDirty;


        internal PlaylistList()
        {
            _requiredHeaderSize = 12;
            _childSections = new DataBoundList<Playlist>();
        }

        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhlp");
                        
            _dataObjectCount = reader.ReadInt32();

            this.ReadToHeaderEnd(reader);
            
            for (int i = 0; i < _dataObjectCount; i++)
            {
                Playlist playlist = new Playlist();
                playlist.Read(iPod, reader);
                _childSections.Add(playlist);
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


        internal void ResolveTracks()
        {
            foreach (Playlist p in _childSections)
            {
                p.ResolveTracks(_iPod);
                p.UpdateSummaryData();
            }
        }

        internal void RemoveTrackFromAllPlaylists(Track track)
        {
            foreach (Playlist p in _childSections)
            {
                p.RemoveTrack(track, true);
            }
        }

        public Playlist this[int index]
        {
            get { return _childSections[index]; }
        }

        /// <summary>
        /// Returns the playlist with the given name.  If no playlists match, returns null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Playlist GetPlaylistByName(string name)
        {
            foreach (Playlist p in _childSections)
            {
                if (p.Name == name)
                    return p;
            }
            return null;
        }

        internal Playlist GetPlaylistById(ulong id)
        {
            foreach (Playlist pl in _childSections)
            {
                if (pl.Id == id)
                    return pl;
            }
            return null;
        }

        internal Playlist GetMasterPlaylist()
        {
            foreach (Playlist p in _childSections)
            {
                if (p.IsMaster)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// Adds a new playlist with the specified name and returns the new playlist.
        /// If there is already a playlist with the same name OperationNotAllowedException is thrown
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns>Playlist</returns>
        public Playlist Add(string playlistName)
        {
            _iPod.AssertIsWritable();

            if (GetPlaylistByName(playlistName) != null)
                throw new OperationNotAllowedException("There is already a playlist called '" + playlistName + "' on your iPod");

            Playlist newPlaylist = new Playlist(_iPod);
            newPlaylist.Name = playlistName;
            if (playlistName == "Podcasts")
            {
                newPlaylist.IsPodcastPlaylist = true;
            }
            newPlaylist.Id = (ulong)new Random().Next();
            newPlaylist.UpdateSummaryData();
            _childSections.Add(newPlaylist);

            _childSections.RaiseListChangedEvents = true;
            _childSections.ApplyCustomSort(new PlaylistSorter());  //sort playlists by name
            
            //_childSections.ApplySort(
            _childSections.RaiseListChangedEvents = false;
            _isDirty = true;
            return newPlaylist;
        }

        /// <summary>
        /// Databound list of playlists on the iPod.  Playlists shouldnt be added/removed through this list (instead you should go through the normal
        /// iPod.Playlists.Add("name"), but its useful to bind to a DataGridView or other control which supports databinding
        /// </summary>
        public DataBoundList<Playlist> BindingList
        {
            get { return _childSections; }
        }

        /// <summary>
        /// Returns true if the iPod contains the specified playlist.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Playlist item)
        {
            return _childSections.Contains(item);
        }

        /// <summary>
        /// How many playlists are on the iPod.
        /// </summary>
        public int Count
        {
            get { return _childSections.Count; }
        }

        /// <summary>
        /// Remove a playlist from the iPod, optionally deleting contained tracks at the same time.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="deleteTracks"></param>
        /// <returns></returns>
        public bool Remove(Playlist item, bool deleteTracks)
        {
            return Remove(item, deleteTracks, false);
        }

        internal bool Remove(Playlist item, bool deleteTracks, bool skipChecks)
        {
            _iPod.AssertIsWritable();

            if (!skipChecks)
            {
                if (item.IsMaster)
                    throw new OperationNotAllowedException("You cannot remove the Master Playlist");

                //if (item.IsPodcastPlaylist)
                //    throw new OperationNotAllowedException("You cannot remove Podcast Playlists");
            }

            if (deleteTracks)
            {
                List<Track> temp = new List<Track>();
                foreach (Track track in item.Tracks)
                {
                    temp.Add(track);
                }
                foreach (Track track in temp)
                {
                    _iPod.Tracks.Remove(track);
                }
            }

            _childSections.RaiseListChangedEvents = true;
            _childSections.AllowRemove = true;
            _childSections.Remove(item);
            _childSections.AllowRemove = false;
            _childSections.RaiseListChangedEvents = false;
            _iPod.Session.DeletedPlaylists.Add(item);
            _isDirty = true;

            return true;
        }


        public IEnumerator<Playlist> GetEnumerator()
        {
            foreach (Playlist playlist in _childSections)
            {
                yield return playlist;
            }
        }

        /// <summary>
        /// To deal with different versions of iPods, there are 2 seperate versions of the Playlists list. (1 with Podcasts as a normal playlist,
        /// the other with Podcasts as a special list.  SharePod sync's the 2 versions here.
        /// </summary>
        /// <param name="otherList"></param>
        internal void FollowChanges(PlaylistList otherList)
        {
            //Add new playlists from otherList to me
            foreach (Playlist otherPlaylist in otherList)
            {
                if (this.GetPlaylistById(otherPlaylist.Id) == null)
                {
                    _childSections.Add(otherPlaylist);
                }
            }

            for (int count = _childSections.Count - 1; count >= 0; count--)
            {
                Playlist thisPlaylist = _childSections[count];
                Playlist otherPlaylist = otherList.GetPlaylistById(thisPlaylist.Id);
                
                if (otherPlaylist == null)
                {
                    this.Remove(thisPlaylist, false, true);
                }
                else
                {
                    if (!thisPlaylist.IsPodcastPlaylist)
                    {
                        _childSections[count] = otherPlaylist;
                    }
                    else
                    {
                        thisPlaylist.ResolveTracks(_iPod);
                        PodcastListAdapter podcastsAdapter = new PodcastListAdapter(_iPod, thisPlaylist);
                        podcastsAdapter.FollowChanges(otherPlaylist);
                    }
                }
            }
            _childSections.ApplyCustomSort(new PlaylistSorter());
        }

        internal bool IsDirty
        {
            get { return _isDirty; }
        }
    }
}
