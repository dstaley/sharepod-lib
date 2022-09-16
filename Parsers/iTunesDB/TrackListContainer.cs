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
    /// Implements a type 1 (Tracks list) MHSD entry in iTunesDB
    /// </summary>
    class TrackListContainer : BaseDatabaseElement
    {
        private ListContainerHeader _header;
        TrackList _childSection;

        public TrackListContainer(ListContainerHeader parent)
        {
            _header = parent;
        }

        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _childSection = new TrackList();
            _childSection.Read(iPod, reader);
        }

        internal override void Write(BinaryWriter writer)
        {
            _childSection.Write(writer);
        }

        internal override int GetSectionSize()
        {
            return _header.HeaderSize + _childSection.GetSectionSize();
        }

        #endregion

        internal TrackList GetTrackList()
        {
            return _childSection;
        }
    }
}
