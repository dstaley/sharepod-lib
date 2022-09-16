/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Parsers.Artwork
{
    //Implements a MHII ArtworkDB / PhotoDB element
    /// <summary>
    /// An iPod image.  Could either be CoverArt or a Photo.  Contains 1 or more IPodImageFormats
    /// </summary>
    public class IPodImage : BaseDatabaseElement
    {
        private List<MHODType2> _formatElements;
        private List<BaseMHODElement> _allElements = new List<BaseMHODElement>();
        private uint _id;
        private long _trackDBId;

        private uint _unk1, _unk2, _unk3, _unk4, _unk5, _unk6, _unk7, _originalImageSize;
        internal uint UsedCount { get; private set; }

        internal IPodImage()
        {
            _requiredHeaderSize = 64;
            _formatElements = new List<MHODType2>();
        }

        #region Overrides

        internal override void Read(IPod iPod, BinaryReader reader)
        {
            base.Read(iPod, reader);
            _identifier = reader.ReadChars(4);
            _headerSize = reader.ReadInt32();

            ValidateHeader("mhii");

            _sectionSize = reader.ReadInt32();
            int childCount = reader.ReadInt32();
            _id = reader.ReadUInt32();
            _trackDBId = reader.ReadInt64();
            _unk1 = reader.ReadUInt32();
            _unk2 = reader.ReadUInt32();
            _unk3 = reader.ReadUInt32();
            _unk4 = reader.ReadUInt32();
            _unk5 = reader.ReadUInt32();
            _originalImageSize = reader.ReadUInt32();
            _unk6 = reader.ReadUInt32();
            UsedCount = reader.ReadUInt32();
            _unk7 = reader.ReadUInt32();

            ReadToHeaderEnd(reader);

            for (int i = 0; i < childCount; i++)
            {
                BaseMHODElement mhodHeader = new BaseMHODElement();
                mhodHeader.Read(iPod, reader);

                BaseMHODElement mhod;
                if (mhodHeader.Type < 6)
                {
                    mhod = new MHODType2();
                    _formatElements.Add((MHODType2)mhod);
                }
                else
                {
                    mhod = new UnknownMHOD();
                }
                mhod.SetHeader(mhodHeader);
                mhod.Read(iPod, reader);
                _allElements.Add(mhod);
            }
        }

        internal override void Write(BinaryWriter writer)
        {
            _sectionSize = GetSectionSize();

            writer.Write(_identifier);
            writer.Write(_headerSize);
            writer.Write(_sectionSize);
            writer.Write(_allElements.Count);
            writer.Write(_id);
            writer.Write(_trackDBId);
            writer.Write(_unk1);
            writer.Write(_unk2);
            writer.Write(_unk3);
            writer.Write(_unk4);
            writer.Write(_unk5);
            writer.Write(_originalImageSize);
            writer.Write(_unk6);
            writer.Write(UsedCount);
            writer.Write(_unk7);
            writer.Write(_unusedHeader);

            for (int i = 0; i < _allElements.Count; i++)
            {
                _allElements[i].Write(writer);
            }
        }

        internal override int GetSectionSize()
        {
            int size = _headerSize;
            for (int i = 0; i < _allElements.Count; i++)
            {
                size += _allElements[i].GetSectionSize();
            }
            return size;
        }


        #endregion

        public uint Id
        {
            get { return _id; }
        }

        internal long TrackDBId
        {
            get { return _trackDBId; }
        }

        /// <summary>
        /// Enumerates each valid (width > 0) artwork format.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPodImageFormat> Formats
        {
            get
            {
                foreach (MHODType2 mhod in _formatElements)
                {
                    if (mhod.ArtworkFormat != null && mhod.ArtworkFormat.Width > 0)
                        yield return mhod.ArtworkFormat;
                }
            }
        }

        internal bool IsPhotoFormat
        {
            set
            {
                foreach (MHODType2 mhod in _formatElements)
                {
                    mhod.ArtworkFormat.IsPhoto = value;
                }
            }
        }

        /// <summary>
        /// Returns the smallest format in this image
        /// </summary>
        public IPodImageFormat SmallestFormat
        {
            get
            {
                IPodImageFormat format = _formatElements[0].ArtworkFormat;
                List<SupportedArtworkFormat> supportedFormats = format.IsPhoto ? _iPod.DeviceInfo.SupportedPhotoFormats : _iPod.DeviceInfo.SupportedArtworkFormats;
                foreach (MHODType2 mhod in _formatElements)
                {
                    if (mhod.ArtworkFormat != null && mhod.ArtworkFormat.ImageSize < format.ImageSize &&
                        supportedFormats.Exists(delegate(SupportedArtworkFormat testFormat) { return testFormat.FormatId == mhod.ArtworkFormat.FormatId || mhod.ArtworkFormat.FormatId == 1; }))
                    {
                        format = mhod.ArtworkFormat;
                    }
                }
                return format;
            }
        }

        /// <summary>
        /// Returns the largest format in this image
        /// </summary>
        public IPodImageFormat LargestFormat
        {
            get
            {
                if (_formatElements.Count == 0) return null;

                IPodImageFormat format = SmallestFormat;
                List<SupportedArtworkFormat> supportedFormats = format.IsPhoto ? _iPod.DeviceInfo.SupportedPhotoFormats : _iPod.DeviceInfo.SupportedArtworkFormats;
                foreach (MHODType2 mhod in _formatElements)
                {
                    if (mhod.ArtworkFormat != null && mhod.ArtworkFormat.ImageSize > format.ImageSize &&
                        supportedFormats.Exists(delegate(SupportedArtworkFormat testFormat) { return testFormat.FormatId == mhod.ArtworkFormat.FormatId || mhod.ArtworkFormat.FormatId == 1; }))
                    {
                        format = mhod.ArtworkFormat;
                    }
                }
                return format;
            }
        }

        internal void Create(IPod iPod, iTunesDB.Track track, Image image)
        {
            _iPod = iPod;
            _identifier = "mhii".ToCharArray();
            _headerSize = 152;
            _id = _iPod.IdGenerator.GetNewArtworkId();
            _trackDBId = track.DBId;
            _unusedHeader = new byte[_headerSize - _requiredHeaderSize];
            UsedCount = 1;
            _unk7 = 1;

            foreach (SupportedArtworkFormat supportedFormat in _iPod.DeviceInfo.SupportedArtworkFormats)
            {
                if (!track.IsVideo && supportedFormat.VideoOnly)
                    continue;

                MHODType2 mhod = new MHODType2();
                Bitmap resized = ArtworkHelper.GenerateResizedImage(image, supportedFormat);
                byte[] data = ArtworkHelper.GetByteData(resized);
                resized.Dispose();
                mhod.Create(_iPod, supportedFormat, data);

                _formatElements.Add(mhod);
                _allElements.Add(mhod);
            }
        }

        internal void Update(Image image)
        {
            foreach (MHODType2 mhod in _formatElements)
            {
                SupportedArtworkFormat supportedFormat = _iPod.DeviceInfo.SupportedArtworkFormats.Find(a => a.FormatId == mhod.ArtworkFormat.FormatId);

                if (supportedFormat == null)
                    continue;
                if (mhod.ArtworkFormat.IsFullResolution)
                    continue;

                Bitmap resized = ArtworkHelper.GenerateResizedImage(image, supportedFormat);
                byte[] data = ArtworkHelper.GetByteData(resized);
                resized.Dispose();
                mhod.ArtworkFormat.UpdateImageData(data);
            }

            _formatElements.RemoveAll(delegate(MHODType2 mhod)
            {
                SupportedArtworkFormat supportedFormat = _iPod.DeviceInfo.SupportedArtworkFormats.Find(a => a.FormatId == mhod.ArtworkFormat.FormatId);
                return supportedFormat == null;
            });
        }
    }
}
