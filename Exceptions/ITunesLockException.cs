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
    /// Thrown when an iTunesLock file exists on the iPod.  This usually means iTunes has locked the iPod
    /// and is currently syncing.
    /// </summary>
    public class ITunesLockException : BaseSharePodLibException
    {
        public ITunesLockException(string lockFilePath)
            : base(
            "iTunes has locked the iPod database. Please wait for iTunes to finish synchronizing. \r\n" +
            "If iTunes is not running, delete the '" + lockFilePath + "' file."
            )
        {
            Category = "iPod cannot be opened";
        }
    }
}
