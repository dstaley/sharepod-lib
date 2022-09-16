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
    public class UnsupportedArtworkFormatException : BaseSharePodLibException
    {
        public UnsupportedArtworkFormatException(uint imageSize)
            : base("The artwork format (size " + imageSize.ToString() + ") is not currently supported.")
        {
            Category = "Unsupported Artwork format.";
        }
    }
}
