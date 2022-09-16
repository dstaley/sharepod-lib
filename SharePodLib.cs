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
using System.Reflection;
using SharePodLib.IPodDevice.FileSystems;
using IPhoneConnector;
using System.Diagnostics;

namespace SharePodLib
{
	/// <summary>
	/// Contains SharePodLib-specific methods.
	/// </summary>
    public static class SharePodLib
    {
        private static string _licenceName;
		private static string _licenceKey;

        private static List<DeviceFileSystem> _registeredFileSystems = new List<DeviceFileSystem>();

        static SharePodLib()
        {
            StandardFileSystem iPodProfile = new StandardFileSystem("IPod", @"ipod_control\iTunes\", @"ipod_control\", @"ipod_control\Artwork\", @"Photos\");
            iPodProfile.ParseDbFilesLocally = true;
            _registeredFileSystems.Add(iPodProfile);

            if (IPhone.IsMobileDeviceDllAvailable)
            {
                Trace.WriteLine("iPhone support enabled");

                //hack: remove old dlls which will cause problems now itunesmobiledevice is trying to use them too
                SharePodLib.RemoveUnpackedEmbeddedResource("icudt40.dll", true);
                SharePodLib.RemoveUnpackedEmbeddedResource("icuin40.dll", true);
                SharePodLib.RemoveUnpackedEmbeddedResource("icuuc40.dll", true);

                IPhoneFileSystem iPhoneProfile = new IPhoneFileSystem("iPhone", @"iTunes_Control/iTunes/", @"iTunes_Control/", @"iTunes_Control/Artwork/", @"Photos/");
                iPhoneProfile.ParseDbFilesLocally = true;
                _registeredFileSystems.Add(iPhoneProfile);
            }
            else
            {
                Trace.WriteLine("iPhone support disabled - AppleMobileDevice.dll not found");
            }
            try
            {
                // Setup PATH variable to include %TEMP%\SharePodLib\bin.  This is where native dlls get output to.
                string tempPath = Path.Combine(Path.GetTempPath(), "SharePodLib\\bin");
                Environment.SetEnvironmentVariable("PATH", tempPath + ";" + Environment.GetEnvironmentVariable("PATH"));
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception setting PATH - " + ex.Message);
            }
        }
        

		/// <summary>
		/// If you have a licence key, call this method, passing in your name and key to disable the splash screen.
		/// </summary>
        /// <param name="name">Your registration name</param>
		/// <param name="key">Your key</param>
		public static void SetLicence(string name, string key)
		{
            _licenceName = name;
			_licenceKey = key;
		}

		internal static bool IsLicenced()
		{
            #if (SHAREPODLIB_SOURCE_LICENSE)
                return true;
            #endif

			if (_licenceKey == null)
				return false;

			return SerialNumberGenerator.VerifyHash(_licenceName, _licenceKey);
        }

        
        /// <summary>
        /// List of device FileSystems SharePodLib will use when searching for iPods.  This list can be updated dynamically before calling GetConnectediPod().
        /// </summary>
        public static List<DeviceFileSystem> RegisteredFileSystems
        {
            get { return _registeredFileSystems; }
        }



        internal static string UnpackEmbeddedResource(string name, bool toTemp)
        {
            return UnpackEmbeddedResource(name, name, toTemp);
        }
        internal static string UnpackEmbeddedResource(string name, string to, bool toTemp)
        {
            //if file already exists in the application folder, dont unpack it
            string dllPath = Path.Combine(System.Windows.Forms.Application.StartupPath, to);
            if (File.Exists(dllPath))
                return dllPath;
                        
            if (toTemp)
            {
                dllPath = Path.Combine(Path.GetTempPath(), "SharePodLib\\bin");
                if (!Directory.Exists(dllPath)) Directory.CreateDirectory(dllPath);
            }
            else
            {
                dllPath = System.Windows.Forms.Application.StartupPath;
            }
            
            dllPath = Path.Combine(dllPath, to);
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(String.Format("SharePodLib.Resources.{0}", name));
            if (!File.Exists(dllPath) || new FileInfo(dllPath).Length != resourceStream.Length)
            {
                using (FileStream stream = new FileStream(dllPath, FileMode.Create))
                {
                    byte[] buffer = new byte[65536];
                    int chunkLength;
                    while ((chunkLength = resourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        stream.Write(buffer, 0, chunkLength);
                }
            }
            resourceStream.Close();
            return dllPath;
        }

        internal static void RemoveUnpackedEmbeddedResource(string name, bool isTemp)
        {
            if (isTemp)
            {
                string dllPath = Path.Combine(Path.GetTempPath(), "SharePodLib\\bin\\" + name);
                if (File.Exists(dllPath)) File.Delete(dllPath);
            }
            else
            {
                string dllPath = Path.Combine(System.Windows.Forms.Application.StartupPath, name);
                if (File.Exists(dllPath)) File.Delete(dllPath);
            }
        }
    }
}
