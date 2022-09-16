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
	/// Wraps a .NET DateTime and an iPod-format timestamp.
	/// </summary>
    public class IPodDateTime : IComparable
    {
        uint _timeStamp;
        DateTime _dateTime;

        public IPodDateTime(uint timestamp)
        {
            _dateTime = Helpers.GetDateTimeFromTimeStamp(timestamp);
            _timeStamp = timestamp;
        }

        public IPodDateTime(DateTime date)
        {
            _dateTime = date;
            _timeStamp = Helpers.GetTimeStampFromDate(_dateTime);
        }

        public DateTime DateTime
        {
            get { return _dateTime; }
        }

        public uint TimeStamp
        {
            get { return _timeStamp; }
        }

        public override string ToString()
        {
            if (_timeStamp == 0)
                return String.Empty;
            else
                return _dateTime.ToString();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return _timeStamp.CompareTo(((IPodDateTime)obj).TimeStamp);
        }

        #endregion
    }
}
