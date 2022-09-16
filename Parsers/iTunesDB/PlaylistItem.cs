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
    /// Implements a MHIP entry in iTunesDB
    /// </summary>
    public class PlaylistItem : BaseDatabaseElement
    {
        private int _dataObjectCount;
        private int _podcastGroupingFlag;
        private int _groupId;
        private int _trackId;
        private int _timeStamp;
        private int _podcastGroupParent;

        private Track _track;

        List<BaseMHODElement> _childSections;

        internal PlaylistItem()
        {
            _headerSize = 76;
            _requiredHeaderSize = 36;
            _identifier = "mhip".ToCharArray();
            _dataObjectCount = 0;
            _podcastGroupingFlag = 0;
            _groupId = 0;
            _trackId = 0;
            _timeStamp = 0;
            _podcastGroupParent = 0;
            _unusedHeader = new byte[_headerSize - _requiredHeaderSize];
            _childSections = new List<BaseMHODElement>();
            _track = null;
        }

        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhip");
                        
            _sectionSize = reader.ReadInt32();
            _dataObjectCount = reader.ReadInt32();
            _podcastGroupingFlag = reader.ReadInt32();
            _groupId = reader.ReadInt32();
            _trackId = reader.ReadInt32();
            _timeStamp = reader.ReadInt32();
            _podcastGroupParent = reader.ReadInt32();

            this.ReadToHeaderEnd(reader);

            for (int i = 0; i < _dataObjectCount; i++)
            {
                //MHODElement mhod = new MHODElement();
                //mhod.Read(iPod, reader);
                BaseMHODElement mhod = MHODFactory.ReadMHOD(iPod, reader);
                _childSections.Add(mhod);
            }            
        }

        internal override void Write(BinaryWriter writer)
        {
            _sectionSize = GetSectionSize();

            writer.Write(_identifier);
            writer.Write(_headerSize);
            writer.Write(_sectionSize);
            writer.Write(_childSections.Count);
            writer.Write(_podcastGroupingFlag);
            writer.Write(_groupId);
            writer.Write(_trackId);
            writer.Write(_timeStamp);
            writer.Write(_podcastGroupParent);
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

        internal void ResolveTrack(IPod iPod)
        {
            _track = iPod.Tracks.FindById(_trackId);
        }

        public Track Track
        {
            get { return _track; }
            set { 
                _track = value;
                _trackId = _track.Id;
            }
        }

        
        public int PlaylistPosition
        {
            get
            {
                BaseMHODElement mhod = GetDataElement(MHODElementType.PlaylistPosition);
                if (mhod == null)
                    return 0;
                else
                    return ((PlaylistPositionMHOD)mhod).Position;
            }
            set
            {
                SetPlaylistPosition(value);
            }
        }

        private StringMHOD GetDataElement(int type)
        {
            for (int i = 0; i < _childSections.Count; i++)
            {
                if (_childSections[i] is StringMHOD && _childSections[i].Type == type)
                {
                    return (StringMHOD)_childSections[i];
                }
            }
            return null;
        }

        private void SetDataElement(int type, string data)
        {
            StringMHOD mhod = GetDataElement(type);
            if (mhod != null)
            {
                mhod.Data = data;
            }
            else
            {
                StringMHOD newSection = new UnicodeMHOD(type);
                newSection.Data = data;
                _childSections.Add(newSection);                
            }
        }

        private void SetPlaylistPosition(int playlistIndex)
        {
            BaseMHODElement mhod = GetDataElement(MHODElementType.PlaylistPosition);
            if (mhod != null)
            {
                ((PlaylistPositionMHOD)mhod).Position = playlistIndex;
            }
            else
            {
                mhod = new PlaylistPositionMHOD();
                ((PlaylistPositionMHOD)mhod).Create();
                ((PlaylistPositionMHOD)mhod).Position = playlistIndex;
                _childSections.Add(mhod);
            }            
        }

        public bool IsPodcastGroup
        {
            get
            {
                return _podcastGroupingFlag != 0 && _podcastGroupParent == 0 && !String.IsNullOrEmpty(PodcastGroupTitle);
            }
            set
            {
                if (value)
                    _podcastGroupingFlag = 256;
                else
                    throw new NotImplementedException();
            }
        }

        public string PodcastGroupTitle
        {
            get
            {
                StringMHOD element = GetDataElement(MHODElementType.Title);
                if (element == null)
                    return null;
                else
                    return element.Data;
            }
            set
            {
                this.SetDataElement(MHODElementType.Title, value);
            }
        }

        public int PodcastGroupParentId
        {
            get { return _podcastGroupParent; }
            set { _podcastGroupParent = value; }
        }

        public int GroupId
        {
            get { return _groupId; }
            set { _groupId = value; }
        }
    }
}
