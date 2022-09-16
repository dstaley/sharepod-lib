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
    class PlaylistPositionMHOD : BaseMHODElement
    {
        private byte[] _byteData;
        protected int _position;

        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public PlaylistPositionMHOD()
            : base()
        {
            _type = MHODElementType.PlaylistPosition;
        }

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            if (_sectionSize == _headerSize)
            {
            }
            else
            {
                _position = reader.ReadInt32();
                _byteData = reader.ReadBytes(_sectionSize - (_headerSize + 4));
            }
        }

        public void Create()
        {
            _byteData = new byte[16];
        }

        internal override void Write(BinaryWriter writer)
        {
            if (writer.BaseStream.Position == 1192046)
            {
            }
            base.Write(writer);
            writer.Write(_position);
            if (_byteData != null)
                writer.Write(_byteData);
        }

        internal override int GetSectionSize()
        {
            int size = _headerSize + 4;
            if (_byteData != null)
                size += _byteData.Length;

            return size;
        }
    }
}
