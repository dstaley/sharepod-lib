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
    class UnknownMHOD : BaseMHODElement
    {
        private byte[] _byteData;

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            _byteData = reader.ReadBytes(_sectionSize - _headerSize);
        }

        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(_byteData);
        }

        internal override int GetSectionSize()
        {
            int size = _headerSize + _byteData.Length;
            
            return size;
        }
    }
}
