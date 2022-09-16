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
    /// Thrown if artwork is added to an iPod without an ArtworkDB
    /// </summary>
    public class ArtworkDBNotFoundException : BaseSharePodLibException
    {
        
        public ArtworkDBNotFoundException()
            : base("iPod ArtworkDB not found. You cannot add or remove artwork.")
        {
            Category = "Artwork Problem";

        }
    }
}
