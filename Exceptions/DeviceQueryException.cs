/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib.Exceptions
{
    /// <summary>
    /// Thrown when the device is queried and fails to obtain a handle to the device.
    /// </summary>
    public class DeviceQueryException : BaseSharePodLibException
    {
        public DeviceQueryException()
            : base(
             "The first time SharePod runs, it needs administrator rights to query your iPod.\r\n\r\n" + 
                "Please login as a user with administrator rights then re-run SharePod.\r\n\r\n" + 
                "Once you have successfully run SharePod once, you no longer need administrator rights."
            )
        {
            Category = "SharePod requires administrator rights.";
        }
    }
}
