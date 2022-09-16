/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SharePodLib.Parsers.iTunesDB
{
    internal class IdGenerator
    {

        private int _lastTrackId;
        private Int64 _lastDBId;
        private int _lastPodcastGroupId = 1;

        private IPod _iPod;


        public IdGenerator(IPod iPod)
        {
            _iPod = iPod;

            _lastTrackId = 0;

            for (int i = 0; i < _iPod.Tracks.Count; i++)
            {
                if (_iPod.Tracks[i].Id > _lastTrackId)
                    _lastTrackId = _iPod.Tracks[i].Id;

                if (_iPod.Tracks[i].DBId > _lastDBId)
                    _lastDBId = _iPod.Tracks[i].DBId;
            }

            ListContainerHeader listHeader = _iPod.ITunesDB.DatabaseRoot.GetChildSection(MHSDSectionType.PlaylistsV2);
            if (listHeader != null)
            {
                PlaylistListV2Container podcastsContainer = (PlaylistListV2Container)listHeader.GetListContainer();
                PlaylistList podcastsList = podcastsContainer.GetPlaylistsList();

                Playlist podcastsPlaylist = podcastsList.GetPlaylistByName("Podcasts");
                if (podcastsPlaylist != null)
                {
                    foreach (PlaylistItem item in podcastsPlaylist.Items())
                    {
                        if (item.GroupId > _lastPodcastGroupId)
                            _lastPodcastGroupId = item.GroupId;
                    }
                }
            }
        }

        public int GetNewTrackId()
        {
            _lastTrackId++;
            return _lastTrackId;
        }

        public Int64 GetNewDBId()
        {
            _lastDBId++;
            return _lastDBId;
        }

        public string GetNewIPodFilePath(Track track, string fileExtension)
        {            
            Random r = new Random();
            string folderNumber = "F" + r.Next(49).ToString("00");

            string path;
            while (true)
            {
                if (track.MediaType == MediaType.Ringtone)
                {
                    path = Path.Combine(_iPod.FileSystem.IPodControlPath, "Ringtones");
                }
                else
                {
                    path = Path.Combine(Path.Combine(_iPod.FileSystem.IPodControlPath, "Music"), folderNumber);
                }
                path = Path.Combine(path, GetNewRandomFileName() + fileExtension);
                if (!_iPod.FileSystem.FileExists(path))
                    break;
            }
            
            return path.Substring(_iPod.DriveLetter.Length);
        }

        private string GetNewRandomFileName()
        {
            string path = "";
            Random r = new Random();
            for (int i = 0; i < 4; i++)
            {
                char c = (char)r.Next(65, 90);
                string s = Convert.ToString(c);
                path += s;
            }
            path = "SP" + path;
            return path;
        }

        public uint GetNewArtworkId()
        {
            uint newArtworkId = _iPod.ArtworkDB.NextImageId;
            _iPod.ArtworkDB.NextImageId++;
            return newArtworkId;
        }

        public int GetNewPodcastGroupId()
        {
            _lastPodcastGroupId++;
            return _lastPodcastGroupId;
        }
    }
}
