/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Parsers.PlayCounts
{
    class Entry : BaseDatabaseElement
    {
        private int _playCount;
        private DateTime _lastPlayed;
        private int _bookmarkPosition;
        private int _rating;


        public Entry(int entrySize)
        {
            _headerSize = entrySize;
            _requiredHeaderSize = 16;
        }

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);

            _playCount = reader.ReadInt32();
            _lastPlayed = Helpers.GetDateTimeFromTimeStamp(reader.ReadUInt32());
            _bookmarkPosition = reader.ReadInt32();
            _rating = reader.ReadInt32();
            
            ReadToHeaderEnd(reader);
        }

        internal override void Write(BinaryWriter writer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal override int GetSectionSize()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal int PlayCount
        {
            get { return _playCount; }
        }

        internal DateTime DateLastPlayed
        {
            get { return _lastPlayed; }
        }

        internal int Rating
        {
            get { return _rating / 20; }
        }
    }
}
