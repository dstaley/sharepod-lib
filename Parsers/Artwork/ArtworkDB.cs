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
    internal class ArtworkDB : BaseDatabase
    {
        private ArtworkDBRoot _databaseRoot;
        private ImageList _artworkList;
        private IThmbFileList _iThmbFileList;
        private bool _isDirty = false;
        
        
        public ArtworkDB(IPod iPod)
        {
            _iPod = iPod;
            _databaseFilePath = iPod.FileSystem.ArtworkDBPath;
        }

        public override void Parse()
        {
            if (!_iPod.FileSystem.FileExists(_databaseFilePath))
            {
                if (_iPod.DeviceInfo.SupportedArtworkFormats.Count > 0)
                {
                    Trace.WriteLine("ArtworkDB not found - importing empty ArtworkDB");
                    string tempPath = Path.GetTempFileName();
                    File.WriteAllBytes(tempPath, Properties.Resources.ArtworkDB_empty);
                    _iPod.FileSystem.CreateDirectory(_iPod.FileSystem.ArtworkFolderPath);
                    _iPod.FileSystem.CopyFileToDevice(tempPath, _databaseFilePath);
                    File.Delete(tempPath);
                }
                else
                {
                    return;  //no ArtworkDB and no SupportsArtworkFormats > we don't need to do anything.
                }
            }

            _databaseRoot = new ArtworkDBRoot();
            ReadDatabase(_databaseRoot);
            Trace.WriteLine("ArtworkDB: " + _compatibility);

            _artworkList = ((ImageListContainer)_databaseRoot.GetChildSection(MHSDSectionType.Images).GetListContainer()).ImageList;
            _iThmbFileList = ((IThmbFileListContainer)_databaseRoot.GetChildSection(MHSDSectionType.Files).GetListContainer()).FileList;

            //Match up the artwork to our track objects
            foreach (Track track in _iPod.Tracks)
            {
                IPodImage artwork = GetTrackArtForTrack(track);
                if (artwork != null)
                {
                    foreach (IPodImageFormat format in artwork.Formats)
                    {
                        track.Artwork.Add(format);
                    }
                }
                //if (track.ArtworkIdLink != 0)
                //{
                //    TrackArt artwork = _artworkList.GetArtById(track.ArtworkIdLink);
                //    foreach (ArtworkFormat format in artwork.Formats())
                //    {
                //        track.Artwork.Add(format);
                //    }
                //}
            }

            //Failsafe (older database) match up the artwork to our track objects
            //foreach (TrackArt artwork in _artworkList.TrackArt())
            //{
            //    Track track = _iPod.Tracks.FindByDBId(artwork.TrackDBId);
            //    if (track != null && track.ArtworkIdLink == 0)
            //    {
            //        foreach (ArtworkFormat format in artwork.Formats())
            //        {
            //            track.Artwork.Add(format);
            //        }
            //    }
            //}
        }

        public override void Save()
        {
            if (_databaseRoot == null)
                return;

            AssertIsWritable();
            Debug.WriteLine("Saving ArtworkDB " + DateTime.Now);
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

        public ImageList ArtworkList
        {
            get { return _artworkList; }
        }

        public override int Version
        {
            get { return 0; }
        }

        public uint NextImageId
        {
            get { return _databaseRoot.NextImageId; }
            set { _databaseRoot.NextImageId = value; }
        }

        internal void SetArtwork(Track track, Image image)
        {
            if (_iPod.DeviceInfo.SupportedArtworkFormats.Count == 0)
                return;

            AssertIsWritable();

            IPodImage existingArt = GetTrackArtForTrack(track);
            if (existingArt == null)
            {
                if (_iPod.FileSystem.AvailableFreeSpace <= 0)
                {
                    throw new OutOfDiskSpaceException("Your iPod does not have enough free space.");
                }

                _artworkList.AddNewArtwork(track, image);
            }
            else
            {
                existingArt.Update(image);
                track.Artwork.Clear();
                track.Artwork.AddRange(existingArt.Formats);
                track.ArtworkIdLink = existingArt.Id;
            }
            _isDirty = true;
        }

        internal void RemoveArtwork(Track track)
        {
            if (_iPod.DeviceInfo.SupportedArtworkFormats.Count == 0)
                return;

            if (_databaseRoot == null)
                return;

            bool shouldRemove = true;
            if (track.ArtworkIdLink != 0)
            {
                List<Track> tracksUsingArtwork = _iPod.Tracks.FindAll(delegate(Track t)
                {
                    return t.ArtworkIdLink == track.ArtworkIdLink;
                });
                shouldRemove = tracksUsingArtwork.Count <= 1;
            }

            IPodImage existingArt = GetTrackArtForTrack(track);
            if (existingArt != null)
            {
                AssertIsWritable();
                if (shouldRemove) _artworkList.RemoveArtwork(existingArt);
                track.Artwork.Clear();
                _isDirty = true;
            }
        }

        internal void GetIThmbRepository(IPodImageFormat format, out string fileName, out uint fileOffset)
        {
            fileName = "";
            bool foundFile = false;
            foreach (IThmbFile file in _iThmbFileList.Files())
            {
                if (file.FormatId == format.FormatId)
                {
                    foundFile = true;
                    break;
                }
            }
            if (!foundFile)
            {
                //If we didnt find a file ref for specified formatId, create one.
                _iThmbFileList.AddIThmbFile(format);
            }

            fileOffset = 0;
            foreach (IThmbFile file in _iThmbFileList.Files())
            {
                if (file.FormatId == format.FormatId)
                {
                    for (int i = 1; ; i++)
                    {
                        fileName = String.Format(@"F{0}_{1}.ithmb", file.FormatId, i);
                        string iThmbPath = _iPod.FileSystem.ArtworkFolderPath + fileName;

                        if (!_iPod.FileSystem.FileExists(iThmbPath))
                        {
                            fileOffset = 0;
                            return;
                        }

                        //dont let the iThmb file get above 200MB
                        fileOffset = GetNextFreeBlockInIThmb(fileName, format.ImageBlockSize);
                        if (fileOffset < 209715200)
                            return;
                    }
                }
            }
        }


        internal uint GetNextFreeBlockInIThmb(string fileName, uint iThmbBlockSize)
        {
            List<uint> offsets = new List<uint>();
            foreach (IPodImage artwork in _artworkList.Images())
            {
                foreach (IPodImageFormat fmt in artwork.Formats)
                {
                    if (fmt.FileName == fileName)
                    {
                        offsets.Add(fmt.FileOffset);
                    }
                }
            }
            offsets.Sort();
            long lastOffset = iThmbBlockSize * -1;
            foreach (uint offset in offsets)
            {
                if (lastOffset + iThmbBlockSize < offset)
                    break;
                lastOffset = offset;
            }
            return (uint)(lastOffset + (long)iThmbBlockSize);
        }

        private IPodImage GetTrackArtForTrack(Track track)
        {
            IPodImage artwork;

            if (track.ArtworkIdLink != 0)
            {
                artwork = _artworkList.GetArtById(track.ArtworkIdLink);
                if (artwork != null) return artwork;
            }

            //Failsafe (older database)
            artwork = _artworkList.GetArtByTrackId(track.DBId);
            return artwork;
        }
    }
}
