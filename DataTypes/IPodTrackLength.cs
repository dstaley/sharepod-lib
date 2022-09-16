/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers;
using System.Collections;

namespace SharePodLib.DataTypes
{
	/// <summary>
	/// Wraps a track length in milliseconds and a human-readable hh:mm:ss string
	/// </summary>
    public class IPodTrackLength : IComparable
    {
        UInt32 _trackLengthMSecs;
        string _trackLengthMinsSecs;

        public IPodTrackLength(UInt32 trackLengthInMSecs)
        {
            _trackLengthMSecs = trackLengthInMSecs;
            _trackLengthMinsSecs = Helpers.GetTimeString(this.Seconds);
        }

        public IPodTrackLength(int trackLengthInMSecs)
            : this((UInt32)trackLengthInMSecs)
        {
        }

        public UInt32 Seconds
        {
            get { return _trackLengthMSecs / 1000; }
        }

        public UInt32 MilliSeconds
        {
            get { return _trackLengthMSecs; }
        }

        public override string ToString()
        {
            return _trackLengthMinsSecs;
        }


        #region IComparable Members

        public int CompareTo(object obj)
        {
            return _trackLengthMSecs.CompareTo(((IPodTrackLength)obj).MilliSeconds);
        }

        #endregion
    }
}
