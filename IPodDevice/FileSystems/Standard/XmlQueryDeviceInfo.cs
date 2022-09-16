/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;
using SharePodLib.DatabaseHash;
using SharePodLib.Exceptions;
using SharePodLib.WinApi;
using SharePodLib.Parsers.Artwork;
using System.Drawing.Imaging;
using SharePodLib.Parsers;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;

namespace SharePodLib.IPodDevice.FileSystems
{

    class XmlQueryDeviceInfo : IDeviceInfo
    {
        IPod _iPod;
        string _serialNumber;
        string _firewireId;
        int _familyId;
        string _deviceXml;
        List<SupportedArtworkFormat> _supportedArtworkFormats = new List<SupportedArtworkFormat>();
        List<SupportedArtworkFormat> _supportedPhotoFormats = new List<SupportedArtworkFormat>();

        Exception _readException;

        public Exception ReadException
        {
            get { return _readException; }
        }

        internal XmlQueryDeviceInfo(IPod iPod)
        {
            _iPod = iPod;
        }

        internal void Read()
        {
            if (File.Exists(_iPod.DriveLetter + "ipod_control\\Device\\ExtendedSysInfoXml"))
            {
                Trace.WriteLine("Using ExtendedSysInfoXml file");
                _deviceXml = File.ReadAllText(_iPod.DriveLetter + "ipod_control\\Device\\ExtendedSysInfoXml");
            }
            else
            {
                bool read = ReadXmlFromDevice();
                if (!read)
                    return;
            }

            if (_deviceXml.Length == 0)
            {
                Trace.WriteLine("DeviceXml is empty");
                return;
            }

            try
            {
                XmlDocument sysInfoXml = new XmlDocument();
                sysInfoXml.XmlResolver = null;
                sysInfoXml.LoadXml(_deviceXml);
                ParseDeviceXml(sysInfoXml);
            }
            catch (Exception ex)
            {
                DebugLogger.LogException(ex);
            }
        }

        private bool ReadXmlFromDevice()
        {
            Trace.WriteLine("Attempting to read SysInfoExtended...");

            string helperName = Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "Helper.exe";
            SharePodLib.UnpackEmbeddedResource("SharePodLibHelper.exe", helperName, true);
            string tmp = Path.GetTempFileName();

            try
            {
                // SharePodLibHelper runs requiring admin rights
                ProcessStartInfo psi = new ProcessStartInfo(helperName, "\"" + tmp + "\" devicexml " + _iPod.DriveLetter);
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process helper = Process.Start(psi);
                helper.WaitForExit();
                _deviceXml = File.ReadAllText(tmp);
                File.Delete(tmp);
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1223) /* cancelled by user */
                {
                    throw new BaseSharePodLibException("You must give " + helperName + " administrator access when requested.");
                }
                throw ex;
            }

            Trace.WriteLine("Read SysInfoExtended - length " + _deviceXml.Length.ToString());

            _deviceXml = _deviceXml.Replace("\0", "");  //replace any NULL chars that could screw up xml loading

            //cache the result so next time we don't need to query the device.
            try
            {
                Helpers.EnsureDirectoryExists(new DirectoryInfo(_iPod.DriveLetter + "ipod_control\\Device\\"));

                File.WriteAllText(_iPod.DriveLetter + "ipod_control\\Device\\ExtendedSysInfoXml", _deviceXml);
            }
            catch (Exception ex) //this sometimes results in a CRC error, unknown why
            {
                DebugLogger.LogException(ex);
                Trace.WriteLine(_deviceXml);
            }
            return _deviceXml != "";
        }

        /// <summary>
        /// FirewireId of the iPod
        /// </summary>
        public string FirewireId
        {
            get { return _firewireId; }
        }

        /// <summary>
        /// Serial number of the iPod
        /// </summary>
        public string SerialNumber
        {
            get { return _serialNumber; }
        }

        public string SerialNumberForHashing
        {
            get
            {
                //returns firewireid + hex endcoded serial + trailing 0
                string serial = FirewireId + BitConverter.ToString(Encoding.ASCII.GetBytes(SerialNumber)).Replace("-", "") + "00";
                return serial;
            }
        }

        /// <summary>
        /// Tries to return the FamilyId as an IPodFamily enum
        /// </summary>
        public IPodFamily Family
        {
            get
            {
                if (Enum.IsDefined(typeof(IPodFamily), _familyId))
                {
                    return (IPodFamily)_familyId;
                }
                else
                {
                    return IPodFamily.Unknown;
                }
            }
        }

        /// <summary>
        /// Returns the FamilyId as an integer reported by the iPod
        /// </summary>
        public int FamilyId
        {
            get { return _familyId; }
        }

