/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib.Parsers.iTunesDB
{
    /// <summary>
    /// Writes the necessary entries for the Podcast playlist in the Podcast MHSD section.
    /// </summary>
    internal class PodcastListAdapter
    {
        private Playlist _playlist;
        private IPod _iPod;

        public PodcastListAdapter(IPod iPod, Playlist playlist)
        {
            _iPod = iPod;
            _playlist = playlist;
            _playlist.IsPodcastPlaylist = true;
        }

        public void AddTrack(Track track)
        {
            PlaylistItem trackItem = null;
            foreach (PlaylistItem item in _playlist.Items())
            {
                if (item.Track == track)
                {
                    trackItem = item;
                    break;
                }
            }
            //If the track is in the list and has a valid group, dont need to do anything else.
            if (trackItem != null && trackItem.GroupId != 0)
            {
                return;
            }
            
            PlaylistItem parentItem = GetPodcastGroup(track.Album);
            
            if (trackItem == null)
            {
                trackItem = new PlaylistItem();
                trackItem.Track = track;
                _playlist.AddItem(trackItem,-1);
            }
            trackItem.PodcastGroupParentId = parentItem.GroupId;
            trackItem.GroupId = _iPod.IdGenerator.GetNewPodcastGroupId();
        }

        public void RemoveItem(PlaylistItem item)
        {
            _playlist.RemoveItem(item);
            PlaylistItem parentItem = GetPodcastGroup(item.PodcastGroupParentId);
            if (parentItem != null)
            {
                if (!PodcastGroupHasEntries(parentItem))
                    _playlist.RemoveItem(parentItem);
            }
        }

        private PlaylistItem GetPodcastGroup(string groupName)
        {
            foreach (PlaylistItem item in _playlist.Items())
            {
                if (item.IsPodcastGroup)
                    if (item.PodcastGroupTitle == groupName)
                    {
                        return item;
                    }
            }
            return CreatePodcastGroup(groupName);
        }

        
        private PlaylistItem CreatePodcastGroup(string groupName)
        {
            PlaylistItem item = new PlaylistItem();
            item.PodcastGroupTitle = groupName;
            item.IsPodcastGroup = true;
            item.PodcastGroupParentId = 0;
            item.GroupId = _iPod.IdGenerator.GetNewPodcastGroupId();
            _playlist.AddItem(item, 0);
            return item;
        }

        private bool PodcastGroupHasEntries(PlaylistItem podcastGroup)
        {
            foreach (PlaylistItem item in _playlist.Items())
            {
                if (item.PodcastGroupParentId == podcastGroup.GroupId)
                    return true;
            }
            return false;
        }

        private PlaylistItem GetPodcastGroup(int groupId)
        {
            foreach (PlaylistItem item in _playlist.Items())
            {
                if (item.GroupId == groupId)
                    return item;
            }
            return null;
        }

        internal void FollowChanges(Playlist otherPlaylist)
        {
            for (int count = 0; count < otherPlaylist.ItemCount; count++)
            {
                if (otherPlaylist[count] != null)
                {
                    AddTrack(otherPlaylist[count]);
                }
            }

            for (int count = _playlist.ItemCount - 1; count >= 0; count--)
            {
                if (_playlist.ItemCount == 0)
                    break;
                if (!_playlist.GetPlaylistItem(count).IsPodcastGroup)
                {
                    if (!otherPlaylist.ContainsTrack(_playlist[count]))
                    {
                        RemoveItem(_playlist.GetPlaylistItem(count));
                        count = _playlist.ItemCount; //restart from top again
                    }
                }
            }
        }
    }
}
