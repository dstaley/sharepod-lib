/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib.Exceptions;
using SharePodLib.Parsers.iTunesDB;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace SharePodLib.Parsers.Artwork
{
    internal class PhotoDB : BaseDatabase
    {
        private ArtworkDBRoot _databaseRoot;
        private ImageAlbumList _albumList;
        private bool _isDirty = false;

        public PhotoDB(IPod iPod)
        {
            _iPod = iPod;
            _databaseFilePath = iPod.FileSystem.PhotoDBPath;
        }

        public override void Parse()
        {
            if (!_iPod.FileSystem.FileExists(_databaseFilePath))
            {
                return;
            }

            _databaseRoot = new ArtworkDBRoot();
            try
            {
                ReadDatabase(_databaseRoot);
                ImageList imageList = ((ImageListContainer)_databaseRoot.GetChildSection(MHSDSectionType.Images).GetListContainer()).ImageList;
                _albumList = ((ImageAlbumListContainer)_databaseRoot.GetChildSection(MHSDSectionType.Albums).GetListContainer()).ImageAlbumList;

                foreach (IPodImage art in imageList.Images())
                {
                    art.IsPhotoFormat = true;
                }

                _albumList.ResolveImages(imageList);
            }
            catch (Exception ex)
            {
                //Swallow any PhotoDB parsing issues for now.  We never write this file out anyway.
                DebugLogger.LogException(ex);
            }
            Trace.WriteLine("PhotoDB: " + _compatibility);
        }

        public override void Save()
        {
            if (_databaseRoot == null)
                return;

            AssertIsWritable();
            Debug.WriteLine("Saving PhotoDB " + DateTime.Now);
            WriteDatabase(_databaseRoot);
            _isDirty = false;
        }

        public override bool IsDirty
        {
            get { return _isDirty; }
        }

        public override void AssertIsWritable()
        {
            if (_databaseRoot == null)
            {
                throw new ArtworkDBNotFoundException();
            }
            base.AssertIsWritable();
        }

        public ImageAlbumList PhotoAlbumList
        {
            get { return _albumList; }
        }

        public override int Version
        {
            get { return 0; }
        }
    }
}