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
	/// Thrown when adding tracks and adding new artwork to the iPod.  SharePodLib will make sure there will be at least 10Mb of free space on the 
	/// iPod after copying the track.
	/// </summary>
    public class OutOfDiskSpaceException : BaseSharePodLibException
    {
        public OutOfDiskSpaceException(string message)
            : base(message)
        {
            Category = "The iPod cannot store any more files";
        }


    }
}
