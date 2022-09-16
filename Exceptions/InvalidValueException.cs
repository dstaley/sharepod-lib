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
	/// Thrown when an invalid value is specified for a track or playlist property
	/// </summary>
    public class InvalidValueException : BaseSharePodLibException
    {
        public InvalidValueException(string message)
            : base(message)
        {
            Category = "Invalid Value Specified";
        }


    }
}
