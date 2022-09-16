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
	/// Thrown when an invalid drive is specified to a call to IPod.GetIPodByDrive()
	/// </summary>
    public class InvalidIPodDriveException : BaseSharePodLibException
    {
        public InvalidIPodDriveException(string message)
            : base(message)
        {
            Category = "iPod Not Found";
        }
    }
}
