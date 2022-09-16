/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib.Parsers;
using System.Diagnostics;
using System.Reflection;

namespace SharePodLib.IPodDevice.FileSystems
{
    /// <summary>
    /// Abstraction of a standard (disk-based) iPod file system.
    /// </summary>
    class StandardFileSystem : DeviceFileSystem
    {
        private FileStream _fileLock;
        private const int FileCopyBufferSize = 131072; // 262144; //256k chunks

        public override event FileCopyProgressHandler FileCopyProgress;
        public override event EventHandler SyncCancelled;

        public StandardFileSystem(string name, string iTunesFolderPath, string iPodControlFolderPath, string artworkFolderPath, string photoFolderPath)
            : base(name, iTunesFolderPath, iPodControlFolderPath, artworkFolderPath, photoFolderPath)
        {
            
        }
       

        public StandardFileSystem() { }

        public override void CopyFileToDevice(string source, string destination)
        {
            if (!destination.StartsWith(DriveLetter)) destination = DriveLetter + destination;

            if (FailsafeMode)
            {
                File.Copy(source, destination, true);
                return;
            }

            long bytesTransferred = 0;

            using (FileStream sourceFile = File.OpenRead(source))
            {
                using (FileStream destinationFile = File.Create(destination))
                {
                    byte[] copyBuffer = new byte[FileCopyBufferSize];

                    while (true)
                    {
                        int length = sourceFile.Read(copyBuffer, 0, copyBuffer.Length);
                        if (length <= 0)
                            break;
                        destinationFile.Write(copyBuffer, 0, length);
                        bytesTransferred += length;

                        if (FileCopyProgress != null) FileCopyProgress(sourceFile.Length, bytesTransferred);
                    }
                }
            }
        }

        public override void CopyFileFromDevice(string source, string destination)
        {
            if (!source.StartsWith(DriveLetter)) source = DriveLetter + source;

            if (FailsafeMode)
            {
                File.Copy(source, destination, true);
                return;
            }

            long bytesTransferred = 0;

            using (FileStream sourceFile = File.OpenRead(source))
            {
                using (FileStream destinationFile = File.Create(destination))
                {
                    byte[] copyBuffer = new byte[FileCopyBufferSize];

                    while (true)
                    {
                        int length = sourceFile.Read(copyBuffer, 0, copyBuffer.Length);
                        if (length <= 0)
                            break;
                        destinationFile.Write(copyBuffer, 0, length);
                        bytesTransferred += length;

                        if (FileCopyProgress != null) FileCopyProgress(sourceFile.Length, bytesTransferred);
                    }
                }
            }
        }

        public override bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public override bool DirectoryExists(string name)
        {
            return Directory.Exists(name);
        }

        public override void DeleteFile(string name)
        {
            try
            {
                File.SetAttributes(name, FileAttributes.Normal); //can prevent IO errors deleting readonly files
            }
            catch (Exception) { }
            File.Delete(name);
        }

        public override long GetFileLength(string name)
        {
            return new FileInfo(name).Length;
        }

        public override void CreateDirectory(string name)
        {
            Helpers.EnsureDirectoryExists(new DirectoryInfo(name));
        }

        public override void AcquireLock()
        {
            if (!File.Exists(ITunesLockPath))
            {
                File.Create(ITunesLockPath).Close();
            }
            if (_fileLock == null)
            {
                _fileLock = File.Open(ITunesLockPath, FileMode.Open, FileAccess.Write, FileShare.None);
            }
        }

        public override void ReleaseLock()
        {
            if (_fileLock != null)
            {
                _fileLock.Close();
                File.Delete(ITunesLockPath);
                _fileLock = null;
            }
        }

        public override void Eject()
        {
            UsbEject.Library.Util.Eject(DriveLetter);
        }

        public override long AvailableFreeSpace
        {
            get { return Math.Max(new DriveInfo(DriveLetter).AvailableFreeSpace - 10485760, 0); }
        }

        public override long TotalSize
        {
            get { return new DriveInfo(DriveLetter).TotalSize; }
        }

        internal override IDeviceInfo QueryDeviceInfo(IPod iPod)
        {
            XmlQueryDeviceInfo deviceInfo = new XmlQueryDeviceInfo(iPod);
            deviceInfo.Read();
            return deviceInfo;
        }

        public override void StartSync()
        {
            // not implemented
        }

        public override void EndSync()
        {
            // not implemented
        }

        public override Stream OpenFile(string path, FileAccess mode)
        {
            return new FileStream(path, FileMode.OpenOrCreate, mode);
        }

        internal override DeviceFileSystem GetDevice()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                DeviceFileSystem fs = GetDeviceByDrive(drive);
                if (fs != null)
                    return fs;
            }
            return null;
        }

        internal override List<DeviceFileSystem> GetAllDevices()
        {
            List<DeviceFileSystem> fsList = new List<DeviceFileSystem>();

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                DeviceFileSystem fs = GetDeviceByDrive(drive);
                if (fs != null)
                    fsList.Add(fs);
            }
            return fsList;
        }

        public DeviceFileSystem GetDeviceByDrive(DriveInfo drive)
        {
            if (drive.IsReady && drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable)
            {
                if (DirectoryExists(Path.Combine(drive.Name, ITunesFolderPath)))
                {
                    StandardFileSystem fsInstance = this.Clone();
                    fsInstance.DriveLetter = drive.Name;
                    return fsInstance;
                }
            }
            return null;
        }

        public override string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        internal StandardFileSystem Clone()
        {
            StandardFileSystem standardFS = new StandardFileSystem();
            base.Clone(standardFS);
            return standardFS;
        }
    }
}
