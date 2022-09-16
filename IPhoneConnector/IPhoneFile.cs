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
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace IPhoneConnector
{
    public class IPhoneFileInfo
    {
        public FileType Type;
        public long Size;
        public string Path;
        public string Name;
    }

    public enum FileType
    {
        File = 1,
        Folder = 2,
        BlockDevice = 3,
        CharDevice = 4,
        FIFO = 5,
        Link = 6,
        FileMask = 7,
        Socket = 8,
        Unknown = 9
    }

    public enum OpenMode
    {
        None = 0,
        ReadWrite = 2,
        WriteNew = 3,
    }

    /// <summary>
    /// Exposes a stream to a file on an iPhone, supporting both synchronous and asynchronous read and write operations
    /// </summary>
    public class IPhoneFile : Stream
    {      
        private OpenMode _mode;
        internal long _fileHandle;
        private IPhone _phone;
        private string _fileName;
        
        
        private IPhoneFile(IPhone phone, long handle, OpenMode mode)
            : base()
        {
            _phone = phone;
            _mode = mode;
            _fileHandle = handle;
        }

        public IPhoneFile(IPhone phone, string path, OpenMode mode): this(phone, 0, mode)
        {
            int ret;
            string full_path;
            path = IPhone.StandardPathToIPhonePath(path);

            full_path = phone.FullPath("/", path);
            _fileName = path;
            ret = AppleMobileDeviceAPI.AFCFileRefOpen(phone.AFCHandle, full_path, (int)mode, 0, out _fileHandle);
            
            HandleError("AFCFileRefOpen", ret);
        }

        #region Public Properties
        /// <summary>
        /// gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                if (_mode == OpenMode.ReadWrite)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value that determines whether the current stream can time out. 
        /// </summary>
        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                if (_mode == OpenMode.ReadWrite || _mode == OpenMode.WriteNew)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream . 
        /// </summary>
        public override long Length
        {
            get
            {
                IPhoneFileInfo fi = GetFileInfo();
                return fi.Size;
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream
        /// </summary>
        public override long Position
        {
            get
            {
                long position = 0;
                int ret = AppleMobileDeviceAPI.AFCFileRefTell(_phone.AFCHandle, _fileHandle, ref position);
                HandleError("AFCFileRefTell", ret);
                return position;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Sets the length of this stream to the given value. 
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            int ret;
            ret = AppleMobileDeviceAPI.AFCFileRefSetFileSize(_phone.AFCHandle, _fileHandle, (uint)value);
            HandleError("AFCFileRefSetFileSize", ret);
        }
        #endregion	// Public Properties

        #region Public Methods
        /// <summary>
        /// Releases the unmanaged resources used by iPhoneFile
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            uint len;
            int ret;
            byte[] temp;

            if (!CanRead)
            {
                throw new NotImplementedException("Stream open for writing only");
            }

            if (offset == 0)
            {
                temp = buffer;
            }
            else
            {
                temp = new byte[count];
            }
            len = (uint)count;
            ret = AppleMobileDeviceAPI.AFCFileRefRead(_phone.AFCHandle, _fileHandle, temp, ref len);
            HandleError("AFCFileRefRead", ret);
            
            if (temp != buffer)
            {
                Buffer.BlockCopy(temp, 0, buffer, offset, (int)len);
            }
            return (int)len;
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written. 
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            int ret;
            uint len;
            byte[] temp;

            if (!CanWrite)
            {
                throw new NotImplementedException("Stream open for reading only");
            }

            if (offset == 0)
            {
                temp = buffer;
            }
            else
            {
                temp = new byte[count];
                Buffer.BlockCopy(buffer, offset, temp, 0, count);
            }
            len = (uint)count;
            ret = AppleMobileDeviceAPI.AFCFileRefWrite(_phone.AFCHandle, _fileHandle, temp, len);
            HandleError("AFCFileRefWrite", ret);
        }

        /// <summary>
        /// Sets the position within the current stream
        /// </summary>
        /// <param name="offset">A byte offset relative to the <c>origin</c> parameter</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position</param>
        /// <returns>The new position within the stream</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            int ret;

            ret = AppleMobileDeviceAPI.AFCFileRefSeek(_phone.AFCHandle, _fileHandle, offset, (long)origin);
            HandleError("AFCFileRefSeek", ret);
            
            return Position;
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device. 
        /// </summary>
        public override void Flush()
        {
            int ret = AppleMobileDeviceAPI.AFCFlushData(_phone.AFCHandle, _fileHandle);
            HandleError("AFCFlushData", ret);
        }

        public override void Close()
        {
            if (_fileHandle != 0)
            {
                int ret = AppleMobileDeviceAPI.AFCFileRefClose(_phone.AFCHandle, _fileHandle);
                HandleError("AFCFileRefClose", ret);
                _fileHandle = 0;
            }
            
        }
        #endregion	// Public Methods

        public IPhoneFileInfo GetFileInfo()
        {
            return GetFileInfo(_phone, _fileName);
        }

        /// <summary>
        /// Returns the size and type of the specified file or directory.
        /// </summary>
        /// <param name="path">The file or directory for which to retrieve information.</param>
        /// <param name="size">Returns the size of the specified file or directory</param>
        /// <param name="fileType">Returns the size of the specified file or directory</param>
        internal static IPhoneFileInfo GetFileInfo(IPhone iPhone, string path)
        {
            IPhoneFileInfo fileInfo = new IPhoneFileInfo();
            fileInfo.Path = path;
            fileInfo.Name = Path.GetFileName(path);

			IntPtr dictionary;
			int ret = AppleMobileDeviceAPI.AFCFileInfoOpen(iPhone.AFCHandle, path, out dictionary);
			if (ret != 0)
			{
				return null;
			}

			Dictionary<string, string> dictInfo = AppleMobileDeviceAPI.ReadDictionary(dictionary);

			if (dictInfo.ContainsKey("st_size"))
				fileInfo.Size = Int64.Parse(dictInfo["st_size"]);
			if (dictInfo.ContainsKey("st_ifmt"))
			{
				switch (dictInfo["st_ifmt"])
				{
					case "S_IFDIR":
						fileInfo.Type = FileType.Folder;
						break;
					case "S_IFREG":
						fileInfo.Type = FileType.File;
						break;
					case "S_IFBLK":
						fileInfo.Type = FileType.BlockDevice;
						break;
					case "S_IFCHR":
						fileInfo.Type = FileType.CharDevice;
						break;
					case "S_IFIFO":
						fileInfo.Type = FileType.FIFO;
						break;
					case "S_IFLNK":
						fileInfo.Type = FileType.Link;
                        IntPtr handle = new IntPtr();
                        if (AppleMobileDeviceAPI.AFCDirectoryOpen(iPhone.AFCHandle, path, ref handle) == 0)
                        {
                            fileInfo.Type = FileType.Folder;
                            AppleMobileDeviceAPI.AFCDirectoryClose(iPhone.AFCHandle, handle);
                        }
						break;
					case "S_IFMT":
						fileInfo.Type = FileType.FileMask;
						break;
					case "S_IFSOCK":
						fileInfo.Type = FileType.Socket;
						break;
				}
			}
            return fileInfo;
        }

        public void Lock()
        {
            Lock(255);
        }

        public void Lock(int tries)
        {
            //try x times, as apparently this sometimes needs to be repeated if fails.
            int ret=0;
            for (int i = 0; i < tries; i++)
            {
                ret = AppleMobileDeviceAPI.AFCFileRefLock(_phone.AFCHandle, _fileHandle);
                if (ret == 0) break;
                Debug.WriteLine("waiting for lock...");
                Thread.Sleep(250);
            }
            HandleError("AFCFileRefLock", ret, new ApplicationException(_fileName + " couldn't be locked.  iTunes might be currently syncing this iPod. Please wait a few seconds and try again."));
        }

        public void Unlock()
        {
            int ret = AppleMobileDeviceAPI.AFCFileRefUnlock(_phone.AFCHandle, _fileHandle);
            HandleError("AFCFileRefUnlock", ret);
        }

        public void HandleError(string method, int returnCode)
        {
            HandleError(method, returnCode, null);
        }

        public void HandleError(string method, int returnCode, Exception overrideEx)
        {
            if (returnCode != 0) 
            {
                string msg = String.Format("{0}: {1} failed, returned {2}", method, _fileName, returnCode);
                Trace.WriteLine(msg);
                if (overrideEx != null)
                    throw overrideEx;
                else
                      throw new IOException(msg);
            }
        }
    }
}
