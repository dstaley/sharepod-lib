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
    class MHODFactory
    {
        public static BaseMHODElement ReadMHOD(IPod iPod, BinaryReader reader)
        {
            BaseMHODElement mhod = new BaseMHODElement();
            mhod.Read(iPod, reader);

            BaseMHODElement newMhod = null;

            switch (mhod.Type)
            {
                case MHODElementType.PodcastFileUrl:
                case MHODElementType.PodcastRSSUrl:
                    newMhod = new UnknownMHOD();
                    break;

                case MHODElementType.Id:
                case MHODElementType.Title:
                case MHODElementType.FilePath:
                case MHODElementType.Album:
                case MHODElementType.Artist:
                case MHODElementType.Genre:
                case MHODElementType.FileType:
                case MHODElementType.Comment:
                case MHODElementType.Composer:
                case MHODElementType.AlbumArtist:
                case MHODElementType.DescriptionText:
                case MHODElementType.ArtistSortBy:
                    newMhod = new UnicodeMHOD();
                    break;

                case MHODElementType.MenuIndexTable:
                    newMhod = new MenuIndexMHOD();
                    break;

                case MHODElementType.PlaylistPosition:
                    newMhod = new PlaylistPositionMHOD();
                    break;

                default:
                    UnknownMHOD umhod = new UnknownMHOD();
                    newMhod = new UnknownMHOD();
                    break;
            }
            newMhod.SetHeader(mhod);
            newMhod.Read(iPod, reader);
            return newMhod;
        }
    }
}
