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
    /// Implements a type 4 (Album list) MHSD entry in iTunesDB
    /// </summary>
    class AlbumListContainer : BaseDatabaseElement
    {
        private ListContainerHeader _header;
        private byte[] _unk1;

        public AlbumListContainer(ListContainerHeader parent)
        {
            _header = parent;
        }

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            int length = _header.SectionSize - _header.HeaderSize;
            _unk1 = reader.ReadBytes(length);
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(_unk1);
        }

        internal override int GetSectionSize()
        {
            return _header.SectionSize;
        }
    }
}
