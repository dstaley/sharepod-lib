/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.IPodDevice.FileSystems;
using IPhoneConnector;

namespace SharePodLib
{
	public class StandardIPodDisconnectEventArgs : EventArgs
	{
		public string DriveLetter;
	}

	public class IPhoneDisconnectEventArgs : EventArgs
	{
		public IPhone Device;
	}

    /// <summary>
    /// Contains static methods and events for receiving iPod device connection/disconnection events.
    /// </summary>
    public static class Device
    {
        
        /// <summary>
        /// Delegate used to notify of iPod connection.
        /// </summary>
        /// <param name="iPod"></param>
        public delegate void IPodConnectedHandler(IPod iPod);
        /// <summary>
        /// Delegate used to notify of removable device disconnection.
        /// </summary>
        /// <param name="driveLetter"></param>
        public delegate void IPodDisconnectedHandler(EventArgs args);

        private static DeviceChangeHook _deviceChangeHook;

        /// <summary>
        /// Fires when an iPod is connected. See ListenForDeviceChanges()
        /// </summary>
        public static event IPodConnectedHandler IPodConnected;
        /// <summary>
        /// Fires when any removable drive is disconnected. You must check if the drive letter is the same as the drive letter of your IPod object.
        /// </summary>
        public static event IPodDisconnectedHandler IPodDisconnected;


        /// <summary>
        /// Tell SharePodLib to listen for iPod devices being connected.  The iPodConnected event will only fire once this is called.
        /// </summary>
        /// <param name="hWnd">Window handle of the calling application</param>
        public static void ListenForDeviceChanges(IntPtr hWnd)
        {
            if (_deviceChangeHook == null)
            {
                _deviceChangeHook = new DeviceChangeHook(hWnd);
                _deviceChangeHook.DeviceArrived += DeviceChangeHook_DeviceArrived;
                _deviceChangeHook.DeviceRemoved += DeviceChangeHook_DeviceRemoved;
                _deviceChangeHook.Install();

				IPhoneConnectionListener.Connected += new EventHandler(IPhoneConnectionListener_Connected);
				IPhoneConnectionListener.Disconnected += new EventHandler(IPhoneConnectionListener_Disconnected);
                IPhoneConnectionListener.ListenForEvents(FileSystemAccess.Standard);
            }
        }


        /// <summary>
        /// Tell SharePodLib to stop listening for iPod devices being connected. 
        /// </summary>
        public static void StopListeningForDeviceChanges()
        {
            if (_deviceChangeHook != null)
            {
                _deviceChangeHook.Uninstall();
                _deviceChangeHook.DeviceArrived -= DeviceChangeHook_DeviceArrived;
                _deviceChangeHook.DeviceRemoved -= DeviceChangeHook_DeviceRemoved;
                _deviceChangeHook = null;

                IPhoneConnectionListener.Connected -= IPhoneConnectionListener_Connected;
                IPhoneConnectionListener.Disconnected -= IPhoneConnectionListener_Disconnected;

				IPhoneConnectionListener.StopListeningForEvents();
            }
        }

        
        private static void DeviceChangeHook_DeviceArrived(object sender, DeviceChangeEventArgs dce)
        {
            if (IPodConnected == null)
                return;

            DeviceFileSystem deviceFS = IPod.GetDeviceFileSystemForDrive(dce.DriveInfo);
            if (deviceFS == null)
                return;
            IPodConnected(new IPod(deviceFS, IPodLoadAction.NoSync));
        }

        static void DeviceChangeHook_DeviceRemoved(object sender, DeviceChangeEventArgs dce)
        {
            if (IPodDisconnected != null)
            {
				IPodDisconnected(new StandardIPodDisconnectEventArgs { DriveLetter = dce.Drive + "\\" });
            }
        }

		static void IPhoneConnectionListener_Disconnected(object sender, EventArgs e)
		{
			if (IPodDisconnected != null)
			{
				IPodDisconnected(new IPhoneDisconnectEventArgs { Device = (IPhone)sender });
			}
		}

		static void IPhoneConnectionListener_Connected(object sender, EventArgs e)
		{
			if (IPodConnected != null)
			{
                IPhoneFileSystem fs = (IPhoneFileSystem)SharePodLib.RegisteredFileSystems.Find(a => a is IPhoneFileSystem);
				
                IPhoneFileSystem fs2 = fs.Clone();
				fs2.Phone = (IPhone)sender;
				IPodConnected(new IPod(fs2, IPodLoadAction.NoSync));
			}
		}
    }
}
