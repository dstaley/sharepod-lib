/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Databinding;
using System.IO;
using SharePodLib.Exceptions;
using System.Drawing;

namespace SharePodLib.Parsers.Artwork
{
    // Implements a MHLA entry in ArtworkDB
    /// <summary>
    /// List of image albums
    /// </summary>
    public class ImageAlbumList : BaseDatabaseElement
    {
        private List<ImageAlbum> _childSections;

        public ImageAlbumList()
        {
            _requiredHeaderSize = 12;
            _childSections = new List<ImageAlbum>();
        }

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhla");

            int childCount = reader.ReadInt32();

            this.ReadToHeaderEnd(reader);

            for (int i = 0; i < childCount; i++)
            {
                ImageAlbum album = new ImageAlbum();
                album.Read(iPod, reader);
                _childSections.Add(album);
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

        public List<ImageAlbum> Albums
        {
            get
            {
                return _childSections;
            }
        }

        internal void ResolveImages(ImageList images)
        {
            foreach (ImageAlbum album in _childSections)
            {
                album.ResolveImages(images);
            }
        }
    }
}
