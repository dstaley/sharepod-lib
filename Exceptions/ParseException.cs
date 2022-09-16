/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 


using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Exceptions
{
	/// <summary>
	/// Thrown when the iPod database format could not be recognized or validated correctly by SharePod.
	/// This could occur if the iTunes file format changes or a 3rd party application has written the 
	/// database in a different way to iTunes.
	/// </summary>
    public class ParseException : BaseSharePodLibException
    {
        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
            Category = "Your iPod database could not be read";
        }
    }
}
