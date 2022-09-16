using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SharePodLib.Parsers.iTunesDB
{
    /// <summary>
    /// Implements a type 5 MHSD entry in iTunesDB
    /// </summary>
    class Type5Container : BaseDatabaseElement
    {
        private ListContainerHeader _header;
        private PlaylistList _childSection;

        public Type5Container(ListContainerHeader parent)
        {
            _header = parent;
        }

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _childSection = new PlaylistList();
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

        internal PlaylistList GetPlaylistsList()
        {
            return _childSection;
        }
    }
}
