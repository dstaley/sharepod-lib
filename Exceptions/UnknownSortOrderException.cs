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
	/// Thrown if a Playlist's SortOrder field is not a value enumerated by SharePodLib.
	/// </summary>
    public class UnknownSortOrderException : BaseSharePodLibException
    {
        public UnknownSortOrderException(string message)
            : base(message)
        {
            Category = "The playlist's Sort Order value is not supported";
        }


    }
}
