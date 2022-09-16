/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers;

namespace SharePodLib.DataTypes
{
	/// <summary>
	/// Wraps a file size in bytes and a human-readable string describing the size.
	/// </summary>
    public class IPodTrackSize : IComparable
    {

        uint _trackSize;
        string _trackSizeMB;
        public IPodTrackSize(uint trackSizeInBytes)
        {
            _trackSize = trackSizeInBytes;
            _trackSizeMB = Helpers.GetFileSizeString(trackSizeInBytes, 1);
        }

        public uint ByteCount
        {
            get { return _trackSize; }
        }

        public override string ToString()
        {
            return _trackSizeMB;
        }


        #region IComparable Members

        public int CompareTo(object obj)
        {
            return _trackSize.CompareTo(((IPodTrackSize)obj).ByteCount);
        }

        #endregion
    }
}
