/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using IPhoneConnector;
using SharePodLib.Parsers.Artwork;
using SharePodLib.Parsers.iTunesCDB;

namespace SharePodLib.IPodDevice.FileSystems
{
    class IPhoneDeviceInfo : IDeviceInfo
    {
        string _serial, _deviceId;
        List<SupportedArtworkFormat> _artworkFormats = new List<SupportedArtworkFormat>();
        List<SupportedArtworkFormat> _photoFormats = new List<SupportedArtworkFormat>();
        IPodFamily _family;

        #region IDeviceInfo Members

        public IPhoneDeviceInfo(IPhone phone)
        {
            _serial = phone.SerialNumber;
            _deviceId = phone.DeviceId;
            _family = IPodFamily.IPhone_ITouch;
            Version = phone.ProductVersion;

            if (Version.StartsWith("2"))
                _family = IPodFamily.IPhone_ITouch2;
            else if (Version.StartsWith("3"))
                _family = IPodFamily.IPhone_ITouch3;
            else if (Version.StartsWith("4"))
                _family = IPodFamily.IPhone_ITouch4;

            //_artworkFormats.Add(new SupportedArtworkFormat(3001, System.Drawing.Imaging.PixelFormat.Format16bppRgb555));
            _artworkFormats.Add(new SupportedArtworkFormat(3005, System.Drawing.Imaging.PixelFormat.Format16bppRgb555));
            _artworkFormats.Add(new SupportedArtworkFormat(3006, System.Drawing.Imaging.PixelFormat.Format16bppRgb555, 8192));

            SupportedArtworkFormat format = new SupportedArtworkFormat(3007, System.Drawing.Imaging.PixelFormat.Format16bppRgb555, 16384);
            format.VideoOnly = true;
            _artworkFormats.Add(format);

            _photoFormats.Add(new SupportedArtworkFormat(3004, System.Drawing.Imaging.PixelFormat.Format16bppRgb555));
            _photoFormats.Add(new SupportedArtworkFormat(3011, System.Drawing.Imaging.PixelFormat.Format16bppRgb555));
            _photoFormats.Add(new SupportedArtworkFormat(3008, System.Drawing.Imaging.PixelFormat.Format16bppRgb555));
            _photoFormats.Add(new SupportedArtworkFormat(3009, System.Drawing.Imaging.PixelFormat.Format16bppRgb555));
        }

        public Exception ReadException
        {
            get { return null; }
        }

        public string FirewireId
        {
            get { return _deviceId.Substring(0, 16).ToUpper(); }
        }

        public string SerialNumber
        {
            get { return _serial; }
        }

        public string SerialNumberForHashing
        {
            get { return _deviceId; }
        }

        public IPodFamily Family
        {
            get { return _family; }
        }

        public int FamilyId
        {
            get { return (int)_family; }
        }

        public List<SupportedArtworkFormat> SupportedArtworkFormats
        {
            get { return _artworkFormats; }
        }

        public string RawDeviceDescriptor
        {
            get { return null; }
        }

        public List<SupportedArtworkFormat> SupportedPhotoFormats
        {
            get { return _photoFormats; }
        }

        public string Version { get; private set; }

        #endregion
    }
}
