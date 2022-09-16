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
	/// Thrown when trying to add a track to the iPod which already exists. (Same Title, Artist, Album, TrackNumber)
	/// </summary>
    public class TrackAlreadyExistsException : BaseSharePodLibException
    {
        private Track _existingTrack;

        
        public TrackAlreadyExistsException(string message, Track existingTrack)
            : base(message)
        {
            Category = "This track already exists on your iPod";
            _existingTrack = existingTrack;
        }

        /// <summary>
        /// Track that is on the iPod already
        /// </summary>
        public Track ExistingTrack
        {
            get { return _existingTrack; }
        }
    }
}
