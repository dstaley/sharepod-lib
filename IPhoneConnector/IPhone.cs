// Software License Agreement (BSD License)
// 
// Based on original code by Peter Dennis Bartok <PeterDennisBartok@gmail.com>
// All rights reserved.
// 
// Redistribution and use of this software in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
// 
// * Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
// 
// * Neither the name of Peter Dennis Bartok nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission of Yahoo! Inc.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace IPhoneConnector
{
    public enum DirectoryItems
    {
        All,
        Files,
        Folders
    }

    public enum TraceLevel
    {
        Normal,
        All
    }

    public enum FileSystemAccess
    {
        Standard,
        RootIfAvailable
    }

    /// <summary>
    /// Exposes access to the Apple iPhone
    /// </summary>
    public class IPhone
    {
        internal const string TraceCategory = "IPhoneConnector";
        private const int BUFFER_SIZE = 524288; //copy in 512KB chunks

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler SyncCancelled;
        public delegate void FileCopyProgressHandler(long fileLength, long bytesCopied);
        public event FileCopyProgressHandler FileCopyProgress;

        
        private DeviceEventSink DeviceHandleEventSink;

		internal IntPtr DeviceHandle { get; private set; }
        private IntPtr _afcHandle, _notificationsHandle;
        
        /// <summary>
        /// Returns true if an iPhone is connected to the computer
        /// </summary>
        public bool IsConnected {get; set;}
        public bool IsConnecting { get; set; }
        private string _directory;
        private TraceLevel _traceLevel;
        private FileSystemAccess _access;
        private string _firmwareVersion, _productVersion, _serialNumber, _uuid;
        private IPhoneFile _syncLockFile;
        
        /// <summary>
        /// Creates a new iPhone object. If an iPhone is connected to the computer, a connection will automatically be opened.
        /// </summary>
        internal IPhone(IntPtr device, FileSystemAccess access)
        {
            _directory = "/";
            _access = access;
			DeviceHandle = device;
			
			if (ConnectToDevice())
			{
				if (Connected != null)
					Connected(this, null);
			}
			IsConnecting = false;
        }


		internal void OnDisconnect()
		{
			IsConnected = false;
			Trace.WriteLine("Device "+_serialNumber+" disconnected.", TraceCategory);
			if (Disconnected != null) Disconnected(this, null);
		}


        public void CloseConnection()
        {
            if (_afcHandle != IntPtr.Zero)
            {
                int ret = AppleMobileDeviceAPI.AFCConnectionClose(_afcHandle);
                Trace.WriteLine("AFCConnectionClose: " + ret, TraceCategory);
                _afcHandle = IntPtr.Zero;
                IsConnected = false;
            }            
        }
       

        #region Properties

        /// <summary>
        /// Set level of tracing. Normal traces connect/disconnect, set to 'All' to trace all calls.
        /// </summary>
        public TraceLevel TraceLevel
        {
            get { return _traceLevel; }
            set { _traceLevel = value; }
        }

        
        /// <summary>
        /// Returns the handle to the iPhone com.apple.afc service
        /// </summary>
        internal IntPtr AFCHandle
        {
            get { return this._afcHandle; }
        }

        public string ProductVersion
        {
            get { return _productVersion; }
        }

        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
        }

        public string DeviceId
        {
            get { return _uuid; }
        }

        public string SerialNumber
        {
            get { return _serialNumber; }
        }

        #endregion
        
        #region Filesystem
        
        
        /// <summary>
        /// Creates the directory specified in path
        /// </summary>
        /// <param name="path">The directory path to create</param>
        /// <returns>true if directory was created</returns>
        public bool CreateDirectory(string path)
        {
            path = StandardPathToIPhonePath(path);
            string full_path;

            full_path = FullPath(_directory, path);
            int ret = AppleMobileDeviceAPI.AFCDirectoryCreate(_afcHandle, full_path);
            Trace.WriteLine(String.Format("AFCDirectoryCreate: ({0}): {1}", full_path, ret), TraceCategory);

            return ret == 0;
        }

        /// <summary>
        /// Gets the names of subdirectories in a specified directory.
        /// </summary>
        /// <param name="path">The path for which an array of subdirectory names is returned.</param>
        /// <returns>An array of type <c>String</c> containing the names of subdirectories in <c>path</c>.</returns>
        /// 
        public List<IPhoneFileInfo> GetItemsInDirectory(string path, DirectoryItems directoryItems)
        {        
            if (!IsConnected)
            {
                throw new Exception("Not connected to phone");
            }

            path = StandardPathToIPhonePath(path);

            IntPtr directoryHandle = new IntPtr();
            string full_path = FullPath(_directory, path);

            int ret = AppleMobileDeviceAPI.AFCDirectoryOpen(_afcHandle, full_path, ref directoryHandle);
            if (_traceLevel == TraceLevel.All)
                Trace.WriteLine(String.Format("AFCDirectoryOpen: ({0}): {1}", full_path, ret), TraceCategory);

            if (ret != 0)
            {
                throw new Exception("Path does not exist");
            }

            string buffer = null;
            List<IPhoneFileInfo> items = new List<IPhoneFileInfo>();

            ret = AppleMobileDeviceAPI.AFCDirectoryRead(_afcHandle, directoryHandle, ref buffer);

            if (_traceLevel == TraceLevel.All)
                Trace.WriteLine(String.Format("AFCDirectoryRead: ({0}): {1}", full_path, ret), TraceCategory);

            while (buffer != null)
            {
                if (buffer == "." || buffer == "..")
                {
                    //ignore these items
                }
                else
                {
                    IPhoneFileInfo fileInfo = IPhoneFile.GetFileInfo(this, (FullPath(full_path, buffer)));
                    switch (directoryItems)
                    {
                        case DirectoryItems.All:
                            items.Add(fileInfo);
                            break;
                        case DirectoryItems.Files:
                            if (fileInfo.Type == FileType.File)
                                items.Add(fileInfo);
                            break;
                        case DirectoryItems.Folders:
                            if (fileInfo.Type == FileType.Folder)
                                items.Add(fileInfo);
                            break;
                    }
                }
                ret = AppleMobileDeviceAPI.AFCDirectoryRead(_afcHandle, directoryHandle, ref buffer);
                if (_traceLevel == TraceLevel.All)
                    Trace.WriteLine(String.Format("AFCDirectoryRead: ({0}): {1}", full_path, ret), TraceCategory);
            }
            AppleMobileDeviceAPI.AFCDirectoryClose(_afcHandle, directoryHandle);

            if (_traceLevel == TraceLevel.All)
                Trace.WriteLine(String.Format("AFCDirectoryClose: ({0}): {1}", full_path, ret), TraceCategory);
            
            return items;
        }

        /// <summary>
        /// Moves a file or a directory and its contents to a new location or renames a file or directory if the old and new parent path matches.
        /// </summary>
        /// <param name="sourceName">The path of the file or directory to move or rename.</param>
        /// <param name="destName">The path to the new location for <c>sourceName</c>.</param>
        ///	<remarks>Files cannot be removed across filesystem boundaries.</remarks>
        public void Rename(string sourceName, string destName)
        {
            sourceName = StandardPathToIPhonePath(sourceName);
            destName = StandardPathToIPhonePath(destName);
            int ret = AppleMobileDeviceAPI.AFCRenamePath(_afcHandle, FullPath(_directory, sourceName), FullPath(_directory, destName));
            if (_traceLevel == TraceLevel.All)
                Trace.WriteLine(String.Format("AFCRenamePath: ({0} > {1}): {2}", sourceName, destName, ret), TraceCategory);
            
        }


        /// <summary>
        /// Determines whether the given path refers to an existing file or directory on the phone. 
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><c>true</c> if path refers to an existing file or directory, otherwise <c>false</c>.</returns>
        public bool ItemExists(string path)
        {
            path = StandardPathToIPhonePath(path);
            return IPhoneFile.GetFileInfo(this, path) != null;
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory on the phone. 
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><c>true</c> if path refers to an existing directory, otherwise <c>false</c>.</returns>
        public bool IsDirectory(string path)
        {
            path = StandardPathToIPhonePath(path);
            IPhoneFileInfo fileInfo = IPhoneFile.GetFileInfo(this, path);
            return fileInfo.Type == FileType.Folder;
        }

        public IPhoneFileInfo GetFileInfo(string path)
        {
            path = StandardPathToIPhonePath(path);
            IPhoneFileInfo fileInfo = IPhoneFile.GetFileInfo(this, path);
            return fileInfo;
        }
        
        /// <summary>
        /// Deletes the specified directory and any subdirectories in the directory.
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        public void DeleteItem(string path)
        {
            path = StandardPathToIPhonePath(path);
            string full_path;
                        
            full_path = FullPath(_directory, path);
            if (IsDirectory(full_path))
            {
                InternalDeleteDirectory(path);
            }
            else
            {
                DeleteFile(path);
            }

        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to remove.</param>
        public void DeleteFile(string path)
        {
            path = StandardPathToIPhonePath(path);
            string full_path;

            full_path = FullPath(_directory, path);
            if (ItemExists(full_path))
            {
                int ret = AppleMobileDeviceAPI.AFCRemovePath(_afcHandle, full_path);
                if (_traceLevel == TraceLevel.All)
                    Trace.WriteLine(String.Format("AFCRemovePath: ({0}): {1}", full_path, ret), TraceCategory);
            }
        }

        public void CopyFileToDevice(string sourceFileName, string deviceFileName)
        {
            deviceFileName = StandardPathToIPhonePath(deviceFileName);
            long bytesCopied = 0;
            using (Stream fileStream = File.OpenRead(sourceFileName))
            {
                using (IPhoneFile deviceFile = new IPhoneFile(this, deviceFileName, OpenMode.WriteNew))
                {
                    byte[] copyBuffer = new byte[BUFFER_SIZE];

                    while (true)
                    {
                        int length = fileStream.Read(copyBuffer, 0, copyBuffer.Length);
                        if (length <= 0)
                            break;
                        deviceFile.Write(copyBuffer, 0, length);
                        bytesCopied += length;

                        if (FileCopyProgress != null) FileCopyProgress(fileStream.Length, bytesCopied);

                    }
                }
            }
        }

        public void CopyFileFromDevice(string deviceFileName, string destinationFileName)
        {
            deviceFileName = StandardPathToIPhonePath(deviceFileName);
            long bytesCopied = 0;
            using (IPhoneFile deviceFile = new IPhoneFile(this, deviceFileName, OpenMode.ReadWrite))
            {
                using (Stream destinationFile = File.OpenWrite(destinationFileName))
                {
                    byte[] copyBuffer = new byte[BUFFER_SIZE];

                    while (true)
                    {
                        int length = deviceFile.Read(copyBuffer, 0, copyBuffer.Length);
                        if (length <= 0)
                            break;
                        destinationFile.Write(copyBuffer, 0, length);
                        bytesCopied += length;
                        if (FileCopyProgress != null) FileCopyProgress(deviceFile.Length, bytesCopied);
                    }
                }
            }
        }

        
        #endregion	// Filesystem

        public AFCDeviceInfo QueryDeviceInfo()
        {
            IntPtr dictPtr = IntPtr.Zero;
            AFCDeviceInfo di = new AFCDeviceInfo();

            int ret = AppleMobileDeviceAPI.AFCDeviceInfoOpen(_afcHandle, ref dictPtr);
            if (_traceLevel == TraceLevel.All)
                Trace.WriteLine(String.Format("AFCDeviceInfoOpen: {0}", ret), TraceCategory);

            if (ret == 0)
            {
				Dictionary<string, string> deviceInfo = AppleMobileDeviceAPI.ReadDictionary(dictPtr);
                if (deviceInfo.ContainsKey("Model"))
                    di.Model = deviceInfo["Model"];
                if (deviceInfo.ContainsKey("FSFreeBytes"))
                    di.FileSystemFreeBytes = long.Parse(deviceInfo["FSFreeBytes"]);
                if (deviceInfo.ContainsKey("FSTotalBytes"))
                    di.FileSystemTotalBytes = long.Parse(deviceInfo["FSTotalBytes"]);
                if (deviceInfo.ContainsKey("FSBlockSize"))
                    di.FileSystemBlockSize = int.Parse(deviceInfo["FSBlockSize"]);
            }
                        
            return di;
        }

        #region Private Methods

        private bool ConnectToDevice()
        {
            IsConnecting = true;
            int ret = AppleMobileDeviceAPI.AMDeviceConnect(DeviceHandle);
            Trace.WriteLine(String.Format("AMDeviceConnect: {0}", ret), TraceCategory);
            if (ret == 1)
            {
                Trace.WriteLine("The iPhone is in Recovery Mode and cannot be used until activated with iTunes.", TraceCategory);
            }
            if (ret != 0)
            {
                return false;
            }
            ret = AppleMobileDeviceAPI.AMDeviceIsPaired(DeviceHandle);
            Trace.WriteLine(String.Format("AMDeviceIsPaired: {0}", ret), TraceCategory);

            if (ret == 0)
            {
                ret = AppleMobileDeviceAPI.AMDevicePair(DeviceHandle);
                Trace.WriteLine(String.Format("AMDevicePair: {0}", ret), TraceCategory);
                if (ret != 0)
                {
                    return false;
                }
            }

            ret = AppleMobileDeviceAPI.AMDeviceValidatePairing(DeviceHandle);
            Trace.WriteLine(String.Format("AMDeviceValidatePairing: {0}", ret), TraceCategory);
            if (ret != 0)
            {
                return false;
            }

            ret = AppleMobileDeviceAPI.AMDeviceStartSession(DeviceHandle);
            Trace.WriteLine(String.Format("AMDeviceStartSession: {0}", ret), TraceCategory);
            if (ret == 1)
            {
                return false;
            }

            IntPtr serialPtr = AppleMobileDeviceAPI.AMDeviceCopyDeviceIdentifier(DeviceHandle);
            _uuid = AppleMobileDeviceAPI.CFStringRefToString(serialPtr);
            _firmwareVersion = AppleMobileDeviceAPI.AMDeviceCopyValue(DeviceHandle, 0, "FirmwareVersion");
            Thread.Sleep(10);
            _productVersion = AppleMobileDeviceAPI.AMDeviceCopyValue(DeviceHandle, 0, "ProductVersion");
            Thread.Sleep(10);
            _serialNumber = AppleMobileDeviceAPI.AMDeviceCopyValue(DeviceHandle, 0, "SerialNumber");

            if (_access == FileSystemAccess.RootIfAvailable)
            {
                ret = AppleMobileDeviceAPI.AMDeviceStartService(DeviceHandle, "com.apple.afc2", ref _afcHandle, IntPtr.Zero);
                Trace.WriteLine(String.Format("StartService (AFC2): {0}", ret), TraceCategory);
            }
            
            if (_access == FileSystemAccess.Standard || _access == FileSystemAccess.RootIfAvailable && ret != 0)
            {
                ret = AppleMobileDeviceAPI.AMDeviceStartService(DeviceHandle, "com.apple.afc", ref _afcHandle, IntPtr.Zero);
                Trace.WriteLine(String.Format("StartService (AFC): {0}", ret), TraceCategory);
                if (ret != 0)
                {
                    return false;
                }
            }
            
            ret = AppleMobileDeviceAPI.AMSInitialize();
            Trace.WriteLine(String.Format("AMSInitialize: {0}", ret), TraceCategory);
            
            ret = AppleMobileDeviceAPI.AMDeviceStartService(DeviceHandle, "com.apple.mobile.notification_proxy", ref _notificationsHandle, IntPtr.Zero);
            Trace.WriteLine(String.Format("StartService (Notifications): {0}", ret), TraceCategory);

            ret = AppleMobileDeviceAPI.AMDeviceStopSession(DeviceHandle);
            Trace.WriteLine(String.Format("AMDeviceStopSession: {0}", ret), TraceCategory);
            if (ret != 0)
                return false;
            ret = AppleMobileDeviceAPI.AMDeviceDisconnect(DeviceHandle);
            Trace.WriteLine(String.Format("AMDeviceDisconnect: {0}", ret), TraceCategory);
            if (ret != 0)
                return false;

            ret = AppleMobileDeviceAPI.AFCConnectionOpen(_afcHandle, 0, ref _afcHandle);
            Trace.WriteLine(String.Format("AFCConnectionOpen: {0}", ret), TraceCategory);
            if (ret != 0)
            {
                return false;
            }

            ret = AppleMobileDeviceAPI.AMDObserveNotification(_notificationsHandle, AppleMobileDeviceAPI.StringToCFString("com.apple.itunes-client.syncCancelRequest"));
            Trace.WriteLine(String.Format("AMDObserveNotification: {0}", ret), TraceCategory);
            if (ret != 0)
            {
                return false;
            }

            DeviceHandleEventSink = new DeviceEventSink(DeviceNotification2);
            AppleMobileDeviceAPI.AMDListenForNotifications(_notificationsHandle, DeviceHandleEventSink, IntPtr.Zero);
            Trace.WriteLine(String.Format("AMDListenForNotifications: {0}", ret), TraceCategory);
            if (ret != 0)
            {
                return false;
            }

            IsConnected = true;
            IsConnecting = false;
            return true;
        }

        void DeviceNotification2(IntPtr str, IntPtr user)
        {
            string notification = AppleMobileDeviceAPI.CFStringRefToString(str);
            if (notification == "com.apple.itunes-client.syncCancelRequest") //"AMDNotificationFaceplant")
            {
                if (SyncCancelled != null)
                    SyncCancelled(this, null);
            }
        }

        public void StartSync()
        {
            if (_syncLockFile != null) return;

            int ret = AppleMobileDeviceAPI.AMDPostNotification(_notificationsHandle, AppleMobileDeviceAPI.StringToCFString("com.apple.itunes-mobdev.syncWillStart"), 0);
            Trace.WriteLine(String.Format("AMDevicePostNotification: (sync) {0}", ret), TraceCategory);
            Thread.Sleep(200);
            
            _syncLockFile = new IPhoneFile(this, "/com.apple.itunes.lock_sync", OpenMode.ReadWrite);
            Thread.Sleep(50);
            ret = AppleMobileDeviceAPI.AMDPostNotification(_notificationsHandle, AppleMobileDeviceAPI.StringToCFString("com.apple.itunes-mobdev.syncLockRequest"), 0);
            Trace.WriteLine(String.Format("AMDevicePostNotification: (lock) {0}", ret), TraceCategory);
            Thread.Sleep(200);

            _syncLockFile.Lock(40);
        }

        public void EndSync()
        {
            if (_syncLockFile != null)
            {
                _syncLockFile.Unlock();
                _syncLockFile.Close();
            }
            _syncLockFile = null;

            int ret = AppleMobileDeviceAPI.AMDPostNotification(_notificationsHandle, AppleMobileDeviceAPI.StringToCFString("com.apple.itunes-mobdev.syncDidFinish"), 0);
            Trace.WriteLine(String.Format("AMDevicePostNotification: (done-sync) {0}", ret), TraceCategory);
            Thread.Sleep(200);
        }

        


        private void  InternalDeleteDirectory(string path)
        {
            string full_path;

            full_path = FullPath(_directory, path);
            List<IPhoneFileInfo> items = GetItemsInDirectory(path, DirectoryItems.All);

            foreach (IPhoneFileInfo fileInfo in items)
            {
                if (fileInfo.Type == FileType.File)
                    DeleteFile(fileInfo.Path);
            }

            foreach (IPhoneFileInfo fileInfo in items)
            {
                if (fileInfo.Type == FileType.Folder)
                    InternalDeleteDirectory(fileInfo.Path);
            }

            int ret = AppleMobileDeviceAPI.AFCRemovePath(_afcHandle, full_path);
            if (_traceLevel == TraceLevel.All)
                Trace.WriteLine(String.Format("AFCRemovePath ({0}): {1}", full_path, ret), TraceCategory);

        }

        static char[] path_separators = { '/' };
        internal string FullPath(string path1, string path2)
        {
            string[] path_parts;
            string[] result_parts;
            int target_index;

            if ((path1 == null) || (path1 == String.Empty))
            {
                path1 = "/";
            }

            if ((path2 == null) || (path2 == String.Empty))
            {
                path2 = "/";
            }

            if (path2[0] == '/')
            {
                path_parts = path2.Split(path_separators);
            }
            else if (path1[0] == '/')
            {
                path_parts = (path1 + "/" + path2).Split(path_separators);
            }
            else
            {
                path_parts = ("/" + path1 + "/" + path2).Split(path_separators);
            }
            result_parts = new string[path_parts.Length];
            target_index = 0;

            for (int i = 0; i < path_parts.Length; i++)
            {
                if (path_parts[i] == "..")
                {
                    if (target_index > 0)
                    {
                        target_index--;
                    }
                }
                else if ((path_parts[i] == ".") || (path_parts[i] == ""))
                {
                    // Do nothing
                }
                else
                {
                    result_parts[target_index++] = path_parts[i];
                }
            }

            return "/" + String.Join("/", result_parts, 0, target_index);
        }

        internal static string StandardPathToIPhonePath(string path)
        {
            return path.Replace("\\", "/");
        }

        #endregion	// Private Methods


        public static bool IsMobileDeviceDllAvailable
        {
            get
            {
                return AppleMobileDeviceAPI.GetMobileDeviceDllPathInfo() != null;
            }
        }
    }
}