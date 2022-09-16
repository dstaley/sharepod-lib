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
    // Implements a MHLF entry in ArtworkDB
    /// <summary>
    /// List of ithmb artwork files
    /// </summary>
    class IThmbFileList : BaseDatabaseElement
    {
        private List<IThmbFile> _childSections;

        public IThmbFileList()
        {
            _requiredHeaderSize = 12;
            _childSections = new List<IThmbFile>();
        }

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhlf");

            int fileCount = reader.ReadInt32();

            this.ReadToHeaderEnd(reader);

            for (int i = 0; i < fileCount; i++)
            {
                IThmbFile file = new IThmbFile();
                file.Read(iPod, reader);
                _childSections.Add(file);
            }

        }

        internal override void Write(BinaryWriter writer)
        {
            _sectionSize = GetSectionSize();

            writer.Write(_identifier);
            writer.Write(_headerSize);
            writer.Write(_childSections.Count);
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

        /// <summary>
        /// Enumerates each IThmbFile in this list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IThmbFile> Files()
        {
            foreach (IThmbFile file in _childSections)
            {
                yield return file;
            }
        }

        internal void AddIThmbFile(IPodImageFormat format)
        {
            IThmbFile newFile = new IThmbFile();
            newFile.Create(format.ImageSize, format.FormatId);
            _childSections.Add(newFile);
        }
    }
}
