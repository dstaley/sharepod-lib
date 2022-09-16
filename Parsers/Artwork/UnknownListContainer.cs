/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SharePodLib.Parsers.Artwork
{
    /// <summary>
    /// Implements any unknown type MHSD entry in iTunesDB
    /// Simply reads to the end of the list, ignoring the contents.
    /// Role is to protect against future iTunesDB changes.
    /// </summary>
    class UnknownListContainer : BaseDatabaseElement
    {
        private ListContainerHeader _header;
        private byte[] _unk1;

        public UnknownListContainer(ListContainerHeader parent)
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
