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
    enum StringEncodingType
    {
        Ascii = 0,
        UTF8 = 1,
        Unicode = 2
    }

    /// <summary>
    /// Implements an MHOD used in the ArtworkDB and PhotoDB
    /// </summary>
    class ArtworkStringMHOD : StringMHOD
    {
        private UInt16 _unk1x;
        private int _padding;
        private StringEncodingType _stringType;
        private int _unk3;
        private string _data;

        private int _actualPadding;


        internal ArtworkStringMHOD()
        {
            _requiredHeaderSize = 24;

            _headerSize = 24;
            _identifier = "mhod".ToCharArray();
            _type = MHODElementType.Album; //3
            _stringType = StringEncodingType.Unicode;
        }


        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            long startOfElement = reader.BaseStream.Position;

            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhod");

            _sectionSize = reader.ReadInt32();
            _type = reader.ReadUInt16();
            _unk1x = reader.ReadUInt16();

            _unk2 = reader.ReadInt32();
            _padding = reader.ReadInt32();

            ReadToHeaderEnd(reader);

            int dataLength = reader.ReadInt32();
            _stringType = (StringEncodingType)reader.ReadInt32();
            _unk3 = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(dataLength);

            _data = StringEncoding.GetString(bytes);

            _actualPadding = (int)((startOfElement + _sectionSize) - reader.BaseStream.Position);
            //Jump over padding section
            if (_actualPadding != 0)
                reader.BaseStream.Seek(startOfElement + _sectionSize, SeekOrigin.Begin);
        }


        internal override void Write(BinaryWriter writer)
        {
            _sectionSize = GetSectionSize();

            byte[] bytes = StringEncoding.GetBytes(_data);

            writer.Write(_identifier);
            writer.Write(_headerSize);
            writer.Write(_sectionSize);
            writer.Write((UInt16)_type);
            writer.Write(_unk1x);
            writer.Write(_unk2);
            writer.Write(_padding);
            writer.Write(_unusedHeader);
            writer.Write(bytes.Length);
            writer.Write((int)_stringType);
            writer.Write(_unk3);
            writer.Write(bytes);

            //Jump over padding section
            writer.BaseStream.Seek(_actualPadding, SeekOrigin.Current);
        }


        internal override int GetSectionSize()
        {
            int dataLength = StringEncoding.GetByteCount(_data);
            return _headerSize + 12 + dataLength + _actualPadding;
        }

        #endregion

        public override string Data
        {
            get { return _data; }
            set
            {
                if (!value.StartsWith(":"))
                    _data = ":" + value;
                else
                    _data = value;
            }
        }

        internal void Create(string data)
        {
            _unusedHeader = new byte[_headerSize - _requiredHeaderSize];
            Data = data;
        }


        private Encoding StringEncoding
        {
            get
            {
                if (_stringType == StringEncodingType.Unicode)
                    return Encoding.Unicode;
                else if (_stringType == StringEncodingType.Ascii)
                    return Encoding.ASCII;
                else
                    return Encoding.UTF8;
            }
        }
    }
}