        /// <summary>
        /// List of supported artwork sizes for the iPod
        /// </summary>
        public List<SupportedArtworkFormat> SupportedArtworkFormats
        {
            get { return _supportedArtworkFormats; }
        }

        /// <summary>
        /// List of supported photo sizes for the iPod
        /// </summary>
        public List<SupportedArtworkFormat> SupportedPhotoFormats
        {
            get { return _supportedPhotoFormats; }
        }

        /// <summary>
        /// Returns the exact data the iPod returned to SharePodLib.
        /// </summary>
        public string RawDeviceDescriptor
        {
            get { return _deviceXml; }
        }

        public string Version { get; private set; }

        private void ParseDeviceXml(XmlDocument document)
        {
            XmlNode node = document.SelectSingleNode("/plist/dict/key[text()='SerialNumber']/following-sibling::*[1]");
            _serialNumber = node.InnerText;

            node = document.SelectSingleNode("/plist/dict/key[text()='FireWireGUID']/following-sibling::*[1]");
            _firewireId = node.InnerText;
            Trace.WriteLine("iPod FirewireId: " + _firewireId);

            node = document.SelectSingleNode("/plist/dict/key[text()='FamilyID']/following-sibling::*[1]");
            _familyId = int.Parse(node.InnerText);
            Trace.WriteLine("iPod Family: " + _familyId);

            if (_familyId == (int)IPodFamily.iPod_Nano_Gen5)
            {
                // Nano 5G doesn't report supported formats :(
                _supportedArtworkFormats.Add(new SupportedArtworkFormat(1056, PixelFormat.Format16bppRgb565, 128, 128));
                _supportedArtworkFormats.Add(new SupportedArtworkFormat(1078, PixelFormat.Format16bppRgb565));
                _supportedArtworkFormats.Add(new SupportedArtworkFormat(1073, PixelFormat.Format16bppRgb565, 240, 240));
                _supportedArtworkFormats.Add(new SupportedArtworkFormat(1074, PixelFormat.Format16bppRgb565));

                _supportedPhotoFormats.Add(new SupportedArtworkFormat(1087, PixelFormat.Format16bppRgb565));
                _supportedPhotoFormats.Add(new SupportedArtworkFormat(1079, PixelFormat.Format16bppRgb565));
                _supportedPhotoFormats.Add(new SupportedArtworkFormat(1066, PixelFormat.Format16bppRgb565));
                return;
            }

            node = document.SelectSingleNode("/plist/dict/key[text()='AlbumArt']/following-sibling::*[1]");
            ReadArtworkNode(node, _supportedArtworkFormats, false);

            node = document.SelectSingleNode("/plist/dict/key[text()='ImageSpecifications']/following-sibling::*[1]");
            ReadArtworkNode(node, _supportedPhotoFormats, true);
        }

        private void ReadArtworkNode(XmlNode node, List<SupportedArtworkFormat> artwork, bool useReportedSize)
        {
            if (node == null) return;

            XmlNodeList albumArtNodes = node.SelectNodes("dict");
            foreach (XmlNode albumArtNode in albumArtNodes)
            {
                string formatId = albumArtNode.SelectSingleNode("key[text()='FormatId']").NextSibling.InnerText;
                uint width = UInt32.Parse(albumArtNode.SelectSingleNode("key[text()='RenderWidth']").NextSibling.InnerText);
                uint height = UInt32.Parse(albumArtNode.SelectSingleNode("key[text()='RenderHeight']").NextSibling.InnerText);
                string pixelFormat = albumArtNode.SelectSingleNode("key[text()='PixelFormat']").NextSibling.InnerText;

                if (pixelFormat == "4C353635")
                {
                    Trace.WriteLine(String.Format("Supported artwork format: {0} {1}x{2}, format {3}", formatId, width, height, pixelFormat));

                    try
                    {
                        if (!artwork.Exists(delegate(SupportedArtworkFormat format)
                        {
                            return format.Width == width && format.Height == height;
                        }))
                        {
                            if (useReportedSize)
                                artwork.Add(new SupportedArtworkFormat(uint.Parse(formatId), PixelFormat.Format16bppRgb565, width, height));
                            else
                                artwork.Add(new SupportedArtworkFormat(uint.Parse(formatId), PixelFormat.Format16bppRgb565));
                        }
                        else
                        {
                            Trace.WriteLine("Format ignored.");
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.LogException(ex);
                    }
                }
                else
                {
                    Trace.WriteLine(String.Format("Unknown artwork format: {0} {1}x{2} {3}", formatId, width, height, pixelFormat));
                }
            }
        }
    }
}
