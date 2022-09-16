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
    class Header : BaseDatabaseElement
    {
       
        int _entrySize;
        int _nbrEntries;
        List<Entry> _entries;


        public Header()
        {
            _requiredHeaderSize = 16;
            _entries = new List<Entry>(); // new List<Track>();
        }
        
        
        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhdp");

            _entrySize = reader.ReadInt32();
            _nbrEntries = reader.ReadInt32();

            this.ReadToHeaderEnd(reader);


            for (int i = 0; i < _nbrEntries; i++)
            {
                Entry entry = new Entry(_entrySize);
                entry.Read(iPod, reader);
                _entries.Add(entry);
            }
        }

        internal override void Write(BinaryWriter writer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal override int GetSectionSize()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEnumerable<Entry> Entries()
        {
            foreach (Entry e in _entries)
            {
                yield return e;
            }
        }

        public int EntryCount
        {
            get
            {
                return _nbrEntries;
            }
        }
    }
}
