// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections.Generic;
using System.Text;

namespace UsbEject.Library
{
    /// <summary>
    /// The device class for disk devices.
    /// </summary>
    internal class DiskDeviceClass : DeviceClass
    {
        /// <summary>
        /// Initializes a new instance of the DiskDeviceClass class.
        /// </summary>
        public DiskDeviceClass()
            :base(new Guid(Native.GUID_DEVINTERFACE_DISK))
        {
        }

        public override List<Device> Devices
        {
            get
            {
                ROOT.CIMV2.Win32.DiskDrive.DiskDriveCollection wmiDrives = ROOT.CIMV2.Win32.DiskDrive.GetInstances();
                List<Device> devices = base.Devices;
                foreach (Device d in devices)
                {

                    foreach (ROOT.CIMV2.Win32.DiskDrive dd in wmiDrives)
                    {
                        if (dd.PNPDeviceID != d.PNPDeviceID)
                            continue;

                        d.Index = (int)dd.Index;
                    }
                    
                }
                return devices;
            }
        }

    }
}
