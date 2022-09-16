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
    /// Implements a type 2 (Image album list) MHSD entry in ArtworkDB / PhotoDB
    /// </summary>
    class ImageAlbumListContainer : BaseDatabaseElement
    {
        private ListContainerHeader _header;
        ImageAlbumList _childSection;

        public ImageAlbumListContainer(ListContainerHeader parent)
        {
            _header = parent;
        }

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _childSection = new ImageAlbumList();
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

        internal ImageAlbumList ImageAlbumList
        {
            get { return _childSection; }
        }
    }
}
