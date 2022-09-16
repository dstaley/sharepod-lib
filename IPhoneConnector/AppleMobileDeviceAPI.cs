// Software License Agreement (BSD License)
// 
// Based on original code by Peter Dennis Bartok <PeterDennisBartok@gmail.com>
// All rights reserved
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

//
// Based on code developed by geohot, ixtli, nightwatch, warren
// See http://iphone.fiveforty.net/wiki/index.php?title=Main_Page
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace IPhoneConnector
{

    /// <summary>
    /// Provides the fields representing the type of notification
    /// </summary>
    enum NotificationMessage
    {
        Connected = 1,
        Disconnected = 2,
        Unknown = 3,
    }

    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct AMDeviceNotification
    {
        uint unknown0;	/* 0 */
        uint unknown1;	/* 4 */
        uint unknown2;	/* 8 */
        DeviceNotificationCallback callback;   /* 12 */
        uint unknown3;	/* 16 */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct AMDeviceNotificationCallbackInfo
    {
        internal IntPtr dev_ptr;
        public NotificationMessage msg;
    }


    public struct AFCDeviceInfo
    {
        public string Model;
        public long FileSystemTotalBytes;
        public long FileSystemFreeBytes;
        public int FileSystemBlockSize;
    }

    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DeviceNotificationCallback(ref AMDeviceNotificationCallbackInfo callback_info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DeviceEventSink(IntPtr str, IntPtr user);

    internal class AppleMobileDeviceAPI
    {
        
        
        static IntPtr _notificationPtr = IntPtr.Zero;
        
        static AppleMobileDeviceAPI()
        {
            Environment.SetEnvironmentVariable("PATH", GetMobileDeviceDllPathInfo() + Environment.GetEnvironmentVariable("PATH"));
        }

        internal static int AMDeviceNotificationSubscribe(DeviceNotificationCallback callback, uint unused1, uint unused2, uint unused3, ref AMDeviceNotification notification)
        {
            if (_notificationPtr != IntPtr.Zero)
            {
                return 0;
            }

            IntPtr ptr;
            int ret;

            ptr = IntPtr.Zero;
            ret = AMDeviceNotificationSubscribe(callback, unused1, unused2, unused3, out ptr);
            if ((ret == 0) && (ptr != IntPtr.Zero))
            {
                notification = (AMDeviceNotification)Marshal.PtrToStructure(ptr, notification.GetType());
            }
            _notificationPtr = ptr;
            return ret;
        }

        internal static int AMDeviceNotificationUnsubscribe()
        {
            if (_notificationPtr != IntPtr.Zero)
            {
                int ret = AMDeviceNotificationUnsubscribe(_notificationPtr);
                if (ret == 0)
                    _notificationPtr = IntPtr.Zero;
                return ret;
            }
            return 0;
        }

        /// <summary>
        /// Pass in a IntPtr. It will be filled.  Use it with the GetKeyValue shite
        /// </summary>
        /// <param name="conn">Pointer to an afc_connection struct</param>
        /// <param name="info">Pointer to an afc_dictionary struct</param>
        /// <returns>afc_error</returns>
        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCDeviceInfoOpen(IntPtr conn, ref IntPtr buffer);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMSInitialize();

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCKeyValueRead(IntPtr dict, out IntPtr key, out IntPtr value);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCKeyValueClose(IntPtr dict);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int AMDeviceNotificationSubscribe(DeviceNotificationCallback callback, uint unused1, uint unused2, uint unused3, out IntPtr am_device_notification_ptr);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int AMDeviceNotificationUnsubscribe(IntPtr am_device_notification_ptr);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDeviceConnect(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr AMDeviceCopyDeviceIdentifier(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDeviceDisconnect(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDeviceIsPaired(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDevicePair(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDeviceValidatePairing(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDeviceStartSession(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDeviceStopSession(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDeviceGetConnectionID(IntPtr device);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCDirectoryOpen(IntPtr conn, string path, ref IntPtr dir);

        public static int AFCDirectoryRead(IntPtr conn, IntPtr dir, ref string buffer)
        {
            IntPtr ptr;
            int ret;

            ptr = IntPtr.Zero;
            ret = AFCDirectoryRead(conn, dir, ref ptr);
            if ((ret == 0) && (ptr != IntPtr.Zero))
            {
                buffer = Marshal.PtrToStringAnsi(ptr);
            }
            else
            {
                buffer = null;
            }
            return ret;
        }
        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int AFCDirectoryRead(IntPtr conn, IntPtr dir, ref IntPtr dirent);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCDirectoryClose(IntPtr conn, IntPtr dir);

        public static int AMDeviceStartService(IntPtr device, string service_name, ref IntPtr serviceHandle, IntPtr unknown)
        {
            return AMDeviceStartService(device, StringToCFString(service_name), ref serviceHandle, unknown);
        }
        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int AMDeviceStartService(IntPtr device, IntPtr service_name, ref IntPtr handle, IntPtr unknown);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCConnectionOpen(IntPtr handle, uint io_timeout, ref IntPtr conn);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCConnectionClose(IntPtr handle);

        public static string AMDeviceCopyValue(IntPtr device, uint unknown, string name)
        {
            IntPtr result;
            IntPtr cfstring;

            cfstring = StringToCFString(name);

            result = AMDeviceCopyValue_Int(device, unknown, cfstring);
            if (result != IntPtr.Zero)
            {
                byte length;

                length = Marshal.ReadByte(result, 8);
                if (length > 0)
                {
                    return Marshal.PtrToStringAnsi(new IntPtr(result.ToInt64() + 9), length);
                }
                else
                {
                    return String.Empty;
                }
            }
            return String.Empty;
        }

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDObserveNotification(IntPtr conn, IntPtr notification);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDListenForNotifications(IntPtr conn, DeviceEventSink callback, IntPtr userdata);


        [DllImport("iTunesMobileDevice.dll", EntryPoint = "AMDeviceCopyValue", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr AMDeviceCopyValue_Int(IntPtr device, uint unknown, IntPtr cfstring);

        [DllImport("iTunesMobileDevice.dll", EntryPoint = "AMDPostNotification", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMDPostNotification(IntPtr ptr, IntPtr text, uint unknown);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AMSBeginSync(uint unknown, IntPtr device, uint unk2, uint unk3);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCRemovePath(IntPtr conn, string path);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCRenamePath(IntPtr conn, string old_path, string new_path);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefOpen(IntPtr conn, string path, int mode, int unknown, out long handle);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefLock(IntPtr conn, Int64 handle);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefUnlock(IntPtr conn, Int64 handle);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileInfoOpen(IntPtr conn, string path, out IntPtr handle);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefClose(IntPtr conn, Int64 handle);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefRead(IntPtr conn, Int64 handle, byte[] buffer, ref uint len);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefWrite(IntPtr conn, Int64 handle, byte[] buffer, uint len);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFlushData(IntPtr conn, Int64 handle);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefSeek(IntPtr conn, Int64 handle, long position, long origin);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefTell(IntPtr conn, Int64 handle, ref long position);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCFileRefSetFileSize(IntPtr conn, Int64 handle, long size);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AFCDirectoryCreate(IntPtr conn, string path);


        public static IntPtr StringToCFString(string value)
        {
            //byte[] b;
            //b = new byte[value.Length + 10];
            //b[4] = 0x8c;
            //b[5] = 07;
            //b[6] = 01;
            //b[8] = (byte)value.Length;
            //Encoding.ASCII.GetBytes(value, 0, value.Length, b, 9);
            //return b;

            byte[] bytes = new byte[value.Length + 1];
            Encoding.ASCII.GetBytes(value, 0, value.Length, bytes, 0);

            return __CFStringMakeConstantString(bytes);
        }

        [DllImport("CoreFoundation.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr __CFStringMakeConstantString(byte[] s);


        public static string CFStringRefToString(IntPtr stringRef)
        {
            return Marshal.PtrToStringAnsi(new IntPtr(stringRef.ToInt64() + 9));
        }

        internal static Dictionary<string, string> ReadDictionary(IntPtr dictPtr)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            IntPtr keyPtr;
            IntPtr valPtr;
            string key;
            string value;

            try
            {
                while ((AppleMobileDeviceAPI.AFCKeyValueRead(dictPtr, out keyPtr, out valPtr) == 0) && (key = Marshal.PtrToStringAnsi(keyPtr)) != null && (value = Marshal.PtrToStringAnsi(valPtr)) != null)
                {
                    dictionary.Add(key, value);
                }
            }
            catch (AccessViolationException) { }
            finally
            {
                AppleMobileDeviceAPI.AFCKeyValueClose(dictPtr);
            }

            return dictionary;
        }

        public static string GetMobileDeviceDllPathInfo()
        {

            string dllPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Apple Inc.\Apple Mobile Device Support\Shared", "iTunesMobileDeviceDLL", null);
            string appSupportPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Apple Inc.\Apple Application Support", "InstallDir", null);

            if (dllPath != null && File.Exists(dllPath))
                return Path.GetDirectoryName(dllPath) + ";" + appSupportPath + ";";

            string commonProgramFiles = null;

            if (Is64Bit())
            {
                commonProgramFiles = Environment.GetEnvironmentVariable("CommonProgramFiles(x86)");
                if (commonProgramFiles == null) commonProgramFiles = @"C:\Program Files (x86)\Common Files";
            }
            else
            {
                commonProgramFiles = Environment.GetEnvironmentVariable("CommonProgramFiles");
                if (commonProgramFiles == null) commonProgramFiles = @"C:\Program Files\Common Files";
            }

            dllPath = Path.Combine(commonProgramFiles, "Apple\\Mobile Device Support\\bin\\iTunesMobileDevice.dll");
            if (File.Exists(dllPath))
                return Path.GetDirectoryName(dllPath);

            dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "iTunesMobileDevice.dll");
            if (File.Exists(dllPath))
                return Path.GetDirectoryName(dllPath);

            return null;
        }

        //x64 bit detection
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

        public static bool Is64Bit()
        {
            try
            {
                bool retVal;
                IsWow64Process(System.Diagnostics.Process.GetCurrentProcess().Handle, out retVal);
                return retVal;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("IsWow64Process exception: " + ex);
                return false;
            }
        }
    }
}