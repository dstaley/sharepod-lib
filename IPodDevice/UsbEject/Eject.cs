using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UsbEject.Library
{
    public static class Util
    {
        /// <summary>
        /// Eject a removable drive. Windows will display the standard 'removable device cannot be removed' if eject fails.
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <returns></returns>
        public static bool Eject(string driveLetter)
        {
            DriveInfo di = new DriveInfo(driveLetter.Substring(0,1));

            VolumeDeviceClass vc = new VolumeDeviceClass();
            foreach (Volume volume in vc.Devices)
            {
                if (volume.LogicalDrive == null)
                    continue;

                if (!di.Name.ToLower().StartsWith(volume.LogicalDrive.ToLower()))
                    continue;

                foreach (Device disk in volume.Disks)
                {
                    if (disk.IsUsb || (disk.Capabilities & DeviceCapabilities.Removable) == DeviceCapabilities.Removable)
                    {
                        string s = disk.Eject(true);
                        return s == null;
                    }
                }

            }
            return false;
        }

    }
}
