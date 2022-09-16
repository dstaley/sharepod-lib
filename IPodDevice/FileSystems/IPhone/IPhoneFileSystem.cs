/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using IPhoneConnector;
using System.Threading;
using System.IO;
using System.Diagnostics;
using SharePodLib.Parsers.iTunesCDB;

namespace SharePodLib.IPodDevice.FileSystems
{
    /// <summary>
    /// Abstraction of an iPhone/iTouch file system.
    /// </summary>
    class IPhoneFileSystem : DeviceFileSystem
    {

        public override event FileCopyProgressHandler FileCopyProgress;
        public override event EventHandler SyncCancelled;

        private IPhone _phone;
        private bool _isSyncing;
        

        public IPhone Phone
        {
            get { return _phone; }
            internal set { _phone = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of this profile</param>
        /// <param name="iTunesFolderPath">Path of the iTunes folder</param>
        /// <param name="musicFolderPath">Path of the Music folder</param>
        public IPhoneFileSystem(string name, string iTunesFolderPath, string iPodControlFolderPath, string artworkFolderPath, string photoFolderPath)
            : base(name, iTunesFolderPath, iPodControlFolderPath, artworkFolderPath, photoFolderPath)
        {
			DriveLetter = "/";
        }

        internal IPhoneFileSystem()
        {
			DriveLetter = "/";
        }

        internal override DeviceFileSystem GetDevice()
        {
            try
            {
				IPhoneConnectionListener.ListenForEvents(FileSystemAccess.Standard);
				_phone = IPhoneConnectionListener.WaitForSingleConnection(10);
            }
            catch (Exception ex)
            {
                DebugLogger.LogException(ex);
            }

            if (_phone != null)
            {
                Trace.WriteLine("iPhone/iTouch connected");

                if (DirectoryExists(ITunesFolderPath))
                {
                    return this;
                }
                else
                {
                    Trace.WriteLine("Path " + ITunesFolderPath + " not found");
                }
            }
            else
            {
                Trace.WriteLine("iPhone/iTouch not connected");
            }
            
            return null;
        }

		internal override List<DeviceFileSystem> GetAllDevices()
		{
			List<DeviceFileSystem> fsList = new List<DeviceFileSystem>();

            bool startedListener = false;
            if (!IPhoneConnectionListener.IsListening)
            {
                startedListener = true;
                IPhoneConnectionListener.ListenForEvents(FileSystemAccess.Standard);
                for (int i = 0; i < 40; i++)
                {
                    Thread.Sleep(100);
                }
            }
			foreach (IPhone phone in IPhoneConnectionListener.Connections)
			{
				IPhoneFileSystem fs = this.Clone();
				fs._phone = phone;
				fsList.Add(fs);
			}
            if (startedListener) IPhoneConnectionListener.StopListeningForEvents();
            
			return fsList;
		}

        public override void CopyFileToDevice(string source, string destination)
        {
            _phone.FileCopyProgress += _phone_FileCopyProgress;
            _phone.CopyFileToDevice(source, destination);
            _phone.FileCopyProgress -= _phone_FileCopyProgress;
        }

        public override void CopyFileFromDevice(string source, string destination)
        {
            _phone.FileCopyProgress += _phone_FileCopyProgress;
            _phone.CopyFileFromDevice(source, destination);
            _phone.FileCopyProgress -= _phone_FileCopyProgress;
        }

        void _phone_FileCopyProgress(long fileLength, long bytesCopied)
        {
            if (FileCopyProgress != null) FileCopyProgress(fileLength, bytesCopied);
        }

        public override bool FileExists(string fileName)
        {
            return _phone.ItemExists(fileName);
        }

        public override bool DirectoryExists(string name)
        {
            return _phone.ItemExists(name);
        }

        public override void DeleteFile(string name)
        {
            _phone.DeleteFile(name);
        }

        public override long GetFileLength(string name)
        {
            return _phone.GetFileInfo(name).Size;
        }

        public override void CreateDirectory(string name)
        {
            _phone.CreateDirectory(name);
        }

        public override void AcquireLock()
        {
            //not currently implemented
        }

        public override void ReleaseLock()
        {
            //not currently implemented
        }

        public override void Eject()
        {
            //not currently implemented
        }

        public override void StartSync()
        {
            if (!_isSyncing)
            {
                // if we don't have local copy of sqlite database files, we need to wait for iphone to finish updating its internal sqlite
                // databases before we snatch them. this has to happen before switching iphone to sync mode.
                if (IPod.ITunesDB.DatabaseRoot is ITunesCDBRoot && !SqliteTables.HaveLocalSqlliteDbs(IPod))
                {
                    var lockFile = new IPhoneFile(_phone, "/com.apple.itdbprep.postprocess.lock", IPhoneConnector.OpenMode.ReadWrite);
                    lockFile.Lock(480);  //this will wait for 2 minutes, then throw exception if file couldnt be locked
                    lockFile.Unlock();
                    lockFile.Close();
                }

                _isSyncing = true;
                _phone.StartSync();
                _phone.SyncCancelled += _phone_SyncCancelled;
            }
        }

        void _phone_SyncCancelled(object sender, EventArgs e)
        {
            if (SyncCancelled != null)
                SyncCancelled(this, null);
        }

        public override void EndSync()
        {
            if (_isSyncing)
            {
                _phone.EndSync();
                _phone.SyncCancelled -= _phone_SyncCancelled;
                _isSyncing = false;
            }            
        }

        public override Stream OpenFile(string path, FileAccess mode)
        {
            if (mode == FileAccess.Read || mode == FileAccess.ReadWrite)
                return new IPhoneFile(_phone, path, OpenMode.ReadWrite);
            else 
                return new IPhoneFile(_phone, path, OpenMode.WriteNew);
        }

        public override long AvailableFreeSpace
        {
            get
            {
                AFCDeviceInfo di = _phone.QueryDeviceInfo();
                return Math.Max(di.FileSystemFreeBytes - 10485760, 0);
            }
        }

        public override long TotalSize
        {
            get
            {
                AFCDeviceInfo di = _phone.QueryDeviceInfo();
                return di.FileSystemTotalBytes;
            }
        }

        internal override IDeviceInfo QueryDeviceInfo(IPod iPod)
        {
            return new IPhoneDeviceInfo(_phone);
        }

        public override string CombinePath(string path1, string path2)
        {
            string combined = Path.Combine(path1, path2);
            return combined.Replace("\\", "/");
        }

        internal IPhoneFileSystem Clone()
        {
            IPhoneFileSystem phoneFS = new IPhoneFileSystem();
            base.Clone(phoneFS);
            phoneFS.Phone = _phone;
            return phoneFS;
        }

        public override object GetProvider()
        {
            return _phone;
        }
    }
}
