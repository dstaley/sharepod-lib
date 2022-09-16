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
    /// Thrown when trying to add album artwork and the iPod reported no supported artwork.
    /// </summary>
    public class NoSupportedArtworkException : BaseSharePodLibException
    {
        public NoSupportedArtworkException()
            : base("No supported artwork formats were detected")
        {
            Category = "Album art could not be added";
        }


    }
}
