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
	/// Thrown when (for example) trying to add/remove tracks from a Smart Playlist.
	/// </summary>
    public class OperationNotAllowedException : BaseSharePodLibException
    {
        public OperationNotAllowedException(string message)
            : base(message)
        {
            Category = "Operation not allowed";
        }


    }
}
