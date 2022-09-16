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
    /// <summary>
    /// Implements an MHBD section in the iTunesDB file
    /// </summary>
    public class iTunesDBRoot : BaseDatabaseElement
    {
        protected int _unk1;
        protected int _versionNumber;
        protected int _listContainerCount;
        protected UInt64 _id;
        protected byte[] _unk2;
        protected Int16 _hashingScheme;

        internal List<ListContainerHeader> _childSections;

        public int VersionNumber
        {
            get { return _versionNumber; }
        }

        internal int HashingScheme
        {
            get { return _hashingScheme; }
        }


        public iTunesDBRoot()
        {
            _requiredHeaderSize = 50;
            _childSections = new List<ListContainerHeader>();
        }

        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhbd");

            _sectionSize = reader.ReadInt32();
            _unk1 = reader.ReadInt32();
            _versionNumber = reader.ReadInt32();
            _listContainerCount = reader.ReadInt32();
            _id = reader.ReadUInt64();
            _unk2 = reader.ReadBytes(16);
            _hashingScheme = reader.ReadInt16();

            this.ReadToHeaderEnd(reader);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                ListContainerHeader containerHeader = new ListContainerHeader();
                containerHeader.Read(iPod, reader);
                _childSections.Add(containerHeader);
            }
        }

        internal override void Write(BinaryWriter writer)
        {
            _sectionSize = GetSectionSize();

            writer.Write(_identifier);
            writer.Write(_headerSize);
            writer.Write(_sectionSize);
            writer.Write(_unk1);
            writer.Write(_versionNumber);
            writer.Write(_listContainerCount);  
            //really this should be _childSections.Count, but have observed some 
            //dbs with wrong count from iTunes, updating it means we fail compatibility test with SourceDoesntMatchOutput

            writer.Write(_id);
            writer.Write(_unk2);
            writer.Write(_hashingScheme);
            writer.Write(_unusedHeader);

            for (int i = 0; i < _childSections.Count; i++)
            {
                _childSections[i].Write(writer);
            }
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

        internal ListContainerHeader GetChildSection(MHSDSectionType type)
        {
            for (int i = 0; i < _childSections.Count; i++)
            {
                if (_childSections[i].Type == type)
                    return _childSections[i];
            }
            return null;
        }

        /// <summary>
        /// Some iPods do not have a Playlists section.  We create one here, and create a master
        /// playlist containing all tracks.
        /// </summary>
        /// <param name="trackList"></param>
        //internal void CreatePlaylistContainer(TrackList trackList)
        //{
        //    if (GetChildSection(MHSDSectionType.Playlists) != null)
        //        return;

        //    PlaylistList playlistList = new PlaylistList(_iPod);
        //    Playlist master = new Playlist(_iPod, true);
        //    foreach (Track t in trackList)
        //    {
        //        master.AddTrack(t, -1, true);
        //    }
        //    playlistList.Add(master);
        //    ListContainerHeader header = new ListContainerHeader(MHSDSectionType.Playlists);
        //    PlaylistListContainer listContainer = new PlaylistListContainer(header, playlistList);
        //    header.SetChildSection(listContainer);
        //    _childSections.Add(header);
        //}

        /// <summary>
        /// Gets the PlaylistList, or PlaylistV2 if Playlist container doesn't exist
        /// </summary>
        /// <returns></returns>
        public PlaylistList GetPlaylistList()
        {
            if (GetChildSection(MHSDSectionType.Playlists) != null)
            {
                PlaylistListContainer playlistsContainer = (PlaylistListContainer)GetChildSection(MHSDSectionType.Playlists).GetListContainer();
                return playlistsContainer.GetPlaylistsList();
            }

            if (GetChildSection(MHSDSectionType.PlaylistsV2) != null)
            {
                PlaylistListV2Container playlistsContainer = (PlaylistListV2Container)GetChildSection(MHSDSectionType.PlaylistsV2).GetListContainer();
                return playlistsContainer.GetPlaylistsList();
            }

            throw new Exception("iTunesDB Playlist container not found");
        }
    }
}
